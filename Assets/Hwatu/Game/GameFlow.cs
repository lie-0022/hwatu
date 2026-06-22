using System;
using UnityEngine;
using Hwatu.Core.Cards;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Game
{
    /// <summary>런 진행 페이즈(화면).</summary>
    public enum RunPhase
    {
        MainMenu,
        CharacterSelect,
        Map,
        Combat,
        Reward,
        BossReward,
        GameOver,
        Victory,
        Event,
        Rest,
        Shop,
        NeowBoon
    }

    /// <summary>
    /// 한 판 루프의 중추 상태머신. RunState를 보유하고 페이즈 전환을 관리한다(02-run-state.md).
    /// 화면 View들은 <see cref="OnPhaseChanged"/>를 구독해 자기 화면을 켜고 끈다.
    /// </summary>
    public sealed class GameFlow : MonoBehaviour
    {
        [SerializeField] private ulong _seed = 20260622UL;

        private const int FinalAct = 3;
        private readonly MapGenerator _mapGen = new MapGenerator();

        public RunState Run { get; private set; }
        public RunPhase Phase { get; private set; }
        public EventData CurrentEvent { get; private set; }   // 이벤트 노드 진입 시 채워짐(RunUI가 읽음)
        public ShopStock CurrentShop { get; private set; }   // 상점 노드 진입 시 생성되는 매물 묶음(카드·유물·포션)

        /// <summary>페이즈 전환 시 발생(현재 페이즈 전달).</summary>
        public event Action<RunPhase> OnPhaseChanged;

        private void Start()
        {
            SetPhase(RunPhase.MainMenu);
        }

        /// <summary>메인 메뉴에서 캐릭터 선택으로.</summary>
        public void GoToCharacterSelect()
        {
            SetPhase(RunPhase.CharacterSelect);
        }

        /// <summary>캐릭터를 골라 새 런 시작 → 1막 맵 생성 → 맵 화면.</summary>
        public void StartNewRun(CharacterData character)
        {
            Run = new RunState(character, _seed);
            GenerateActMap();
            SetPhase(RunPhase.NeowBoon);
        }

        /// <summary>런 시작 보너스(Neow) 적용 후 맵으로. 0=최대HP+8, 1=골드+100, 2=유물 1개.</summary>
        public void ApplyNeowBoon(int choice)
        {
            switch (choice)
            {
                case 0: Run.MaxHp += 8; Run.Hp += 8; break;
                case 1: Run.Gold += 100; break;
                case 2:
                    IRandom rng = new RngStreams(Run.Seed).ForStream("neow_relic");
                    RelicData relic = RewardSystem.RollRelicReward(rng, RelicContent.AllRelics());
                    if (relic != null) { Run.AddRelic(relic); }
                    break;
                default: Run.UpgradeCard(0); break;   // 첫 카드 강화
            }
            SetPhase(RunPhase.Map);
        }

        /// <summary>이벤트 선택지 index를 적용하고 맵으로 복귀.</summary>
        public void OnEventChoice(int index)
        {
            if (CurrentEvent != null && index >= 0 && index < CurrentEvent.Choices.Count)
            {
                CurrentEvent.Choices[index].Apply(Run);
            }
            SetPhase(RunPhase.Map);
        }

        /// <summary>휴식처 선택: 0=회복(최대HP 30%), 1=덱 첫 카드 강화. 후 맵 복귀.</summary>
        public void OnRest(int choice)
        {
            if (choice == 0)
            {
                Run.Heal(Run.MaxHp * 30 / 100);
            }
            else if (choice == 1 && Run.Deck.Count > 0)
            {
                Run.UpgradeCard(0);
            }
            SetPhase(RunPhase.Map);
        }

        /// <summary>상점 매물 1칸을 구매한다(골드·포션 슬롯 충분 시). 상점은 유지된다(여러 개 구매 가능).</summary>
        public bool BuyShopItem(int index)
        {
            if (CurrentShop == null || index < 0 || index >= CurrentShop.Items.Count) { return false; }
            ShopItem it = CurrentShop.Items[index];
            if (it.Sold) { return false; }
            if (it.Kind == ShopItemKind.Potion && Run.Potions.Count >= RunState.MaxPotions) { return false; }
            if (!Run.TrySpend(it.Price)) { return false; }
            switch (it.Kind)
            {
                case ShopItemKind.Card: Run.AddCard(it.Card); break;
                case ShopItemKind.Relic: Run.AddRelic(it.Relic); break;
                case ShopItemKind.Potion: Run.AddPotion(it.Potion); break;
            }
            it.Sold = true;
            return true;
        }

        /// <summary>상점 나가기.</summary>
        public void OnShopLeave()
        {
            SetPhase(RunPhase.Map);
        }

        /// <summary>현재 액트의 맵을 생성한다(맵 전용 RNG 스트림).</summary>
        /// <summary>휴식에서 선택한 덱 카드를 강화하고 맵으로 돌아간다.</summary>
        public void RestUpgradeCard(int index)
        {
            Run.UpgradeCard(index);
            SetPhase(RunPhase.Map);
        }

        /// <summary>휴식에서 선택한 덱 카드를 예리 인챈트하고 맵으로(STS2식).</summary>
        public void RestEnchantCard(int index)
        {
            Run.EnchantCard(index, "sharp");
            SetPhase(RunPhase.Map);
        }

        /// <summary>상점에서 덱의 특정 카드를 제거(선택식, 상점당 1회, 골드 소모). 상점은 유지.</summary>
        public bool ShopRemoveCardAt(int index)
        {
            if (CurrentShop != null && CurrentShop.RemoveUsed) { return false; }
            int cost = CurrentShop != null ? CurrentShop.RemoveCost : 75;
            if (index >= 0 && index < Run.Deck.Count && Run.TrySpend(cost))
            {
                if (CurrentShop != null) { CurrentShop.RemoveUsed = true; }
                return Run.RemoveCardAt(index);
            }
            return false;
        }

        /// <summary>상점 매물 전체를 골드 15로 새로 뽑는다(STS reroll).</summary>
        public void ShopReroll()
        {
            if (Run.TrySpend(15))
            {
                IRandom rng = new RngStreams(Run.Seed).ForStream("shop_reroll_" + Run.Gold);
                CurrentShop = ShopStock.Generate(Run, rng);
            }
        }

        public void GenerateActMap()
        {
            IRandom rng = new RngStreams(Run.Seed).ForStream("map_act" + Run.Act);
            Run.Map = _mapGen.Generate(rng, Run.Act);
            Run.CurrentNodeId = -1;
        }

        /// <summary>맵에서 노드를 선택해 진입.</summary>
        public void EnterNode(MapNode node)
        {
            Run.CurrentNodeId = node.Id;
            switch (node.Type)
            {
                case NodeType.Combat:
                case NodeType.Elite:
                case NodeType.Boss:
                    SetPhase(RunPhase.Combat);
                    break;
                case NodeType.Rest:
                    // 휴식: 회복/강화 선택 화면으로(적용은 OnRest).
                    SetPhase(RunPhase.Rest);
                    break;
                case NodeType.Treasure:
                    // 보물: 유물 1개 자동 획득(중복 제외는 후속).
                    {
                        IRandom relicRng = new RngStreams(Run.Seed).ForStream("treasure_" + node.Id);
                        var relic = Hwatu.Core.Run.RewardSystem.RollRelicReward(relicRng, Hwatu.Core.Content.RelicContent.AllRelics());
                        if (relic != null)
                        {
                            Run.AddRelic(relic);
                        }
                    }
                    SetPhase(RunPhase.Map);
                    break;
                case NodeType.Event:
                    // 이벤트: 시드로 1개 골라 선택 화면으로(선택지 적용은 OnEventChoice).
                    {
                        IRandom evRng = new RngStreams(Run.Seed).ForStream("event_" + node.Id);
                        CurrentEvent = Hwatu.Core.Content.EventContent.Pick(evRng);
                    }
                    SetPhase(RunPhase.Event);
                    break;
                case NodeType.Shop:
                    // 상점: 매물 카드 1장(캐릭터 풀에서) + 가격. 구매/나가기는 RunUI.
                    {
                        IRandom shopRng = new RngStreams(Run.Seed).ForStream("shop_" + node.Id);
                        CurrentShop = ShopStock.Generate(Run, shopRng);
                    }
                    SetPhase(RunPhase.Shop);
                    break;
                default:
                    SetPhase(RunPhase.Map);
                    break;
            }
        }

        /// <summary>전투 종료 콜백(승/패).</summary>
        public void OnCombatEnded(bool won)
        {
            if (!won)
            {
                SetPhase(RunPhase.GameOver);
                return;
            }
            // 보스든 일반이든 보상(보스는 Rare). 보스 후 액트 진행/승리는 OnRewardDone에서 처리.
            SetPhase(RunPhase.Reward);
        }

        /// <summary>보상 화면을 마치고 — 보스였으면 액트 진행/승리, 아니면 맵 복귀.</summary>
        public void OnRewardDone()
        {
            MapNode node = Run.Map?.GetNode(Run.CurrentNodeId);
            if (node != null && node.Type == NodeType.Boss)
            {
                if (Run.Act >= FinalAct)
                {
                    SetPhase(RunPhase.Victory);
                    return;
                }
                AdvanceAct();
            }
            SetPhase(RunPhase.Map);
        }

        // 다음 액트: Act++, 부족분 HP 회복(STS), 새 맵 생성.
        private void AdvanceAct()
        {
            Run.Act++;
            Run.Heal(Run.MaxHp);   // 부족분 전부(최대치까지)
            GenerateActMap();
        }

        /// <summary>현재 노드가 보스인가(전투 화면이 보스 적을 쓰도록).</summary>
        public bool CurrentNodeIsBoss()
        {
            MapNode node = Run.Map?.GetNode(Run.CurrentNodeId);
            return node != null && node.Type == NodeType.Boss;
        }

        /// <summary>런 종료(결과 화면)에서 메인 메뉴로 복귀.</summary>
        public void BackToMenu()
        {
            Run = null;
            SetPhase(RunPhase.MainMenu);
        }

        private void SetPhase(RunPhase phase)
        {
            Phase = phase;
            OnPhaseChanged?.Invoke(phase);
        }
    }
}

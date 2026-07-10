using System.Collections.Generic;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Core.Jokbo
{
    /// <summary>족보 런 진행 페이즈.</summary>
    public enum JokboRunPhase
    {
        Map, Combat, Reward, Rest, Shop, Event, GameOver, Victory
    }

    /// <summary>
    /// 족보 런 상태머신(J5, POCO — 헤드리스 테스트 가능). 기존 GameFlow의 로직판을 UI 비의존으로 옮긴 것.
    /// 맵 생성·노드 진입·전투 시작·보상·휴식·액트 진행을 관리한다. MonoBehaviour 래퍼가 이걸 구동한다.
    /// 맵·인카운터·유물·보상 시스템은 기존 것을 그대로 재사용.
    /// </summary>
    public sealed class JokboRunFlow
    {
        private const int FinalAct = 3;
        private readonly MapGenerator _mapGen = new MapGenerator();

        public JokboRunState Run { get; private set; }
        public JokboRunPhase Phase { get; private set; }
        /// <summary>전투 노드 진입 시 생성되는 족보 전투 엔진(UI/시뮬이 구동).</summary>
        public JokboCombatEngine Combat { get; private set; }
        /// <summary>전투 승리 후 카드 보상 3택(TakeReward로 소비).</summary>
        public List<HwatuCardData> RewardCards { get; private set; }

        /// <summary>새 런 시작 → 1막 맵 → Map 페이즈.</summary>
        public void StartNewRun(ulong seed, int startHp = 70)
        {
            Run = new JokboRunState(seed, startHp);
            GenerateMap();
            Phase = JokboRunPhase.Map;
        }

        private void GenerateMap()
        {
            IRandom rng = new RngStreams(Run.Seed).ForStream("jokbo_map_act" + Run.Act);
            Run.Map = _mapGen.Generate(rng, Run.Act);
            Run.CurrentNodeId = -1;
        }

        /// <summary>맵 노드 선택 → 타입별 처리.</summary>
        public void EnterNode(MapNode node)
        {
            Run.CurrentNodeId = node.Id;
            switch (node.Type)
            {
                case NodeType.Combat:
                case NodeType.Elite:
                case NodeType.Boss:
                    StartCombat(node);
                    break;
                case NodeType.Rest:
                    Phase = JokboRunPhase.Rest;
                    break;
                case NodeType.Treasure:
                {
                    IRandom rng = new RngStreams(Run.Seed).ForStream("jokbo_treasure_" + node.Id);
                    RelicData relic = RewardSystem.RollRelicReward(rng, RelicContent.AllRelics());
                    if (relic != null) { Run.AddRelic(relic); }
                    Phase = JokboRunPhase.Map;
                    break;
                }
                case NodeType.Event:
                    Phase = JokboRunPhase.Event;
                    break;
                case NodeType.Shop:
                    Phase = JokboRunPhase.Shop;
                    break;
                default:
                    Phase = JokboRunPhase.Map;
                    break;
            }
        }

        private void StartCombat(MapNode node)
        {
            List<EnemyData> enemies = EnemyContent.BuildEncounter(node.Type, node.EncounterId, Run.Act);
            ulong seed = Run.Seed + (ulong)(Run.CurrentNodeId + 1);
            Combat = JokboCombatFactory.Create(Run.Deck, enemies, seed, Run.MaxHp, Run.Hp, Run.Relics);
            Phase = JokboRunPhase.Combat;
        }

        /// <summary>전투 종료 콜백. 전투 후 HP를 런에 반영하고, 승리면 보상·패배면 게임오버.</summary>
        public void OnCombatEnded(bool won)
        {
            if (Combat != null) { Run.Hp = Combat.State.Player.Hp; }
            if (!won)
            {
                Phase = JokboRunPhase.GameOver;
                return;
            }
            RollReward();
            Phase = JokboRunPhase.Reward;
        }

        private void RollReward()
        {
            MapNode node = Run.Map.GetNode(Run.CurrentNodeId);
            EncounterType enc = node != null && node.Type == NodeType.Boss ? EncounterType.Boss
                : node != null && node.Type == NodeType.Elite ? EncounterType.Elite : EncounterType.Normal;
            IRandom rng = new RngStreams(Run.Seed).ForStream("jokbo_reward_" + Run.CurrentNodeId);
            RewardCards = JokboRewardSystem.RollCardReward(rng, enc);
            Run.Gold += enc == EncounterType.Boss ? 40 : enc == EncounterType.Elite ? 30 : 15;
        }

        /// <summary>보상 카드 선택(index &lt; 0 = 넘기기) → 액트 진행/맵 복귀.</summary>
        public void TakeReward(int index)
        {
            if (RewardCards != null && index >= 0 && index < RewardCards.Count)
            {
                Run.AddCard(RewardCards[index]);
            }
            RewardCards = null;
            AfterReward();
        }

        private void AfterReward()
        {
            MapNode node = Run.Map.GetNode(Run.CurrentNodeId);
            if (node != null && node.Type == NodeType.Boss)
            {
                if (Run.Act >= FinalAct)
                {
                    Phase = JokboRunPhase.Victory;
                    return;
                }
                Run.Act++;
                Run.Heal(Run.MaxHp);
                GenerateMap();
            }
            Phase = JokboRunPhase.Map;
        }

        /// <summary>휴식: 0=회복(최대HP 30%), 1=덱 첫 카드 강화.</summary>
        public void OnRest(int choice)
        {
            if (choice == 0) { Run.Heal(Run.MaxHp * 30 / 100); }
            else if (choice == 1 && Run.Deck.Count > 0) { Run.UpgradeCard(0); }
            Phase = JokboRunPhase.Map;
        }

        /// <summary>상점 나가기.</summary>
        public void LeaveShop()
        {
            Phase = JokboRunPhase.Map;
        }
    }
}

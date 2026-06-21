using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Content;

namespace Hwatu.Core.Run
{
    /// <summary>
    /// 한 런(run)의 진행 상태: 캐릭터·덱·HP·골드·맵·현재 위치·액트. 전투/보상/이동이 이 상태를 갱신한다.
    /// HP는 전투 간 유지(자동 회복 없음 — 휴식/액트 전환에서만 회복).
    /// </summary>
    public sealed class RunState
    {
        public CharacterData Character { get; }
        public List<CardData> Deck { get; }
        public int MaxHp { get; set; }
        public int Hp { get; set; }
        public int Gold { get; set; }
        public ulong Seed { get; }
        public int Act { get; set; }
        public int Ascension { get; set; }
        public MapGraph Map { get; set; }

        /// <summary>현재 맵 위치 노드 id. -1이면 아직 맵에 진입 전(행0 선택 대기).</summary>
        public int CurrentNodeId { get; set; }

        /// <summary>카드 보상 레어도 피티 오프셋(시작 -5, 03-card-reward.md).</summary>
        public int RareOffset { get; set; }

        /// <summary>보유 유물(전투 시작 효과 등).</summary>
        public List<RelicData> Relics { get; }

        /// <summary>보유 포션(최대 <see cref="MaxPotions"/> 슬롯).</summary>
        public List<PotionData> Potions { get; }
        public const int MaxPotions = 3;

        public RunState(CharacterData character, ulong seed)
        {
            Character = character;
            Deck = new List<CardData>(character.StartingDeck);
            MaxHp = character.StartMaxHp;
            Hp = character.StartMaxHp;
            Gold = character.StartGold;
            Seed = seed;
            Act = 1;
            CurrentNodeId = -1;
            RareOffset = -5;
            Relics = new List<RelicData> { RelicContent.Lantern() };
            Potions = new List<PotionData>();
        }

        /// <summary>덱에 카드를 추가(보상 선택).</summary>
        public void AddCard(CardData card)
        {
            Deck.Add(card);
        }

        /// <summary>유물 획득(보상/상점).</summary>
        public void AddRelic(RelicData relic)
        {
            Relics.Add(relic);
        }

        /// <summary>포션 획득(슬롯 여유 시 true; 가득 차면 false, 추가 안 함).</summary>
        public bool AddPotion(PotionData potion)
        {
            if (Potions.Count >= MaxPotions)
            {
                return false;
            }
            Potions.Add(potion);
            return true;
        }

        /// <summary>골드 지불(부족하면 false, 차감 안 함).</summary>
        public bool TrySpend(int gold)
        {
            if (Gold < gold)
            {
                return false;
            }
            Gold -= gold;
            return true;
        }

        /// <summary>덱에서 카드 1장 제거(상점/이벤트). 성공 시 true.</summary>
        public bool RemoveCard(CardData card)
        {
            return Deck.Remove(card);
        }

        /// <summary>덱의 index 카드를 업그레이드(휴식 Smith).</summary>
        public void UpgradeCard(int index)
        {
            if (index >= 0 && index < Deck.Count)
            {
                Deck[index] = Deck[index].Upgrade();
            }
        }

        /// <summary>HP 회복(최대 초과 안 함).</summary>
        public void Heal(int amount)
        {
            Hp = System.Math.Min(MaxHp, Hp + amount);
        }

        public bool IsDead => Hp <= 0;
    }
}

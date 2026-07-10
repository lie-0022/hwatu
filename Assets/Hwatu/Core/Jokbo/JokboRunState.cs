using System.Collections.Generic;
using Hwatu.Core.Run;

namespace Hwatu.Core.Jokbo
{
    /// <summary>
    /// 족보 런 상태(J5) — 기존 RunState의 족보판. 덱만 HwatuCardData로 두고 HP·골드·맵·유물·포션은 미러/재사용.
    /// 캐릭터 레이어(J6) 전이라 공통 시작 덱 14장으로 시작한다. 맵·유물·포션은 기존 타입을 그대로 쓴다.
    /// </summary>
    public sealed class JokboRunState
    {
        public List<HwatuCardData> Deck { get; }
        public int MaxHp { get; set; }
        public int Hp { get; set; }
        public int Gold { get; set; }
        public ulong Seed { get; }
        public int Act { get; set; }
        public int Ascension { get; set; }
        public MapGraph Map { get; set; }
        public int CurrentNodeId { get; set; }
        public List<RelicData> Relics { get; }
        public List<PotionData> Potions { get; }
        public int PotionChance { get; set; }
        public int RareOffset { get; set; }
        public const int MaxPotions = 3;

        public JokboRunState(ulong seed, int startHp = 70, int startGold = 99)
        {
            Seed = seed;
            MaxHp = startHp;
            Hp = startHp;
            Gold = startGold;
            Act = 1;
            Ascension = 0;
            CurrentNodeId = -1;
            PotionChance = 40;
            Deck = HwatuDeckContent.CommonStarterDeck();
            Relics = new List<RelicData>();
            Potions = new List<PotionData>();
        }

        public void AddCard(HwatuCardData card)
        {
            if (card != null) { Deck.Add(card); }
        }

        public bool RemoveCardAt(int index)
        {
            if (index < 0 || index >= Deck.Count) { return false; }
            Deck.RemoveAt(index);
            return true;
        }

        /// <summary>덱의 카드를 강화(카드당 1회 — HwatuCardData.Upgrade 규칙).</summary>
        public void UpgradeCard(int index)
        {
            if (index >= 0 && index < Deck.Count) { Deck[index] = Deck[index].Upgrade(); }
        }

        public void AddRelic(RelicData relic)
        {
            if (relic != null) { Relics.Add(relic); }
        }

        public bool AddPotion(PotionData potion)
        {
            if (potion == null || Potions.Count >= MaxPotions) { return false; }
            Potions.Add(potion);
            return true;
        }

        public bool TrySpend(int gold)
        {
            if (gold < 0 || Gold < gold) { return false; }
            Gold -= gold;
            return true;
        }

        public void Heal(int amount)
        {
            Hp = System.Math.Min(MaxHp, Hp + amount);
        }

        public bool IsDead => Hp <= 0;
    }
}

using System.Collections.Generic;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;

namespace Hwatu.Core.Cards
{
    /// <summary>
    /// 카드 정의(불변). 코드 빌더(StarterContent)나 ScriptableObject가 만든다.
    /// 런타임 인스턴스는 <see cref="CardInstance"/>가 감싼다.
    /// </summary>
    public sealed class CardData
    {
        public string Id { get; }
        public string Name { get; }
        public CardType Type { get; }
        public int Cost { get; }
        public TargetType Target { get; }
        public bool Exhaust { get; }
        public IReadOnlyList<EffectData> Effects { get; }
        public CardRarity Rarity { get; }
        public bool Retain { get; }   // 턴 종료 시 버리지 않고 손패 유지
        public bool Innate { get; }   // 전투 첫 손패에 보장 투입
        public bool Ethereal { get; } // 턴 종료 시 미사용이면 소멸(휘발)

        /// <summary>이미 강화된 카드인가(Id 끝 '+'). 카드당 강화는 1회만 가능.</summary>
        public bool IsUpgraded => Id.EndsWith("+");

        public CardData(string id, string name, CardType type, int cost,
            TargetType target, bool exhaust, IReadOnlyList<EffectData> effects,
            CardRarity rarity = CardRarity.Common, bool retain = false, bool innate = false, bool ethereal = false)
        {
            Id = id;
            Name = name;
            Type = type;
            Cost = cost;
            Target = target;
            Exhaust = exhaust;
            Effects = effects;
            Rarity = rarity;
            Retain = retain;
            Innate = innate;
            Ethereal = ethereal;
        }

        /// <summary>
        /// 업그레이드(STS 카드별 맞춤을 op별 규칙으로 근사 — docs/research/sts-deckbuilder-research.md §1.3).
        /// 공격 단일 +3 / 다회 타격당 +1 / 방어 +3 / 독 +3 / 약화·취약 +1 / 광·민첩·가시·재생 +2 / 드로우·자원·배수 +1.
        /// </summary>
        public CardData Upgrade()
        {
            if (IsUpgraded) { return this; }   // 카드당 1회만 — 이미 강화된 카드는 그대로 반환
            int damageHits = 0;
            for (int i = 0; i < Effects.Count; i++)
            {
                if (Effects[i].Op == EffectOp.DealDamage) { damageHits++; }
            }
            bool multiHit = damageHits >= 2;   // 다회 공격은 타격당 +1로 억제(과강화 방지)

            var up = new System.Collections.Generic.List<EffectData>(Effects.Count);
            for (int i = 0; i < Effects.Count; i++)
            {
                EffectData e = Effects[i];
                up.Add(new EffectData(e.Op, e.Amount + UpgradeBonus(e, multiHit), e.Target, e.Status, e.Resource));
            }
            return new CardData(Id + "+", Name + "+", Type, Cost, Target, Exhaust, up, Rarity, Retain, Innate, Ethereal);
        }

        // op별 강화량(연구 문서 §1.3). 다회 공격이면 DealDamage는 타격당 +1.
        private static int UpgradeBonus(EffectData e, bool multiHit)
        {
            switch (e.Op)
            {
                case EffectOp.DealDamage: return multiHit ? 1 : 3;
                case EffectOp.GainBlock: return 3;
                case EffectOp.Draw: return 1;
                case EffectOp.GainResource: return 1;
                case EffectOp.ConsumeRadiance: return 1;   // 광 소비 배수 +1
                case EffectOp.MultiplyPoison: return 1;    // 독 증폭 배수 +1
                case EffectOp.ApplyStatus: return StatusUpgradeBonus(e.Status);
                default: return 0;                         // ClearStatus 등은 수치 무의미
            }
        }

        // 상태이상 강화량: 독 +3(큼), 약화·취약 +1(보수적), 스케일 자원(광·민첩·가시·재생) +2.
        private static int StatusUpgradeBonus(StatusType s)
        {
            switch (s)
            {
                case StatusType.Poison: return 3;
                case StatusType.Weak:
                case StatusType.Vulnerable: return 1;
                case StatusType.Radiance:
                case StatusType.Majesty:
                case StatusType.Dexterity:
                case StatusType.Thorns:
                case StatusType.Regen: return 2;
                default: return 1;
            }
        }

        /// <summary>인챈트를 영구 적용한 복제본(STS2 Enchantments식). sharp=공격/방어 +2, brittle=소멸 부여.</summary>
        public CardData WithEnchant(string enchant)
        {
            var list = new System.Collections.Generic.List<EffectData>(Effects.Count);
            bool sharp = enchant == "sharp";
            bool brittle = enchant == "brittle";
            bool radiant = enchant == "radiant";
            for (int i = 0; i < Effects.Count; i++)
            {
                EffectData e = Effects[i];
                int bonus = (e.Op == EffectOp.DealDamage || e.Op == EffectOp.GainBlock) ? (sharp ? 2 : brittle ? 4 : 0) : 0;
                list.Add(new EffectData(e.Op, e.Amount + bonus, e.Target, e.Status, e.Resource));
            }
            if (radiant)
            {
                list.Add(new EffectData(EffectOp.GainResource, amount: 1, target: TargetType.Self, resource: ResourceType.Radiance));
            }
            bool exhaust = Exhaust || enchant == "brittle";
            string mark = sharp ? " ✦" : brittle ? " ✷" : radiant ? " ☀" : "";
            return new CardData(Id + "_e", Name + mark, Type, Cost, Target, exhaust, list, Rarity, Retain, Innate, Ethereal);
        }
    }
}

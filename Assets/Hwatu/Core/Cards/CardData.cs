using System.Collections.Generic;
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

        public CardData(string id, string name, CardType type, int cost,
            TargetType target, bool exhaust, IReadOnlyList<EffectData> effects,
            CardRarity rarity = CardRarity.Common, bool retain = false)
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
        }

        /// <summary>업그레이드 버전(MVP: DealDamage/GainBlock 수치 +3, 이름·Id에 + 표시). 카드별 세부는 후속.</summary>
        public CardData Upgrade()
        {
            var up = new System.Collections.Generic.List<EffectData>(Effects.Count);
            for (int i = 0; i < Effects.Count; i++)
            {
                EffectData e = Effects[i];
                int amt = (e.Op == EffectOp.DealDamage || e.Op == EffectOp.GainBlock) ? e.Amount + 3 : e.Amount;
                up.Add(new EffectData(e.Op, amt, e.Target, e.Status, e.Resource));
            }
            return new CardData(Id + "+", Name + "+", Type, Cost, Target, Exhaust, up, Rarity, Retain);
        }
    }
}

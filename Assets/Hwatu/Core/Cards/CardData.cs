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

        public CardData(string id, string name, CardType type, int cost,
            TargetType target, bool exhaust, IReadOnlyList<EffectData> effects,
            CardRarity rarity = CardRarity.Common)
        {
            Id = id;
            Name = name;
            Type = type;
            Cost = cost;
            Target = target;
            Exhaust = exhaust;
            Effects = effects;
            Rarity = rarity;
        }
    }
}

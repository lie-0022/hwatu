using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;

namespace Hwatu.Core.Content
{
    /// <summary>광객 카드 보상 풀(레어도별). 시작 덱(StarterContent)과 별개로 보상에 등장하는 카드.</summary>
    public static class LuminaryCards
    {
        // ── Common ──
        public static CardData HeavyStrike() => new CardData("lum_heavy", "강타", CardType.Attack, 2, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 9, target: TargetType.Enemy) }, CardRarity.Common);

        public static CardData Guard() => new CardData("lum_guard", "수비", CardType.Skill, 2, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainBlock, amount: 8, target: TargetType.Self) }, CardRarity.Common);

        public static CardData Whet() => new CardData("lum_whet", "연마", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.Draw, amount: 2) }, CardRarity.Common);

        // ── Uncommon ──
        public static CardData Glow() => new CardData("lum_glow", "광휘", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainResource, amount: 2, target: TargetType.Self, resource: ResourceType.Radiance) }, CardRarity.Uncommon);

        public static CardData Pierce() => new CardData("lum_pierce", "취약타", CardType.Attack, 1, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 5, target: TargetType.Enemy),
                new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Vulnerable),
            }, CardRarity.Uncommon);

        // ── Rare ──
        public static CardData Burst() => new CardData("lum_burst", "폭광", CardType.Attack, 2, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 14, target: TargetType.Enemy) }, CardRarity.Rare);

        public static CardData GreatShield() => new CardData("lum_greatshield", "대방패", CardType.Skill, 2, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainBlock, amount: 14, target: TargetType.Self) }, CardRarity.Rare);

        /// <summary>보상 추첨에 쓰는 광객 카드 풀(7장).</summary>
        public static List<CardData> RewardPool()
        {
            return new List<CardData>
            {
                HeavyStrike(), Guard(), Whet(),
                Glow(), Pierce(),
                Burst(), GreatShield(),
            };
        }
    }
}

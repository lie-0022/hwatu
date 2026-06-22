using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;

namespace Hwatu.Core.Content
{
    /// <summary>묵귀(墨鬼) 카드: 독(Poison)·약화 특화. 빛 대신 지속 피해로 적을 녹인다.</summary>
    public static class InkCards
    {
        // 기본(Basic)
        public static CardData ShadowBlade() => new CardData("ink_blade", "그림자칼", CardType.Attack, 1, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 6, target: TargetType.Enemy) }, CardRarity.Basic);

        public static CardData Shade() => new CardData("ink_shade", "그늘", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainBlock, amount: 5, target: TargetType.Self) }, CardRarity.Basic);

        // Common
        public static CardData Lacquer() => new CardData("ink_lacquer", "옻칠", CardType.Skill, 1, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.ApplyStatus, amount: 4, target: TargetType.Enemy, status: StatusType.Poison) }, CardRarity.Common);

        public static CardData InkStrike() => new CardData("ink_strike", "먹칼", CardType.Attack, 1, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 5, target: TargetType.Enemy),
                new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Poison),
            }, CardRarity.Common);

        public static CardData Soot() => new CardData("ink_soot", "그을음", CardType.Attack, 1, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 4, target: TargetType.Enemy),
                new EffectData(EffectOp.ApplyStatus, amount: 1, target: TargetType.Enemy, status: StatusType.Weak),
            }, CardRarity.Common);

        public static CardData DarkGuard() => new CardData("ink_darkguard", "암막", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainBlock, amount: 8, target: TargetType.Self) }, CardRarity.Common);

        // Uncommon
        public static CardData Seep() => new CardData("ink_seep", "침습", CardType.Attack, 2, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 7, target: TargetType.Enemy),
                new EffectData(EffectOp.ApplyStatus, amount: 3, target: TargetType.Enemy, status: StatusType.Poison),
            }, CardRarity.Uncommon);

        public static CardData Miasma() => new CardData("ink_miasma", "먹구름", CardType.Skill, 1, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Poison),
                new EffectData(EffectOp.ApplyStatus, amount: 1, target: TargetType.Enemy, status: StatusType.Weak),
            }, CardRarity.Uncommon);

        public static CardData Veil() => new CardData("ink_veil", "흑무", CardType.Skill, 1, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Vulnerable) }, CardRarity.Uncommon);

        public static CardData Gu() => new CardData("ink_gu", "고독", CardType.Skill, 1, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.ApplyStatus, amount: 3, target: TargetType.Enemy, status: StatusType.Poison),
                new EffectData(EffectOp.ApplyStatus, amount: 1, target: TargetType.Enemy, status: StatusType.Weak),
            }, CardRarity.Uncommon);

        // Rare
        public static CardData Plague() => new CardData("ink_plague", "역병", CardType.Skill, 2, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.ApplyStatus, amount: 8, target: TargetType.Enemy, status: StatusType.Poison) }, CardRarity.Rare);

        public static CardData Decay() => new CardData("ink_decay", "부패", CardType.Attack, 2, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 5, target: TargetType.Enemy),
                new EffectData(EffectOp.ApplyStatus, amount: 5, target: TargetType.Enemy, status: StatusType.Poison),
            }, CardRarity.Rare);

        // 키워드 카드 (Retain/Exhaust/Ethereal 독 빌드)
        public static CardData Lingering() => new CardData("ink_lingering", "잔독", CardType.Skill, 1, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.ApplyStatus, amount: 3, target: TargetType.Enemy, status: StatusType.Poison) },
            CardRarity.Uncommon, retain: true);

        public static CardData BlackSpot() => new CardData("ink_blackspot", "흑점", CardType.Attack, 2, TargetType.Enemy, true,
            new[] { new EffectData(EffectOp.DealDamage, amount: 12, target: TargetType.Enemy) }, CardRarity.Rare);

        public static CardData ToxicCloud() => new CardData("ink_toxic", "독무", CardType.Skill, 2, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.ApplyStatus, amount: 4, target: TargetType.Enemy, status: StatusType.Poison) },
            CardRarity.Uncommon, retain: false, innate: false, ethereal: true);

        // 독침: 2연타 + 중독2 (다회독)
        public static CardData Sting() => new CardData("ink_sting", "독침", CardType.Attack, 1, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 2, target: TargetType.Enemy),
                new EffectData(EffectOp.DealDamage, amount: 2, target: TargetType.Enemy),
                new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Poison),
            }, CardRarity.Uncommon);

        // 맹독: 강력한 1회 독(중독6, Exhaust)
        public static CardData Venom() => new CardData("ink_venom", "맹독", CardType.Skill, 2, TargetType.Enemy, true,
            new[] { new EffectData(EffectOp.ApplyStatus, amount: 6, target: TargetType.Enemy, status: StatusType.Poison) }, CardRarity.Rare);

        /// <summary>묵귀 시작 덱 10장: 그림자칼×5, 그늘×4, 옻칠×1.</summary>
        public static List<CardData> InkStarterDeck()
        {
            var deck = new List<CardData>();
            for (int i = 0; i < 5; i++) deck.Add(ShadowBlade());
            for (int i = 0; i < 4; i++) deck.Add(Shade());
            deck.Add(Lacquer());
            return deck;
        }

        /// <summary>묵귀 보상 풀(15장: Common4/Uncommon7/Rare4).</summary>
        public static List<CardData> RewardPool()
        {
            return new List<CardData>
            {
                InkStrike(), Soot(), DarkGuard(), Lacquer(),
                Seep(), Miasma(), Veil(), Gu(), Lingering(), ToxicCloud(), Sting(),
                Plague(), Decay(), BlackSpot(), Venom(),
            };
        }
    }
}

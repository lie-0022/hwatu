using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;

namespace Hwatu.Core.Content
{
    /// <summary>
    /// 백호 산군(白虎 山君) 카드: 위엄(Majesty) 자가 성장 + 포효 페이오프. 브루저(아이언클래드형).
    /// 위엄은 DamageMath에서 모든 Attack 피해에 가산된다(Radiance의 백호판). 캐릭터 문서 docs/캐릭터_백호_산군.md.
    /// </summary>
    public static class BeastCards
    {
        // ── 기본(Basic) 시작덱 ──
        // 발톱질: 피해 5(+위엄). 기본 공격
        public static CardData Claw() => new CardData("beast_claw", "발톱질", CardType.Attack, 1, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 5, target: TargetType.Enemy) }, CardRarity.Basic);

        // 웅크리기: 방어 5. 기본 방어
        public static CardData Crouch() => new CardData("beast_crouch", "웅크리기", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainBlock, amount: 5, target: TargetType.Self) }, CardRarity.Basic);

        // 정기 주입: 위엄 +1. 기본 성장
        public static CardData InfuseSpirit() => new CardData("beast_infuse", "정기 주입", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.ApplyStatus, amount: 1, target: TargetType.Self, status: StatusType.Majesty) }, CardRarity.Basic);

        // 포효: 피해 7(+위엄). 성장 페이오프 기본형
        public static CardData Roar() => new CardData("beast_roar", "포효", CardType.Attack, 1, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 7, target: TargetType.Enemy) }, CardRarity.Basic);

        /// <summary>백호 시작 덱 10장: 발톱질×4, 웅크리기×3, 정기주입×2, 포효×1.</summary>
        public static List<CardData> BeastStarterDeck()
        {
            var deck = new List<CardData>();
            for (int i = 0; i < 4; i++) { deck.Add(Claw()); }
            for (int i = 0; i < 3; i++) { deck.Add(Crouch()); }
            for (int i = 0; i < 2; i++) { deck.Add(InfuseSpirit()); }
            deck.Add(Roar());
            return deck;
        }

        // ── 보상 풀 (문서 §4 — 기존 op로 구현 가능한 카드 우선. 각성·최대HP·불굴 등 신규 op 카드는 후속) ──
        // 성장
        public static CardData BoarSpirit() => new CardData("beast_boar", "멧돼지의 기세", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Self, status: StatusType.Majesty) }, CardRarity.Common);

        public static CardData GooseGuide() => new CardData("beast_goose", "기러기의 인도", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.Draw, amount: 2) }, CardRarity.Uncommon);

        public static CardData SwallowGrace() => new CardData("beast_swallow", "제비의 날렵함", CardType.Skill, 0, TargetType.Self, false,
            new[]
            {
                new EffectData(EffectOp.Draw, amount: 1),
                new EffectData(EffectOp.ApplyStatus, amount: 1, target: TargetType.Self, status: StatusType.Majesty),
            }, CardRarity.Uncommon);

        // 공격 / 포효
        public static CardData Flash() => new CardData("beast_flash", "일섬", CardType.Attack, 1, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 9, target: TargetType.Enemy) }, CardRarity.Uncommon);

        public static CardData Combo() => new CardData("beast_combo", "연격", CardType.Attack, 2, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
            }, CardRarity.Uncommon);

        public static CardData BeastBlow() => new CardData("beast_blow", "거수의 일격", CardType.Attack, 2, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 10, target: TargetType.Enemy),
                new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Self, status: StatusType.Majesty),
            }, CardRarity.Rare);

        public static CardData GreatRoar() => new CardData("beast_greatroar", "대포효", CardType.Attack, 2, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 14, target: TargetType.Enemy) }, CardRarity.Rare);

        // 수호 / 유틸
        public static CardData MountainMight() => new CardData("beast_might", "산의 위엄", CardType.Skill, 1, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Weak),
                new EffectData(EffectOp.ApplyStatus, amount: 1, target: TargetType.Enemy, status: StatusType.Vulnerable),
            }, CardRarity.Common);

        public static CardData GuardStance() => new CardData("beast_guard", "호위 자세", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainBlock, amount: 9, target: TargetType.Self) }, CardRarity.Common);

        public static CardData SpiritBurst() => new CardData("beast_burst", "위엄 폭발", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.ApplyStatus, amount: 3, target: TargetType.Self, status: StatusType.Majesty) }, CardRarity.Rare);

        /// <summary>백호 보상 풀(10장: Common3/Uncommon4/Rare3).</summary>
        public static List<CardData> RewardPool()
        {
            return new List<CardData>
            {
                BoarSpirit(), MountainMight(), GuardStance(),
                GooseGuide(), SwallowGrace(), Flash(), Combo(),
                BeastBlow(), GreatRoar(), SpiritBurst(),
            };
        }
    }
}

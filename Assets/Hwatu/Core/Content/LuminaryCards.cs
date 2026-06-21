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

        // ── 다양성 추가 ──
        public static CardData Jab() => new CardData("lum_jab", "난타", CardType.Attack, 1, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 4, target: TargetType.Enemy) }, CardRarity.Common);

        public static CardData LightRay() => new CardData("lum_ray", "광선", CardType.Attack, 1, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 6, target: TargetType.Enemy),
                new EffectData(EffectOp.Draw, amount: 1),
            }, CardRarity.Uncommon);

        public static CardData RadiantSurge() => new CardData("lum_surge", "광휘진", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainResource, amount: 3, target: TargetType.Self, resource: ResourceType.Radiance) }, CardRarity.Rare);

        public static CardData Check() => new CardData("lum_check", "견제", CardType.Attack, 1, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 5, target: TargetType.Enemy),
                new EffectData(EffectOp.ApplyStatus, amount: 1, target: TargetType.Enemy, status: StatusType.Weak),
            }, CardRarity.Common);

        public static CardData Stockpile() => new CardData("lum_stock", "비축", CardType.Skill, 1, TargetType.Self, false,
            new[]
            {
                new EffectData(EffectOp.GainBlock, amount: 5, target: TargetType.Self),
                new EffectData(EffectOp.GainResource, amount: 1, target: TargetType.Self, resource: ResourceType.Radiance),
            }, CardRarity.Uncommon);

        public static CardData Frenzy() => new CardData("lum_frenzy", "광폭타", CardType.Attack, 3, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 20, target: TargetType.Enemy) }, CardRarity.Rare);

        public static CardData Purify() => new CardData("lum_purify", "정화", CardType.Skill, 1, TargetType.Self, false,
            new[]
            {
                new EffectData(EffectOp.GainBlock, amount: 4, target: TargetType.Self),
                new EffectData(EffectOp.ClearStatus, amount: 0, target: TargetType.Self, status: StatusType.Poison),
            }, CardRarity.Uncommon);

        // 소멸(Exhaust) 카드: 1회용 강력 — 쓰면 소멸 더미로
        public static CardData WhiteFlash() => new CardData("lum_flash", "백광", CardType.Attack, 2, TargetType.Enemy, true,
            new[] { new EffectData(EffectOp.DealDamage, amount: 25, target: TargetType.Enemy) }, CardRarity.Rare);

        public static CardData Awaken() => new CardData("lum_awaken", "각성", CardType.Skill, 1, TargetType.Self, true,
            new[] { new EffectData(EffectOp.GainResource, amount: 5, target: TargetType.Self, resource: ResourceType.Radiance) }, CardRarity.Rare);

        // 수호: 방어 6 + 보유(Retain) — 턴 끝에 버리지 않고 다음 턴까지 든다
        public static CardData Vigil() => new CardData("lum_vigil", "수호", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainBlock, amount: 6, target: TargetType.Self) }, CardRarity.Uncommon, retain: true);

        // 서광: 코스트0 빛+1, 전투 첫 손패 보장(Innate)
        public static CardData Dawn() => new CardData("lum_dawn", "서광", CardType.Skill, 0, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainResource, amount: 1, target: TargetType.Self, resource: ResourceType.Radiance) }, CardRarity.Uncommon, retain: false, innate: true);

        // 철벽: 민첩 +2(이후 방어 카드가 그만큼 더 막음)
        public static CardData Bulwark() => new CardData("lum_bulwark", "철벽", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Self, status: StatusType.Dexterity) }, CardRarity.Uncommon);

        // 유성: 공14 + 휘발(Ethereal) — 안 쓰면 턴 끝에 소멸
        public static CardData Meteor() => new CardData("lum_meteor", "유성", CardType.Attack, 1, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 14, target: TargetType.Enemy) }, CardRarity.Rare, retain: false, innate: false, ethereal: true);

        // 연광: 빛살 3연타(3×3) — 다회공격(약점/취약과 시너지)
        public static CardData Volley() => new CardData("lum_volley", "연광", CardType.Attack, 1, TargetType.Enemy, false,
            new[]
            {
                new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
            }, CardRarity.Uncommon);

        /// <summary>보상 추첨에 쓰는 광객 카드 풀(21장: Common5/Uncommon9/Rare7).</summary>
        public static List<CardData> RewardPool()
        {
            return new List<CardData>
            {
                HeavyStrike(), Guard(), Whet(), Jab(), Check(),
                Glow(), Pierce(), LightRay(), Stockpile(), Purify(), Vigil(), Dawn(), Bulwark(), Volley(),
                Burst(), GreatShield(), RadiantSurge(), Frenzy(), WhiteFlash(), Awaken(), Meteor(),
            };
        }
    }
}

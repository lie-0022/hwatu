using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;

namespace Hwatu.Core.Content
{
    /// <summary>
    /// 마일스톤1 시작 콘텐츠의 단일 진실원(코드 빌더). 테스트·헤드리스 러너가 ScriptableObject 없이 이걸 쓴다.
    /// (마일스톤2에서 SO로 외부화하되 이 POCO와 동치를 유지한다.)
    /// </summary>
    public static class StarterContent
    {
        // 빛타격: Attack, cost 1, 6 피해
        public static CardData LightStrike() => new CardData(
            "luminary_light_strike", "빛타격", CardType.Attack, 1, TargetType.Enemy, false,
            new[] { new EffectData(EffectOp.DealDamage, amount: 6, target: TargetType.Enemy) });

        // 방패: Skill, cost 1, 5 Block
        public static CardData Shield() => new CardData(
            "luminary_shield", "방패", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainBlock, amount: 5, target: TargetType.Self) });

        // 점화: Skill, cost 1, radiance +1
        public static CardData Ignite() => new CardData(
            "luminary_ignite", "점화", CardType.Skill, 1, TargetType.Self, false,
            new[] { new EffectData(EffectOp.GainResource, amount: 1, target: TargetType.Self, resource: ResourceType.Radiance) });

        /// <summary>Luminary 시작 덱 10장: 빛타격×5, 방패×4, 점화×1.</summary>
        public static List<CardData> LuminaryStarterDeck()
        {
            var deck = new List<CardData>();
            for (int i = 0; i < 5; i++) deck.Add(LightStrike());
            for (int i = 0; i < 4; i++) deck.Add(Shield());
            deck.Add(Ignite());
            return deck;
        }

        // 잡도깨비: swipe 공격 7 / guard 방어 6, sequence [swipe, swipe, guard]
        public static EnemyData DokkaebiMinion() => new EnemyData(
            "dokkaebi_minion", "잡도깨비", 12, 16,
            new[]
            {
                new EnemyMoveData("swipe", IntentType.Attack, 7,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 7, target: TargetType.Enemy) }),
                new EnemyMoveData("guard", IntentType.Block, 6,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 6, target: TargetType.Self) }),
            },
            EnemyAiKind.Sequence,
            new[] { "swipe", "swipe", "guard" });

        // 보스: 달그림자 도깨비 — 강타 13 / 광폭 9 / 방벽 12, sequence [crush, rage, barrier]
        public static EnemyData DokkaebiBoss() => new EnemyData(
            "dokkaebi_boss", "달그림자 도깨비", 45, 55,
            new[]
            {
                new EnemyMoveData("crush", IntentType.Attack, 13,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 13, target: TargetType.Enemy) }),
                new EnemyMoveData("rage", IntentType.Attack, 9,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 9, target: TargetType.Enemy) }),
                new EnemyMoveData("barrier", IntentType.Block, 12,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 12, target: TargetType.Self) }),
            },
            EnemyAiKind.Sequence,
            new[] { "crush", "rage", "barrier" });

        // 까마귀떼: 다회 약공 — peck(2×3) / flock(방어 4)
        public static EnemyData Crows() => new EnemyData(
            "crows", "까마귀떼", 10, 14,
            new[]
            {
                new EnemyMoveData("peck", IntentType.AttackMulti, 2,
                    new[]
                    {
                        new EffectData(EffectOp.DealDamage, amount: 2, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 2, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 2, target: TargetType.Enemy),
                    }, hits: 3),
                new EnemyMoveData("flock", IntentType.Block, 4,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 4, target: TargetType.Self) }),
            },
            EnemyAiKind.Sequence,
            new[] { "peck", "peck", "flock" });

        // 허수아비: 디버프 — weaken(약화 2) / poke(공격 5)
        public static EnemyData Scarecrow() => new EnemyData(
            "scarecrow", "허수아비", 14, 18,
            new[]
            {
                new EnemyMoveData("weaken", IntentType.Debuff, 2,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Weak) }),
                new EnemyMoveData("poke", IntentType.Attack, 5,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 5, target: TargetType.Enemy) }),
            },
            EnemyAiKind.Sequence,
            new[] { "weaken", "poke", "poke" });

        // 엘리트 광귀: 강타(11) / 취약화(취약 2) / 방벽(10)
        public static EnemyData GwangGwiElite() => new EnemyData(
            "gwanggwi_elite", "광귀", 30, 38,
            new[]
            {
                new EnemyMoveData("smash", IntentType.Attack, 11,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 11, target: TargetType.Enemy) }),
                new EnemyMoveData("expose", IntentType.Debuff, 2,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Vulnerable) }),
                new EnemyMoveData("wall", IntentType.Block, 10,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 10, target: TargetType.Self) }),
            },
            EnemyAiKind.Sequence,
            new[] { "smash", "expose", "wall" });

        // 도깨비불: 화염 약공(3×2) + 흐림(약화 1)
        public static EnemyData WillOWisp() => new EnemyData(
            "willowisp", "도깨비불", 9, 13,
            new[]
            {
                new EnemyMoveData("ember", IntentType.AttackMulti, 3,
                    new[]
                    {
                        new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                    }, hits: 2),
                new EnemyMoveData("haze", IntentType.Debuff, 1,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 1, target: TargetType.Enemy, status: StatusType.Weak) }),
            },
            EnemyAiKind.Sequence,
            new[] { "ember", "haze", "ember" });
    }
}

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
            "dokkaebi_minion", "잡도깨비", 28, 34,
            new[]
            {
                new EnemyMoveData("swipe", IntentType.Attack, 9,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 9, target: TargetType.Enemy) }),
                new EnemyMoveData("guard", IntentType.Block, 6,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 6, target: TargetType.Self) }),
            },
            EnemyAiKind.Sequence,
            new[] { "swipe", "swipe", "guard" });

        // 일반: 멧돼지 — 씩씩대기(자버프 광+2)→들이받기(공10, 강화 시 12). 화투 7월 홍싸리 멧돼지
        public static EnemyData Boar() => new EnemyData(
            "boar", "멧돼지", 38, 46,
            new[]
            {
                new EnemyMoveData("gore", IntentType.Attack, 12,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 12, target: TargetType.Enemy) }),
                new EnemyMoveData("snort", IntentType.Buff, 0,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Self, status: StatusType.Radiance) }),
            },
            EnemyAiKind.WeightedRandom,
            new[] { "snort", "gore", "gore" });

        // 일반: 두꺼비 — 웅크리기(방어10)→독침(공5+중독2). 방어/독 지구전형
        public static EnemyData Toad() => new EnemyData(
            "toad", "두꺼비", 44, 52,
            new[]
            {
                new EnemyMoveData("crouch", IntentType.Block, 10,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 10, target: TargetType.Self) }),
                new EnemyMoveData("venomspit", IntentType.Attack, 5,
                    new[]
                    {
                        new EffectData(EffectOp.DealDamage, amount: 5, target: TargetType.Enemy),
                        new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Poison),
                    }),
            },
            EnemyAiKind.WeightedRandom,
            new[] { "crouch", "venomspit", "venomspit" });

        // 일반: 밤송이도깨비 — 가시 두르기(자기 Thorns 3)→찌르기(공6). 플레이어가 공격하면 가시 3 반사당함(가시 양방향)
        public static EnemyData Burr() => new EnemyData(
            "burr", "밤송이도깨비", 24, 30,
            new[]
            {
                new EnemyMoveData("bristle", IntentType.Buff, 0,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 4, target: TargetType.Self, status: StatusType.Thorns) }),
                new EnemyMoveData("prick", IntentType.Attack, 8,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 8, target: TargetType.Enemy) }),
            },
            EnemyAiKind.Sequence,
            new[] { "bristle", "prick", "prick" });

        // 보스: 달그림자 도깨비 — 1페이즈 [강타13/광폭9/방벽12], HP 50%↓ 2페이즈 광폭화 [월식18/강타13/광폭9]
        public static EnemyData DokkaebiBoss() => new EnemyData(
            "dokkaebi_boss", "달그림자 도깨비", 95, 115,
            new[]
            {
                new EnemyMoveData("crush", IntentType.Attack, 16,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 16, target: TargetType.Enemy) }),
                new EnemyMoveData("rage", IntentType.Attack, 9,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 9, target: TargetType.Enemy) }),
                new EnemyMoveData("barrier", IntentType.Block, 12,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 12, target: TargetType.Self) }),
                new EnemyMoveData("eclipse", IntentType.Attack, 24,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 24, target: TargetType.Enemy) }),
            },
            EnemyAiKind.Phase,
            new[] { "crush", "rage", "barrier" },
            new[] { "eclipse", "crush", "rage" });

        // 까마귀떼: 다회 약공 — peck(2×3) / flock(방어 4)
        public static EnemyData Crows() => new EnemyData(
            "crows", "까마귀떼", 22, 28,
            new[]
            {
                new EnemyMoveData("peck", IntentType.AttackMulti, 3,
                    new[]
                    {
                        new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                    }, hits: 3),
                new EnemyMoveData("flock", IntentType.Block, 4,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 4, target: TargetType.Self) }),
            },
            EnemyAiKind.Sequence,
            new[] { "peck", "peck", "flock" });

        // 허수아비: 디버프 — weaken(약화 2) / poke(공격 5)
        public static EnemyData Scarecrow() => new EnemyData(
            "scarecrow", "허수아비", 28, 34,
            new[]
            {
                new EnemyMoveData("weaken", IntentType.Debuff, 3,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 3, target: TargetType.Enemy, status: StatusType.Weak) }),
                new EnemyMoveData("poke", IntentType.Attack, 5,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 5, target: TargetType.Enemy) }),
            },
            EnemyAiKind.Sequence,
            new[] { "weaken", "poke", "poke" });

        // 엘리트 광귀: 강타(11) / 취약화(취약 2) / 방벽(10)
        public static EnemyData GwangGwiElite() => new EnemyData(
            "gwanggwi_elite", "광귀", 90, 104,
            new[]
            {
                new EnemyMoveData("smash", IntentType.Attack, 15,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 15, target: TargetType.Enemy) }),
                new EnemyMoveData("expose", IntentType.Debuff, 2,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Vulnerable) }),
                new EnemyMoveData("wall", IntentType.Block, 10,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 10, target: TargetType.Self) }),
            },
            EnemyAiKind.Sequence,
            new[] { "smash", "expose", "smash" });

        // 도깨비불: 화염 약공(3×2) + 화상(중독 3 부여)
        public static EnemyData WillOWisp() => new EnemyData(
            "willowisp", "도깨비불", 20, 26,
            new[]
            {
                new EnemyMoveData("ember", IntentType.AttackMulti, 3,
                    new[]
                    {
                        new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                    }, hits: 2),
                new EnemyMoveData("scald", IntentType.Debuff, 5,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 5, target: TargetType.Enemy, status: StatusType.Poison) }),
            },
            EnemyAiKind.Sequence,
            new[] { "ember", "scald", "ember" });

        // 엘리트 외눈도깨비: 노려봄(약화2) / 내려찍기(14) / 분노(자기 광+3 → 이후 공격 강화)
        public static EnemyData CyclopsOni() => new EnemyData(
            "cyclops_oni", "외눈도깨비", 95, 110,
            new[]
            {
                new EnemyMoveData("glare", IntentType.Debuff, 2,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Weak) }),
                new EnemyMoveData("oni_smash", IntentType.Attack, 21,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 21, target: TargetType.Enemy) }),
                new EnemyMoveData("enrage", IntentType.Buff, 3,
                    new[] { new EffectData(EffectOp.GainResource, amount: 3, target: TargetType.Self, resource: ResourceType.Radiance) }),
            },
            EnemyAiKind.Conditional,
            new[] { "glare", "oni_smash", "enrage" });   // 조건부: Block↑면 glare(약화), Block↓면 oni_smash(강타)

        // 보스 구미호: 환혹(약화3)/꼬리치기(4×3)/여우불(12), HP 50%↓ 광폭 [구미폭8×3/여우불/꼬리치기]
        public static EnemyData Gumiho() => new EnemyData(
            "gumiho", "구미호", 100, 120,
            new[]
            {
                new EnemyMoveData("charm", IntentType.Debuff, 3,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 3, target: TargetType.Enemy, status: StatusType.Weak) }),
                new EnemyMoveData("tail", IntentType.AttackMulti, 4,
                    new[]
                    {
                        new EffectData(EffectOp.DealDamage, amount: 4, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 4, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 4, target: TargetType.Enemy),
                    }, hits: 3),
                new EnemyMoveData("foxfire", IntentType.Attack, 15,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 15, target: TargetType.Enemy) }),
                new EnemyMoveData("ninetails", IntentType.AttackMulti, 8,
                    new[]
                    {
                        new EffectData(EffectOp.DealDamage, amount: 8, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 8, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 8, target: TargetType.Enemy),
                    }, hits: 3),
                new EnemyMoveData("doom", IntentType.Doom, 28,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 28, target: TargetType.Enemy) }, hits: 1, doomTurns: 1),
            },
            EnemyAiKind.Phase,
            new[] { "charm", "tail", "foxfire" },
            new[] { "ninetails", "doom", "tail" });

        // 일반 장승: 방어형 — 수호(방10) / 노려봄(약화2) / 들이받기(10)
        public static EnemyData Jangseung() => new EnemyData(
            "jangseung", "장승", 34, 42,
            new[]
            {
                new EnemyMoveData("ward", IntentType.Block, 10,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 10, target: TargetType.Self) }),
                new EnemyMoveData("stare", IntentType.Debuff, 2,
                    new[] { new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Weak) }),
                new EnemyMoveData("ram", IntentType.Attack, 13,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 13, target: TargetType.Enemy) }),
            },
            EnemyAiKind.WeightedRandom,
            new[] { "ward", "stare", "ram" });

        // 일반 그슨대: 어둠 정령 — 할퀴기(3×2) / 저주(약화2)
        public static EnemyData Geuseundae() => new EnemyData(
            "geuseundae", "그슨대", 26, 32,
            new[]
            {
                new EnemyMoveData("claw", IntentType.AttackMulti, 3,
                    new[]
                    {
                        new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                        new EffectData(EffectOp.DealDamage, amount: 3, target: TargetType.Enemy),
                    }, hits: 2),
                new EnemyMoveData("curse", IntentType.Debuff, 2,
                    new[]
                    {
                        new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Weak),
                        new EffectData(EffectOp.ApplyStatus, amount: 1, target: TargetType.Enemy, status: StatusType.Vulnerable),
                    }),
            },
            EnemyAiKind.WeightedRandom,
            new[] { "claw", "curse", "claw" });

        // 엘리트 구렁이: 독 특화 — 휘감기(12) / 독니(6+중독4) / 또아리(방10)
        public static EnemyData Serpent() => new EnemyData(
            "serpent", "구렁이", 92, 106,
            new[]
            {
                new EnemyMoveData("coil_strike", IntentType.Attack, 16,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 16, target: TargetType.Enemy) }),
                new EnemyMoveData("fang", IntentType.Attack, 6,
                    new[]
                    {
                        new EffectData(EffectOp.DealDamage, amount: 6, target: TargetType.Enemy),
                        new EffectData(EffectOp.ApplyStatus, amount: 4, target: TargetType.Enemy, status: StatusType.Poison),
                    }),
                new EnemyMoveData("coil", IntentType.Block, 10,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 10, target: TargetType.Self) }),
            },
            EnemyAiKind.Sequence,
            new[] { "fang", "coil", "coil_strike" });

        // 보스 장군: 조건 반응형 강공 — 호령(약화2+자기광2)/내려베기(18)/철벽(방14)/진군(자기광+3). Block↑면 호령으로 방패 무력화(연구 §4.3)
        public static EnemyData General() => new EnemyData(
            "general", "장군", 105, 125,
            new[]
            {
                new EnemyMoveData("slash", IntentType.Attack, 18,
                    new[] { new EffectData(EffectOp.DealDamage, amount: 18, target: TargetType.Enemy) }),
                new EnemyMoveData("rally", IntentType.Buff, 3,
                    new[] { new EffectData(EffectOp.GainResource, amount: 3, target: TargetType.Self, resource: ResourceType.Radiance) }),
                new EnemyMoveData("bastion", IntentType.Block, 14,
                    new[] { new EffectData(EffectOp.GainBlock, amount: 14, target: TargetType.Self) }),
                // 호령: 플레이어가 방어를 굳히면(Block↑) 약화로 방패를 무력화 + 자기 광 강화 — 보스 조건 반응
                new EnemyMoveData("command", IntentType.Debuff, 2,
                    new[]
                    {
                        new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Weak),
                        new EffectData(EffectOp.GainResource, amount: 2, target: TargetType.Self, resource: ResourceType.Radiance),
                    }),
            },
            // Block↑면 호령(order[0]), 낮으면 내려베기/철벽/진군(order[1..]) 순환
            EnemyAiKind.Conditional,
            new[] { "command", "slash", "bastion", "rally" });
    }
}

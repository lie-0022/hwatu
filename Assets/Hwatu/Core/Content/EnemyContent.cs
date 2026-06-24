using Hwatu.Core.Enemies;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Core.Content
{
    /// <summary>노드 타입에 맞는 적을 시드 RNG로 고른다(일반 풀 / 엘리트 / 보스).</summary>
    public static class EnemyContent
    {
        public static EnemyData PickEnemy(NodeType type, IRandom rng) => PickEnemy(type, rng, 1);

        /// <summary>act 기반 난이도 스케일(act≥2면 HP 배율 1+0.25*(act-1)).</summary>
        public static EnemyData PickEnemy(NodeType type, IRandom rng, int act)
        {
            EnemyData picked = PickBase(type, rng);
            return act >= 2 ? picked.WithHpScale(1.0 + 0.25 * (act - 1)) : picked;
        }

        /// <summary>STS식 인카운터 — 보스/엘리트는 단일 강적, 일반은 미리 정의된 고정 조합(단일=중체, 멀티=약체 다수).</summary>
        public static System.Collections.Generic.List<EnemyData> PickEnemies(NodeType type, IRandom rng, int act)
        {
            var list = new System.Collections.Generic.List<EnemyData>();
            if (type == NodeType.Boss || type == NodeType.Elite)
            {
                list.Add(PickEnemy(type, rng, act));   // 단일 강적
                return list;
            }
            // 일반: 정의된 인카운터 중 하나를 통째로(랜덤 섞기가 아니라 '상정된 조합'). 멀티는 약체로만 묶어 여럿이어도 과하지 않게.
            System.Func<EnemyData>[] encounter = NormalEncounters[rng.NextInt(NormalEncounters.Length)];
            foreach (System.Func<EnemyData> builder in encounter)
            {
                EnemyData ed = builder();
                list.Add(act >= 2 ? ed.WithHpScale(1.0 + 0.25 * (act - 1)) : ed);
            }
            return list;
        }

        // STS식 일반 인카운터 풀(맵 생성 시 노드에 인덱스 배정). 단일(중체 1) + 멀티(약체 2~3 고정 조합).
        // 멀티 몹은 HP 낮은 약체(까마귀22·도깨비불20·밤송이24·그슨대26)라 여럿이어도 적당. 단일은 HP 높은 중체.
        public static readonly System.Func<EnemyData>[][] NormalEncounters = new System.Func<EnemyData>[][]
        {
            new System.Func<EnemyData>[] { StarterContent.Boar },                                    // 단일: 멧돼지
            new System.Func<EnemyData>[] { StarterContent.Toad },                                    // 단일: 두꺼비
            new System.Func<EnemyData>[] { StarterContent.Jangseung },                               // 단일: 장승
            new System.Func<EnemyData>[] { StarterContent.DokkaebiMinion },                          // 단일: 잡도깨비
            new System.Func<EnemyData>[] { StarterContent.Crows, StarterContent.Crows },             // 2: 까마귀떼 둘
            new System.Func<EnemyData>[] { StarterContent.WillOWisp, StarterContent.WillOWisp },     // 2: 도깨비불 짝
            new System.Func<EnemyData>[] { StarterContent.Burr, StarterContent.Geuseundae },         // 2: 밤송이+그슨대
            new System.Func<EnemyData>[] { StarterContent.Scarecrow, StarterContent.Crows },         // 2: 허수아비+까마귀
            new System.Func<EnemyData>[] { StarterContent.WillOWisp, StarterContent.Crows, StarterContent.Geuseundae }, // 3: 약체 셋
            new System.Func<EnemyData>[] { StarterContent.Crows, StarterContent.Burr, StarterContent.WillOWisp },       // 3: 약체 셋
        };

        /// <summary>엘리트 3종을 인덱스로(0 광귀·1 외눈·2 구렁이).</summary>
        public static EnemyData EliteByIndex(int i)
            => i == 1 ? StarterContent.CyclopsOni() : i == 2 ? StarterContent.Serpent() : StarterContent.GwangGwiElite();

        /// <summary>보스 3종을 인덱스로(0 달그림자·1 구미호·2 장군).</summary>
        public static EnemyData BossByIndex(int i)
            => i == 1 ? StarterContent.Gumiho() : i == 2 ? StarterContent.General() : StarterContent.DokkaebiBoss();

        /// <summary>맵에 확정된 인카운터 인덱스로 적 목록을 만든다(전투=조합·엘리트/보스=단일). act HP 스케일 적용. 진입 때 랜덤 재계산 없음.</summary>
        public static System.Collections.Generic.List<EnemyData> BuildEncounter(NodeType type, int encounterId, int act)
        {
            var list = new System.Collections.Generic.List<EnemyData>();
            if (type == NodeType.Boss) { list.Add(Scale(BossByIndex(encounterId), act)); return list; }
            if (type == NodeType.Elite) { list.Add(Scale(EliteByIndex(encounterId), act)); return list; }
            int id = (encounterId >= 0 && encounterId < NormalEncounters.Length) ? encounterId : 0;
            foreach (System.Func<EnemyData> b in NormalEncounters[id]) { list.Add(Scale(b(), act)); }
            return list;
        }

        private static EnemyData Scale(EnemyData ed, int act)
            => act >= 2 ? ed.WithHpScale(1.0 + 0.25 * (act - 1)) : ed;

        private static EnemyData PickBase(NodeType type, IRandom rng)
        {
            switch (type)
            {
                case NodeType.Boss:
                    int bo = rng.NextInt(3);
                    return bo == 0 ? StarterContent.DokkaebiBoss()
                         : bo == 1 ? StarterContent.Gumiho()
                         : StarterContent.General();
                case NodeType.Elite:
                    int el = rng.NextInt(3);
                    return el == 0 ? StarterContent.GwangGwiElite()
                         : el == 1 ? StarterContent.CyclopsOni()
                         : StarterContent.Serpent();
                default:
                    int i = rng.NextInt(9);
                    return i == 0 ? StarterContent.DokkaebiMinion()
                         : i == 1 ? StarterContent.Crows()
                         : i == 2 ? StarterContent.Scarecrow()
                         : i == 3 ? StarterContent.WillOWisp()
                         : i == 4 ? StarterContent.Jangseung()
                         : i == 5 ? StarterContent.Geuseundae()
                         : i == 6 ? StarterContent.Boar()
                         : i == 7 ? StarterContent.Toad()
                         : StarterContent.Burr();
            }
        }
    }
}

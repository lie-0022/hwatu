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

        /// <summary>STS식 다중 몬스터 선택. 보스/엘리트는 1마리(강적), 일반은 1~3마리.</summary>
        public static System.Collections.Generic.List<EnemyData> PickEnemies(NodeType type, IRandom rng, int act)
        {
            var list = new System.Collections.Generic.List<EnemyData>();
            if (type == NodeType.Boss || type == NodeType.Elite)
            {
                list.Add(PickEnemy(type, rng, act));   // 단일 강적
                return list;
            }
            // 일반: 45% 1마리 / 40% 2마리 / 15% 3마리
            int roll = rng.NextInt(100);
            int count = roll < 45 ? 1 : roll < 85 ? 2 : 3;
            for (int i = 0; i < count; i++)
            {
                list.Add(PickEnemy(type, rng, act));
            }
            return list;
        }

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
                    int i = rng.NextInt(8);
                    return i == 0 ? StarterContent.DokkaebiMinion()
                         : i == 1 ? StarterContent.Crows()
                         : i == 2 ? StarterContent.Scarecrow()
                         : i == 3 ? StarterContent.WillOWisp()
                         : i == 4 ? StarterContent.Jangseung()
                         : i == 5 ? StarterContent.Geuseundae()
                         : i == 6 ? StarterContent.Boar()
                         : StarterContent.Toad();
            }
        }
    }
}

using Hwatu.Core.Enemies;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Core.Content
{
    /// <summary>노드 타입에 맞는 적을 시드 RNG로 고른다(일반 풀 / 엘리트 / 보스).</summary>
    public static class EnemyContent
    {
        public static EnemyData PickEnemy(NodeType type, IRandom rng)
        {
            switch (type)
            {
                case NodeType.Boss:
                    return rng.NextInt(2) == 0
                        ? StarterContent.DokkaebiBoss()
                        : StarterContent.Gumiho();
                case NodeType.Elite:
                    int el = rng.NextInt(3);
                    return el == 0 ? StarterContent.GwangGwiElite()
                         : el == 1 ? StarterContent.CyclopsOni()
                         : StarterContent.Serpent();
                default:
                    int i = rng.NextInt(6);
                    return i == 0 ? StarterContent.DokkaebiMinion()
                         : i == 1 ? StarterContent.Crows()
                         : i == 2 ? StarterContent.Scarecrow()
                         : i == 3 ? StarterContent.WillOWisp()
                         : i == 4 ? StarterContent.Jangseung()
                         : StarterContent.Geuseundae();
            }
        }
    }
}

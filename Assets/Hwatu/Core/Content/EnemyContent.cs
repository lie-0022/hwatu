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
                    return StarterContent.DokkaebiBoss();
                case NodeType.Elite:
                    return StarterContent.GwangGwiElite();
                default:
                    int i = rng.NextInt(3);
                    return i == 0 ? StarterContent.DokkaebiMinion()
                         : i == 1 ? StarterContent.Crows()
                         : StarterContent.Scarecrow();
            }
        }
    }
}

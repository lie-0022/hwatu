using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class EnemyContentTests
    {
        [Test]
        public void Boss_And_Elite_AreFixed()
        {
            IRandom rng = new SplitMix64Random(1);
            Assert.AreEqual("달그림자 도깨비", EnemyContent.PickEnemy(NodeType.Boss, rng).Name);
            Assert.AreEqual("광귀", EnemyContent.PickEnemy(NodeType.Elite, rng).Name);
        }

        [Test]
        public void Combat_FromNormalPool()
        {
            var pool = new HashSet<string> { "잡도깨비", "까마귀떼", "허수아비" };
            for (ulong s = 1; s <= 40; s++)
            {
                EnemyData e = EnemyContent.PickEnemy(NodeType.Combat, new SplitMix64Random(s));
                Assert.IsTrue(pool.Contains(e.Name), $"seed {s}: unexpected enemy {e.Name}");
            }
        }

        [Test]
        public void Deterministic_SameSeed()
        {
            Assert.AreEqual(
                EnemyContent.PickEnemy(NodeType.Combat, new SplitMix64Random(7)).Name,
                EnemyContent.PickEnemy(NodeType.Combat, new SplitMix64Random(7)).Name);
        }
    }
}

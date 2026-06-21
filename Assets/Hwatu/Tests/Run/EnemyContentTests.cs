using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class EnemyContentTests
    {
        [Test]
        public void Boss_FromBossPool()
        {
            var pool = new HashSet<string> { "달그림자 도깨비", "구미호" };
            for (ulong s = 1; s <= 20; s++)
            {
                Assert.IsTrue(pool.Contains(EnemyContent.PickEnemy(NodeType.Boss, new SplitMix64Random(s)).Name));
            }
        }

        [Test]
        public void Elite_FromElitePool()
        {
            var pool = new HashSet<string> { "광귀", "외눈도깨비" };
            for (ulong s = 1; s <= 20; s++)
            {
                EnemyData e = EnemyContent.PickEnemy(NodeType.Elite, new SplitMix64Random(s));
                Assert.IsTrue(pool.Contains(e.Name), $"seed {s}: {e.Name}");
            }
        }

        [Test]
        public void Combat_FromNormalPool()
        {
            var pool = new HashSet<string> { "잡도깨비", "까마귀떼", "허수아비", "도깨비불" };
            for (ulong s = 1; s <= 40; s++)
            {
                EnemyData e = EnemyContent.PickEnemy(NodeType.Combat, new SplitMix64Random(s));
                Assert.IsTrue(pool.Contains(e.Name), $"seed {s}: unexpected enemy {e.Name}");
            }
        }

        [Test]
        public void WillOWisp_Scald_AppliesPoison()
        {
            EnemyData wisp = StarterContent.WillOWisp();
            EnemyMoveData scald = wisp.FindMove("scald");
            Assert.IsNotNull(scald);
            Assert.AreEqual(EffectOp.ApplyStatus, scald.Effects[0].Op);
            Assert.AreEqual(StatusType.Poison, scald.Effects[0].Status);
            Assert.AreEqual(3, scald.Effects[0].Amount);
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

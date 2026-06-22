using NUnit.Framework;
using Hwatu.Core.Combat;

namespace Hwatu.Tests.Combat
{
    public class DamageMathTests
    {
        [Test]
        public void RawDamage_BasePlusRadiance()
        {
            var src = new PlayerState(80);
            src.AddStatus(StatusType.Radiance, 2);
            var tgt = new PlayerState(80);
            Assert.AreEqual(8, DamageMath.RawDamage(src, tgt, 6));   // 6 + 광2
        }

        [Test]
        public void RawDamage_WeakReducesToThreeQuarters()
        {
            var src = new PlayerState(80);
            src.AddStatus(StatusType.Weak, 1);
            var tgt = new PlayerState(80);
            Assert.AreEqual(9, DamageMath.RawDamage(src, tgt, 12));  // 12 * 3/4
        }

        [Test]
        public void RawDamage_VulnerableIncreasesToThreeHalves()
        {
            var src = new PlayerState(80);
            var tgt = new PlayerState(80);
            tgt.AddStatus(StatusType.Vulnerable, 1);
            Assert.AreEqual(15, DamageMath.RawDamage(src, tgt, 10)); // 10 * 3/2
        }

        [Test]
        public void RawDamage_WeakAndVulnerableStack()
        {
            var src = new PlayerState(80);
            src.AddStatus(StatusType.Weak, 1);
            var tgt = new PlayerState(80);
            tgt.AddStatus(StatusType.Vulnerable, 1);
            Assert.AreEqual(13, DamageMath.RawDamage(src, tgt, 12)); // 12*3/4=9 → 9*3/2=13 (floor)
        }

        [Test]
        public void RawDamage_NeverNegative()
        {
            var src = new PlayerState(80);
            var tgt = new PlayerState(80);
            Assert.AreEqual(0, DamageMath.RawDamage(src, tgt, -5));
        }
    }
}

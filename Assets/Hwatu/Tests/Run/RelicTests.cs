using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;

namespace Hwatu.Tests.Run
{
    public class RelicTests
    {
        [Test]
        public void Cushion_GivesRadianceOnCombatStart()
        {
            var p = new PlayerState(80);
            RelicContent.Cushion().ApplyCombatStart(p);
            Assert.AreEqual(1, p.GetStatus(StatusType.Radiance));
        }

        [Test]
        public void Blanket_GivesBlockOnCombatStart()
        {
            var p = new PlayerState(80);
            RelicContent.Blanket().ApplyCombatStart(p);
            Assert.AreEqual(5, p.Block);
        }

        [Test]
        public void Lantern_HasNoCombatStartEffect()
        {
            var p = new PlayerState(80);
            RelicContent.Lantern().ApplyCombatStart(p);
            Assert.AreEqual(0, p.Block);
            Assert.AreEqual(0, p.GetStatus(StatusType.Radiance));
        }

        [Test]
        public void AllRelics_AreFive()
        {
            Assert.AreEqual(5, RelicContent.AllRelics().Count);
        }
    }
}

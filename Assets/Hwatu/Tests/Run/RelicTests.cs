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
        public void Charm_GivesBlock8()
        {
            var p = new PlayerState(80);
            RelicContent.Charm().ApplyCombatStart(p);
            Assert.AreEqual(8, p.Block);
        }

        [Test]
        public void SteelScale_GivesDexterity()
        {
            var p = new PlayerState(80);
            RelicContent.SteelScale().ApplyCombatStart(p);
            Assert.AreEqual(1, p.GetStatus(StatusType.Dexterity));
        }

        [Test]
        public void RiceCake_GivesBlock3()
        {
            var p = new PlayerState(80);
            RelicContent.RiceCake().ApplyCombatStart(p);
            Assert.AreEqual(3, p.Block);
        }

        [Test]
        public void Gourd_GivesRadianceAndBlock()
        {
            var p = new PlayerState(80);
            RelicContent.Gourd().ApplyCombatStart(p);
            Assert.AreEqual(1, p.GetStatus(StatusType.Radiance));
            Assert.AreEqual(3, p.Block);
        }

        [Test]
        public void AllRelics_AreTen()
        {
            Assert.AreEqual(10, RelicContent.AllRelics().Count);
        }
    }
}

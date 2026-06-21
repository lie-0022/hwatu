using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;

namespace Hwatu.Tests.Run
{
    public class PotionTests
    {
        [Test]
        public void StrengthPotion_GivesRadiance()
        {
            var p = new PlayerState(80);
            PotionContent.Strength().Apply(p);
            Assert.AreEqual(2, p.GetStatus(StatusType.Radiance));
        }

        [Test]
        public void BlockPotion_GivesBlock()
        {
            var p = new PlayerState(80);
            PotionContent.Block().Apply(p);
            Assert.AreEqual(12, p.Block);
        }

        [Test]
        public void SwiftPotion_GivesDexterity()
        {
            var p = new PlayerState(80);
            PotionContent.Swift().Apply(p);
            Assert.AreEqual(2, p.GetStatus(StatusType.Dexterity));
        }

        [Test]
        public void Pick_IsDeterministic()
        {
            Assert.AreEqual(
                PotionContent.Pick(new SplitMix64Random(3)).Id,
                PotionContent.Pick(new SplitMix64Random(3)).Id);
        }
    }
}

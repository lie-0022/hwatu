using NUnit.Framework;
using Hwatu.Core.Content;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class InkSpiritTests
    {
        [Test]
        public void InkSpirit_Character_HpAndDeck()
        {
            CharacterData ink = CharacterData.InkSpirit();
            Assert.AreEqual("묵귀", ink.Name);
            Assert.AreEqual(70, ink.StartMaxHp);
            Assert.AreEqual(10, ink.StartingDeck.Count);
        }

        [Test]
        public void CharacterPools_InkSpirit_UsesInkPool()
        {
            var pool = CharacterPools.RewardPool("ink_spirit");
            Assert.AreEqual(InkCards.RewardPool().Count, pool.Count);
            Assert.IsTrue(pool.Exists(c => c.Name == "역병"));
        }

        [Test]
        public void CharacterPools_Default_UsesLuminaryPool()
        {
            var pool = CharacterPools.RewardPool("luminary");
            Assert.IsTrue(pool.Exists(c => c.Name == "강타"));
        }
    }
}

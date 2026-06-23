using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;

namespace Hwatu.Tests.Run
{
    public class InkKeywordTests
    {
        [Test]
        public void Lingering_IsRetain_AndPoison()
        {
            CardData c = InkCards.Lingering();
            Assert.IsTrue(c.Retain);
            Assert.AreEqual(StatusType.Poison, c.Effects[0].Status);
        }

        [Test]
        public void BlackSpot_IsExhaust()
        {
            Assert.IsTrue(InkCards.BlackSpot().Exhaust);
        }

        [Test]
        public void ToxicCloud_IsEthereal()
        {
            Assert.IsTrue(InkCards.ToxicCloud().Ethereal);
        }

        [Test]
        public void InkPool_HasTwentyFour()
        {
            Assert.AreEqual(24, InkCards.RewardPool().Count);
        }

        [Test]
        public void Sting_IsMultiHitPoison()
        {
            Assert.AreEqual(3, InkCards.Sting().Effects.Count);
        }
    }
}

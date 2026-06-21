using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Content;

namespace Hwatu.Tests.Run
{
    public class CardUpgradeTests
    {
        [Test]
        public void LightStrike_Upgrade_DamageUp_AndName()
        {
            CardData up = StarterContent.LightStrike().Upgrade();
            Assert.AreEqual(6 + 3, up.Effects[0].Amount);
            Assert.AreEqual("빛타격+", up.Name);
            Assert.AreEqual("luminary_light_strike+", up.Id);
        }

        [Test]
        public void Shield_Upgrade_BlockUp()
        {
            CardData up = StarterContent.Shield().Upgrade();
            Assert.AreEqual(5 + 3, up.Effects[0].Amount);
        }

        [Test]
        public void Ignite_Upgrade_ResourceUnchanged()
        {
            // GainResource는 +3 대상 아님(수치 유지)
            CardData up = StarterContent.Ignite().Upgrade();
            Assert.AreEqual(1, up.Effects[0].Amount);
        }
    }
}

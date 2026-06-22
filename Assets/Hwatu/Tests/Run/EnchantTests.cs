using NUnit.Framework;
using Hwatu.Core.Content;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    /// <summary>Enchantments(영구 카드 인챈트) — sharp/brittle + 덱 적용.</summary>
    public class EnchantTests
    {
        [Test]
        public void Sharp_BoostsDamage()
        {
            var c = StarterContent.LightStrike().WithEnchant("sharp");
            Assert.AreEqual(8, c.Effects[0].Amount);   // 6 + 2
            Assert.IsTrue(c.Name.Contains("✦"));
        }

        [Test]
        public void Brittle_AddsExhaust()
        {
            var c = StarterContent.LightStrike().WithEnchant("brittle");
            Assert.IsTrue(c.Exhaust);
            Assert.AreEqual(10, c.Effects[0].Amount);   // brittle 공격 +4(강력) + 소멸
        }

        [Test]
        public void EnchantCard_ReplacesDeckCard()
        {
            var run = new RunState(CharacterData.Luminary(), 1);
            run.EnchantCard(0, "sharp");
            Assert.IsTrue(run.Deck[0].Name.Contains("✦"));
        }

        [Test]
        public void ForgeEvent_FirstChoice_EnchantsFirstCard()
        {
            var run = new RunState(CharacterData.Luminary(), 1);
            EventContent.Forge().Choices[0].Apply(run);
            Assert.IsTrue(run.Deck[0].Name.Contains("✦"));
        }

        [Test]
        public void Radiant_AddsRadianceEffect()
        {
            var c = StarterContent.LightStrike().WithEnchant("radiant");
            Assert.AreEqual(2, c.Effects.Count);   // 원래 효과 + 광 +1
            Assert.IsTrue(c.Name.Contains("☀"));
        }
    }
}

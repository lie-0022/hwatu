using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;

namespace Hwatu.Tests.Combat
{
    public class ContentTests
    {
        [Test]
        public void LuminaryDeck_Has10Cards_WithCorrectComposition()
        {
            var deck = StarterContent.LuminaryStarterDeck();
            Assert.AreEqual(10, deck.Count);

            int strike = 0, shield = 0, ignite = 0;
            foreach (var c in deck)
            {
                if (c.Id == "luminary_light_strike") strike++;
                else if (c.Id == "luminary_shield") shield++;
                else if (c.Id == "luminary_ignite") ignite++;
            }
            Assert.AreEqual(5, strike, "빛타격 5장");
            Assert.AreEqual(4, shield, "방패 4장");
            Assert.AreEqual(1, ignite, "점화 1장");
        }

        [Test]
        public void Dokkaebi_HasSwipeAndGuard_WithSequenceAi()
        {
            var e = StarterContent.DokkaebiMinion();
            Assert.AreEqual("dokkaebi_minion", e.Id);
            Assert.AreEqual(EnemyAiKind.Sequence, e.AiKind);
            Assert.AreEqual(IntentType.Attack, e.FindMove("swipe").Intent);
            Assert.AreEqual(IntentType.Block, e.FindMove("guard").Intent);
            CollectionAssert.AreEqual(new[] { "swipe", "swipe", "guard" }, (System.Collections.ICollection)e.AiOrder);
        }

        [Test]
        public void LightStrike_DealsSixDamage()
        {
            var card = StarterContent.LightStrike();
            Assert.AreEqual(CardType.Attack, card.Type);
            Assert.AreEqual(1, card.Cost);
            Assert.AreEqual(6, card.Effects[0].Amount);
        }
    }
}

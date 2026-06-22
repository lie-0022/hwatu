using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class ExhaustTests
    {
        [Test]
        public void ExhaustCard_GoesToExhaustPile_NotDiscard()
        {
            var deck = new List<CardData> { LuminaryCards.WhiteFlash() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction (백광 손에)
            engine.PlayCard(0);

            Assert.AreEqual(1, state.ExhaustPile.Count);
            Assert.AreEqual(0, state.DiscardPile.Count);
        }

        [Test]
        public void Awaken_GivesRadiance_AndIsExhaust()
        {
            CardData awaken = LuminaryCards.Awaken();
            Assert.IsTrue(awaken.Exhaust);
            Assert.AreEqual(EffectOp.GainResource, awaken.Effects[0].Op);
            Assert.AreEqual(5, awaken.Effects[0].Amount);
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class PurifyTests
    {
        [Test]
        public void Purify_InCombat_RemovesPlayerPoison_AndGainsBlock()
        {
            var deck = new List<CardData> { LuminaryCards.Purify() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); // CombatStart → PlayerTurnStart
            engine.Advance(); // PlayerTurnStart → PlayerAction (정화가 손에)

            state.Player.AddStatus(StatusType.Poison, 5);
            bool played = engine.PlayCard(0);

            Assert.IsTrue(played);
            Assert.AreEqual(0, state.Player.GetStatus(StatusType.Poison));
            Assert.AreEqual(4, state.Player.Block);
        }

        [Test]
        public void PurifyCard_HasBlockAndClear()
        {
            CardData purify = LuminaryCards.Purify();
            Assert.AreEqual(2, purify.Effects.Count);
            Assert.AreEqual(EffectOp.GainBlock, purify.Effects[0].Op);
            Assert.AreEqual(EffectOp.ClearStatus, purify.Effects[1].Op);
            Assert.AreEqual(StatusType.Poison, purify.Effects[1].Status);
        }
    }
}

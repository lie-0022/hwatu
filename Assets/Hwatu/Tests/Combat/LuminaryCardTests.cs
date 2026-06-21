using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class LuminaryCardTests
    {
        [Test]
        public void Volley_DealsThreeHits()
        {
            var deck = new List<CardData> { LuminaryCards.Volley() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction (연광 손에)

            int hp = state.Enemies[0].Hp;
            engine.PlayCard(0);
            Assert.AreEqual(hp - 9, state.Enemies[0].Hp);   // 3×3
        }

        [Test]
        public void Volley_HasThreeDamageEffects()
        {
            Assert.AreEqual(3, LuminaryCards.Volley().Effects.Count);
        }
    }
}

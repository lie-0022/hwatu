using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class DexterityTests
    {
        [Test]
        public void Dexterity_IncreasesBlockGain()
        {
            // 민첩 2 → 방패(방5)가 방7
            var deck = new List<CardData> { StarterContent.Shield() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            state.Player.AddStatus(StatusType.Dexterity, 2);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction (방패 손에)
            engine.PlayCard(0);

            Assert.AreEqual(7, state.Player.Block);   // 5 + 2
        }

        [Test]
        public void Bulwark_AppliesDexterity()
        {
            CardData b = LuminaryCards.Bulwark();
            Assert.AreEqual(EffectOp.ApplyStatus, b.Effects[0].Op);
            Assert.AreEqual(StatusType.Dexterity, b.Effects[0].Status);
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class InnateTests
    {
        [Test]
        public void InnateCard_IsInOpeningHand()
        {
            // 비-Innate 10장 + Innate 1장(서광) → 첫 손패(5장)에 Innate 보장
            var deck = new List<CardData>();
            for (int i = 0; i < 10; i++) deck.Add(StarterContent.LightStrike());
            deck.Add(LuminaryCards.Dawn());

            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); // CombatStart(셔플 + Innate top)
            engine.Advance(); // PlayerTurnStart(첫 손패 5장) → PlayerAction

            Assert.IsTrue(state.Hand.Exists(c => c.Data.Innate), "Innate 카드는 첫 손패에 있어야 한다");
        }

        [Test]
        public void Dawn_IsInnate()
        {
            Assert.IsTrue(LuminaryCards.Dawn().Innate);
        }
    }
}

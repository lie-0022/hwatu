using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class RetainTests
    {
        [Test]
        public void RetainCard_StaysInHand_NonRetainDiscarded_AtTurnEnd()
        {
            // 수호(Retain) + 빛타격(비Retain) — 덱 2장이라 손패도 2장
            var deck = new List<CardData> { LuminaryCards.Vigil(), StarterContent.LightStrike() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction (손패 2장)

            engine.EndTurn();
            engine.Advance(); // PlayerTurnEnd: Retain 유지, 나머지 버림

            Assert.IsTrue(state.Hand.Exists(c => c.Data.Retain), "Retain 카드는 손패에 남아야 한다");
            Assert.IsFalse(state.Hand.Exists(c => !c.Data.Retain), "비Retain 카드는 버려져야 한다");
        }

        [Test]
        public void Vigil_IsRetainCard()
        {
            Assert.IsTrue(LuminaryCards.Vigil().Retain);
        }
    }
}

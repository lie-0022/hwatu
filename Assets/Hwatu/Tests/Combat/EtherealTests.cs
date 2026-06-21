using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class EtherealTests
    {
        [Test]
        public void EtherealCard_Exhausts_IfUnused_AtTurnEnd()
        {
            // 유성(Ethereal) 1장 — 안 쓰고 턴 종료 → 소멸 더미(버림 아님)
            var deck = new List<CardData> { LuminaryCards.Meteor() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction (유성 손에)

            engine.EndTurn();
            engine.Advance(); // PlayerTurnEnd: 미사용 Ethereal → 소멸

            Assert.AreEqual(1, state.ExhaustPile.Count);
            Assert.AreEqual(0, state.DiscardPile.Count);
        }

        [Test]
        public void Meteor_IsEthereal()
        {
            Assert.IsTrue(LuminaryCards.Meteor().Ethereal);
        }
    }
}

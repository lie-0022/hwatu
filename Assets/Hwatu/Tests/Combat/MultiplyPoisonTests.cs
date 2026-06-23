using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    /// <summary>독 증폭(multiply_poison) op — 대상 중독을 배수로(STS Catalyst식). 촉매 카드 = ×2.</summary>
    public class MultiplyPoisonTests
    {
        [Test]
        public void Catalyst_DoublesEnemyPoison()
        {
            var deck = new List<CardData> { InkCards.Catalyst() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            state.Enemies[0].AddStatus(StatusType.Poison, 5);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();   // PlayerAction (촉매 손에)
            engine.PlayCard(0);
            Assert.AreEqual(10, state.Enemies[0].GetStatus(StatusType.Poison), "독 5 → ×2 = 10");
        }

        [Test]
        public void Catalyst_NoPoison_NoEffect()
        {
            var deck = new List<CardData> { InkCards.Catalyst() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();
            engine.PlayCard(0);
            Assert.AreEqual(0, state.Enemies[0].GetStatus(StatusType.Poison), "독 0이면 효과 없음");
        }
    }
}

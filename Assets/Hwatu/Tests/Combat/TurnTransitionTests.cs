using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class TurnTransitionTests
    {
        private static CombatEngine NewEngine(ulong seed = 123UL)
        {
            var state = CombatFactory.CreateLuminaryVsDokkaebi(seed);
            return new CombatEngine(state, new EffectDispatcher());
        }

        [Test]
        public void CombatStart_Advances_To_PlayerAction_WithEnergyAndHand()
        {
            var engine = NewEngine();
            Assert.AreEqual(CombatPhase.PlayerTurnStart, engine.Advance()); // CombatStart → PlayerTurnStart
            Assert.AreEqual(CombatPhase.PlayerAction, engine.Advance());    // PlayerTurnStart → PlayerAction

            Assert.AreEqual(1, engine.State.Turn);
            Assert.AreEqual(3, engine.State.Player.Energy);
            Assert.AreEqual(5, engine.State.Hand.Count);
        }

        [Test]
        public void PlayerAction_Holds_UntilEndTurn()
        {
            var engine = NewEngine();
            engine.Advance();
            engine.Advance(); // → PlayerAction
            Assert.AreEqual(CombatPhase.PlayerAction, engine.Advance()); // 멈춤 유지

            Assert.IsTrue(engine.EndTurn());
            Assert.AreEqual(CombatPhase.PlayerTurnEnd, engine.State.Phase);
        }

        [Test]
        public void Block_ResetsAtNextPlayerTurnStart()
        {
            var engine = NewEngine();
            engine.Advance();
            engine.Advance(); // turn1 PlayerAction
            engine.State.Player.SetBlock(99);
            engine.EndTurn();

            // PlayerTurnEnd → EnemyTurn → CheckDeath → PlayerTurnStart → PlayerAction
            for (int i = 0; i < 10 &&
                 engine.State.Phase != CombatPhase.PlayerAction &&
                 engine.Result == CombatResult.InProgress; i++)
            {
                engine.Advance();
            }

            Assert.AreEqual(2, engine.State.Turn);
            Assert.AreEqual(0, engine.State.Player.Block); // 새 턴에 Block 초기화
        }

        [Test]
        public void Energy_Spent_WhenPlayingCard()
        {
            var engine = NewEngine();
            engine.Advance();
            engine.Advance(); // PlayerAction, Energy 3, Hand 5

            int before = engine.State.Player.Energy;
            bool played = engine.PlayCard(0, 0); // 첫 카드(cost 1)
            Assert.IsTrue(played);
            Assert.AreEqual(before - 1, engine.State.Player.Energy);
            Assert.AreEqual(4, engine.State.Hand.Count); // 손에서 1장 빠짐
        }
    }
}

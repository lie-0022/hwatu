using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;

namespace Hwatu.Tests.Combat
{
    public class PoisonTests
    {
        [Test]
        public void PlayerPoison_TicksAndDecays_OnFirstTurnStart()
        {
            CombatState state = CombatFactory.CreateLuminaryVsDokkaebi(1);
            state.Player.AddStatus(StatusType.Poison, 3);
            int hp = state.Player.Hp;
            var engine = new CombatEngine(state, new EffectDispatcher());

            engine.Advance(); // CombatStart → PlayerTurnStart
            engine.Advance(); // PlayerTurnStart 실행(Poison 틱) → PlayerAction

            Assert.AreEqual(hp - 3, state.Player.Hp);
            Assert.AreEqual(2, state.Player.GetStatus(StatusType.Poison));
        }

        [Test]
        public void EnemyPoison_TicksAndDecays_OnEnemyTurn()
        {
            CombatState state = CombatFactory.CreateLuminaryVsDokkaebi(1);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); // CombatStart → PlayerTurnStart
            engine.Advance(); // PlayerTurnStart → PlayerAction

            EnemyState enemy = state.Enemies[0];
            enemy.AddStatus(StatusType.Poison, 5);
            int hp = enemy.Hp;

            engine.EndTurn();
            engine.Advance(); // PlayerTurnEnd → EnemyTurn
            engine.Advance(); // EnemyTurn 실행(Poison 틱) → CheckDeath

            Assert.AreEqual(hp - 5, enemy.Hp);
            Assert.AreEqual(4, enemy.GetStatus(StatusType.Poison));
        }
    }
}

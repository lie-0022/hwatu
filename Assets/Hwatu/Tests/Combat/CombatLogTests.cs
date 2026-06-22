using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class CombatLogTests
    {
        [Test]
        public void Log_RecordsTurnAndCardPlay()
        {
            var deck = new List<CardData> { StarterContent.LightStrike() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); // CombatStart → PlayerTurnStart
            engine.Advance(); // PlayerTurnStart 실행(턴 로그) → PlayerAction
            engine.PlayCard(0);

            Assert.IsTrue(state.Log.Exists(l => l.Contains("1턴")));
            Assert.IsTrue(state.Log.Exists(l => l.Contains("빛타격")));
        }

        [Test]
        public void Log_RecordsEnemyAction()
        {
            var deck = new List<CardData> { StarterContent.LightStrike() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction
            engine.EndTurn();
            engine.Advance(); // PlayerTurnEnd → EnemyTurn
            engine.Advance(); // EnemyTurn 실행(적 로그) → CheckDeath

            Assert.IsTrue(state.Log.Exists(l => l.Contains("잡도깨비")));
        }
    }
}

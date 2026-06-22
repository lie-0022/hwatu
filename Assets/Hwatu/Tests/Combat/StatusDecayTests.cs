using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    /// <summary>약화·취약 등 지속 턴 디버프가 턴 종료 시 1씩 감소하는지 검증(영구 지속 버그 회귀 방지).</summary>
    public class StatusDecayTests
    {
        private static CombatEngine MakeEngine(out CombatState state)
        {
            var deck = new List<CardData> { StarterContent.Shield() };
            state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            return new CombatEngine(state, new EffectDispatcher());
        }

        // EndTurn()은 phase만 바꾸므로, 적 턴을 거쳐 다음 PlayerAction까지 상태머신을 진행시킨다.
        private static void RunToNextPlayerAction(CombatEngine engine, CombatState state)
        {
            for (int i = 0; i < 200; i++)
            {
                if (state.Phase == CombatPhase.PlayerAction
                    || state.Phase == CombatPhase.Win
                    || state.Phase == CombatPhase.Lose)
                {
                    return;
                }
                engine.Advance();
            }
        }

        [Test]
        public void PlayerWeak_DecaysOnTurnEnd()
        {
            var engine = MakeEngine(out CombatState state);
            engine.Advance(); engine.Advance();          // PlayerAction까지
            state.Player.AddStatus(StatusType.Weak, 2);
            engine.EndTurn();
            RunToNextPlayerAction(engine, state);        // 플레이어 턴 종료 → 약화 -1
            Assert.AreEqual(1, state.Player.GetStatus(StatusType.Weak));
        }

        [Test]
        public void EnemyVulnerable_DecaysOnEnemyTurnEnd()
        {
            var engine = MakeEngine(out CombatState state);
            engine.Advance(); engine.Advance();
            state.Enemies[0].AddStatus(StatusType.Vulnerable, 2);
            engine.EndTurn();
            RunToNextPlayerAction(engine, state);        // 적 턴 종료 → 적 취약 -1
            Assert.AreEqual(1, state.Enemies[0].GetStatus(StatusType.Vulnerable));
        }

        [Test]
        public void Weak_DoesNotGoNegative()
        {
            var engine = MakeEngine(out CombatState state);
            engine.Advance(); engine.Advance();
            state.Player.AddStatus(StatusType.Weak, 1);
            engine.EndTurn();
            RunToNextPlayerAction(engine, state);        // 1 → 0
            Assert.AreEqual(0, state.Player.GetStatus(StatusType.Weak));
        }
    }
}

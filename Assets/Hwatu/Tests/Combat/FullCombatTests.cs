using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    /// <summary>
    /// 마일스톤1 완료 기준: 헤드리스로 "한 전투를 끝까지 이기고 질 수 있다".
    /// </summary>
    public class FullCombatTests
    {
        // 공격 정책: PlayerAction에서 가능한 Attack 카드를 모두 내고, 없으면 EndTurn.
        private static void RunWithAttackPolicy(CombatEngine engine, int safety = 500)
        {
            int guard = 0;
            while (engine.Result == CombatResult.InProgress && guard++ < safety)
            {
                CombatPhase phase = engine.Advance();
                if (engine.Result != CombatResult.InProgress)
                {
                    break;
                }
                if (phase != CombatPhase.PlayerAction)
                {
                    continue;
                }
                if (!TryPlayFirstAttack(engine))
                {
                    engine.EndTurn();
                }
            }
        }

        private static bool TryPlayFirstAttack(CombatEngine engine)
        {
            var hand = engine.State.Hand;
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i].Data.Type == CardType.Attack && engine.State.Player.Energy >= hand[i].Data.Cost)
                {
                    return engine.PlayCard(i, 0);
                }
            }
            return false;
        }

        [Test]
        public void Player_CanWin_ByAttacking()
        {
            var state = CombatFactory.CreateLuminaryVsDokkaebi(12345UL);
            var engine = new CombatEngine(state, new EffectDispatcher());

            RunWithAttackPolicy(engine);

            Assert.AreEqual(CombatResult.Win, engine.Result);
            Assert.IsTrue(state.Enemies[0].IsDead);
        }

        [Test]
        public void Player_CanLose_ByNeverDefending()
        {
            var state = CombatFactory.CreateLuminaryVsDokkaebi(12345UL);
            var engine = new CombatEngine(state, new EffectDispatcher());

            // 절대 공격·방어하지 않고 매 턴 넘김 → 적 공격 누적으로 사망
            int guard = 0;
            while (engine.Result == CombatResult.InProgress && guard++ < 500)
            {
                CombatPhase phase = engine.Advance();
                if (engine.Result != CombatResult.InProgress)
                {
                    break;
                }
                if (phase == CombatPhase.PlayerAction)
                {
                    engine.EndTurn();
                }
            }

            Assert.AreEqual(CombatResult.Lose, engine.Result);
            Assert.IsTrue(engine.State.Player.Hp <= 0);
        }

        [Test]
        public void Combat_IsDeterministic_SameSeedSameResult()
        {
            CombatResult Run(ulong seed)
            {
                var state = CombatFactory.CreateLuminaryVsDokkaebi(seed);
                var engine = new CombatEngine(state, new EffectDispatcher());
                RunWithAttackPolicy(engine);
                return engine.Result;
            }

            Assert.AreEqual(Run(777UL), Run(777UL));
        }
    }
}

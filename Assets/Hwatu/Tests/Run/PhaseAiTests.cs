using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;

namespace Hwatu.Tests.Run
{
    public class PhaseAiTests
    {
        private static PhaseAi MakeBossAi(out EnemyState enemy, int hp)
        {
            EnemyData boss = StarterContent.DokkaebiBoss();
            var ai = new PhaseAi(boss, boss.AiOrder, boss.SecondPhaseOrder, 50);
            enemy = new EnemyState(boss, 50, ai);   // maxHp 50
            enemy.SetHp(hp);
            return ai;
        }

        [Test]
        public void AboveThreshold_UsesPhase1()
        {
            PhaseAi ai = MakeBossAi(out EnemyState enemy, 50);   // 100% > 50%
            Assert.AreEqual("crush", ai.PeekNext(enemy).Id);
            Assert.IsFalse(ai.IsEnraged);
        }

        [Test]
        public void BelowThreshold_Enrages_UsesPhase2FirstMove()
        {
            PhaseAi ai = MakeBossAi(out EnemyState enemy, 20);   // 40% < 50%
            EnemyMoveData move = ai.PeekNext(enemy);
            Assert.IsTrue(ai.IsEnraged);
            Assert.AreEqual("eclipse", move.Id);
        }

        [Test]
        public void Enrage_IsLatched_DoesNotRevert()
        {
            PhaseAi ai = MakeBossAi(out EnemyState enemy, 20);
            ai.PeekNext(enemy);          // enrage
            enemy.SetHp(50);             // (이론상) 회복
            Assert.IsTrue(ai.IsEnraged);
            Assert.AreEqual("eclipse", ai.PeekNext(enemy).Id);   // 여전히 phase2
        }

        [Test]
        public void PeekNext_IsIdempotent_BeforeAdvance()
        {
            PhaseAi ai = MakeBossAi(out EnemyState enemy, 50);
            Assert.AreEqual(ai.PeekNext(enemy).Id, ai.PeekNext(enemy).Id);
        }
    }
}

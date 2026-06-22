using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;

namespace Hwatu.Tests.Combat
{
    public class EnemyIntentTests
    {
        [Test]
        public void Sequence_CyclesSwipeSwipeGuard()
        {
            var ai = new SequenceAi(StarterContent.DokkaebiMinion());

            Assert.AreEqual("swipe", ai.PeekNext(null).Id); ai.Advance();
            Assert.AreEqual("swipe", ai.PeekNext(null).Id); ai.Advance();
            Assert.AreEqual("guard", ai.PeekNext(null).Id); ai.Advance();
            Assert.AreEqual("swipe", ai.PeekNext(null).Id); // 순환
        }

        [Test]
        public void PeekNext_DoesNotAdvance()
        {
            var ai = new SequenceAi(StarterContent.DokkaebiMinion());
            Assert.AreEqual("swipe", ai.PeekNext(null).Id);
            Assert.AreEqual("swipe", ai.PeekNext(null).Id); // peek은 진행시키지 않음
        }

        [Test]
        public void EnemyState_ExposesIntent_AndActsAsCombatant()
        {
            var data = StarterContent.DokkaebiMinion();
            var enemy = new EnemyState(data, 14, new SequenceAi(data));
            enemy.RefreshIntent();

            Assert.AreEqual(IntentType.Attack, enemy.CurrentIntent.Intent);
            Assert.AreEqual("swipe", enemy.CurrentIntent.Id);
            Assert.AreEqual(14, enemy.Hp);
            Assert.IsFalse(enemy.IsDead);

            enemy.SetHp(0);
            Assert.IsTrue(enemy.IsDead);
        }
    }
}

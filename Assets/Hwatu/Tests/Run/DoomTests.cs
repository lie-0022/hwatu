using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;

namespace Hwatu.Tests.Run
{
    public class DoomTests
    {
        [Test]
        public void Gumiho_DoomMove_HasDoomTurns1()
        {
            EnemyMoveData doom = StarterContent.Gumiho().FindMove("doom");
            Assert.IsNotNull(doom);
            Assert.AreEqual(1, doom.DoomTurns);
        }

        [Test]
        public void EnemyState_DoomTimer_DefaultInactive_AndSettable()
        {
            EnemyData g = StarterContent.Gumiho();
            var e = new EnemyState(g, 50, new SequenceAi(g));
            Assert.AreEqual(-1, e.DoomTimer);
            e.SetDoomTimer(2);
            Assert.AreEqual(2, e.DoomTimer);
        }
    }
}

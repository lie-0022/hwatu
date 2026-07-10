using NUnit.Framework;
using Hwatu.Core.Jokbo;

namespace Hwatu.Tests.Jokbo
{
    /// <summary>족보 런 완주 시뮬(J5 완료 기준) — 맵 경로 자동 진행이 항상 종료하고, 일부는 완주(Victory)한다.</summary>
    public class JokboRunSimTests
    {
        [Test]
        public void FullRun_AlwaysTerminates()
        {
            for (ulong s = 1; s <= 20; s++)
            {
                var flow = new JokboRunFlow();
                flow.StartNewRun(s, 80);
                JokboRunPhase end = JokboRunSim.RunFull(flow);
                Assert.IsTrue(end == JokboRunPhase.Victory || end == JokboRunPhase.GameOver,
                    $"시드 {s}: 런이 승리/패배로 종료해야 함(무한 없음), 실제 {end}");
            }
        }

        [Test]
        public void SomeRuns_ReachVictory_WithCommonDeck()
        {
            int victories = 0;
            for (ulong s = 1; s <= 30; s++)
            {
                var flow = new JokboRunFlow();
                flow.StartNewRun(s, 80);
                if (JokboRunSim.RunFull(flow) == JokboRunPhase.Victory) { victories++; }
            }
            Assert.Greater(victories, 0, $"공통 덱만으로도 완주 가능한 런이 있어야(승리 {victories}/30)");
        }

        [Test]
        public void FullRun_IsDeterministic()
        {
            var a = new JokboRunFlow(); a.StartNewRun(42, 80);
            var b = new JokboRunFlow(); b.StartNewRun(42, 80);
            Assert.AreEqual(JokboRunSim.RunFull(a), JokboRunSim.RunFull(b));
        }
    }
}

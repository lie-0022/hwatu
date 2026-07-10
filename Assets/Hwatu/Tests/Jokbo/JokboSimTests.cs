using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Content;
using Hwatu.Core.Jokbo;

namespace Hwatu.Tests.Jokbo
{
    /// <summary>J3 시뮬 AI·밸런스 측정기 — 그리디 탐색과 결정론 검증.</summary>
    public class JokboSimTests
    {
        private static HwatuCardData C(int month, HwatuCardKind kind)
            => new HwatuCardData(month, kind);

        [Test]
        public void FindBestSubset_PrefersNamedJokbo_OverPair()
        {
            // 홍단 3장 + 1월 피(페어 가능)가 함께 있으면 홍단(12×2=24)이 페어(7)보다 우선
            var hand = new List<JokboCardInstance>
            {
                new JokboCardInstance(C(1, HwatuCardKind.Ribbon), 0),
                new JokboCardInstance(C(2, HwatuCardKind.Ribbon), 1),
                new JokboCardInstance(C(3, HwatuCardKind.Ribbon), 2),
                new JokboCardInstance(C(1, HwatuCardKind.Chaff), 3),
            };
            List<int> best = JokboSimAi.FindBestSubset(hand);
            Assert.IsNotNull(best);
            var cards = new List<HwatuCardData>();
            foreach (int i in best) { cards.Add(hand[i].Data); }
            Assert.AreEqual(JokboType.RedRibbons, JokboDetector.Detect(cards), "홍단 선택");
        }

        [Test]
        public void FindBestSubset_EmptyHand_ReturnsNull()
        {
            Assert.IsNull(JokboSimAi.FindBestSubset(new List<JokboCardInstance>()));
        }

        [Test]
        public void Simulate_IsDeterministic_SameSeed()
        {
            List<HwatuCardData> deck = HwatuDeckContent.CommonStarterDeck();
            JokboBalanceSim.Matchup a = JokboBalanceSim.Simulate(deck, StarterContent.DokkaebiMinion(), 80, 10, baseSeed: 42);
            JokboBalanceSim.Matchup b = JokboBalanceSim.Simulate(deck, StarterContent.DokkaebiMinion(), 80, 10, baseSeed: 42);
            Assert.AreEqual(a.WinRatePct, b.WinRatePct);
            Assert.AreEqual(a.AvgTurns, b.AvgTurns);
            Assert.AreEqual(a.AvgHpLeftOnWin, b.AvgHpLeftOnWin);
        }

        [Test]
        public void StarterDeck_BeatsNormalEnemy_MostOfTheTime()
        {
            // 공통 시작 덱 vs 일반 잡도깨비 — 기본기(페어)만으로도 대부분 이겨야 한다
            List<HwatuCardData> deck = HwatuDeckContent.CommonStarterDeck();
            JokboBalanceSim.Matchup m = JokboBalanceSim.Simulate(deck, StarterContent.DokkaebiMinion(), 80, 30);
            Assert.GreaterOrEqual(m.WinRatePct, 80, $"일반 적 승률 {m.WinRatePct}%");
        }
    }
}

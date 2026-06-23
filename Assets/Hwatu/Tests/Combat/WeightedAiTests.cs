using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;
using Hwatu.Core.Rng;

namespace Hwatu.Tests.Combat
{
    /// <summary>가중치 AI(WeightedAi) — AiOrder 가중치 풀 + 연속 제한 + 완전정보 캐시(연구 §4.2).</summary>
    public class WeightedAiTests
    {
        [Test]
        public void WeightedAi_NeverRepeatsMoreThanLimit()
        {
            // 멧돼지 AiOrder [snort, gore, gore], repeatLimit 2 → 같은 move 3연속 불가
            EnemyData boar = StarterContent.Boar();
            var ai = new WeightedAi(boar, new SplitMix64Random(7), 2);
            var enemy = new EnemyState(boar, 40, ai);
            string last = null;
            int run = 0;
            for (int i = 0; i < 30; i++)
            {
                string id = ai.PeekNext(enemy).Id;
                ai.Advance();
                if (id == last) { run++; }
                else { last = id; run = 1; }
                Assert.LessOrEqual(run, 2, $"step {i}: 같은 move 2회 초과 연속");
            }
        }

        [Test]
        public void WeightedAi_PeekIsStable_BeforeAdvance()
        {
            EnemyData boar = StarterContent.Boar();
            var ai = new WeightedAi(boar, new SplitMix64Random(3), 2);
            var enemy = new EnemyState(boar, 40, ai);
            Assert.AreEqual(ai.PeekNext(enemy).Id, ai.PeekNext(enemy).Id, "Advance 전 Peek는 안정적(완전정보)");
        }

        [Test]
        public void WeightedAi_OnlyEmitsValidMoves()
        {
            EnemyData boar = StarterContent.Boar();
            var ai = new WeightedAi(boar, new SplitMix64Random(11), 2);
            var enemy = new EnemyState(boar, 40, ai);
            for (int i = 0; i < 20; i++)
            {
                string id = ai.PeekNext(enemy).Id;
                Assert.IsTrue(id == "snort" || id == "gore", $"유효 move여야: {id}");
                ai.Advance();
            }
        }
    }
}

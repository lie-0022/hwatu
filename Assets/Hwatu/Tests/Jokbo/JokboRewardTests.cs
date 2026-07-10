using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Jokbo;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Jokbo
{
    /// <summary>족보 카드 보상(J5) — 3택·중복없음·결정론·등급 가중(광 귀함, 보스일수록 광↑).</summary>
    public class JokboRewardTests
    {
        [Test]
        public void RollCardReward_ReturnsThreeDistinct()
        {
            var rng = new SplitMix64Random(5);
            List<HwatuCardData> reward = JokboRewardSystem.RollCardReward(rng, EncounterType.Normal);
            Assert.AreEqual(3, reward.Count);
            var keys = new HashSet<string>();
            foreach (HwatuCardData c in reward)
            {
                Assert.IsTrue(keys.Add($"{c.Month}-{c.Kind}-{c.IsDouble}"), "3택은 서로 다른 카드");
            }
        }

        [Test]
        public void RollCardReward_IsDeterministic()
        {
            var a = JokboRewardSystem.RollCardReward(new SplitMix64Random(11), EncounterType.Normal);
            var b = JokboRewardSystem.RollCardReward(new SplitMix64Random(11), EncounterType.Normal);
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].Month, b[i].Month);
                Assert.AreEqual(a[i].Kind, b[i].Kind);
            }
        }

        [Test]
        public void BrightWeight_HigherForBoss_ThanNormal()
        {
            var bright = new HwatuCardData(1, HwatuCardKind.Bright);
            int normal = JokboRewardSystem.Weight(bright, EncounterType.Normal);
            int boss = JokboRewardSystem.Weight(bright, EncounterType.Boss);
            Assert.Greater(boss, normal, "보스 보상은 광 가중이 더 높다");
        }

        [Test]
        public void ChaffWeight_HigherForNormal_ThanBoss()
        {
            var chaff = new HwatuCardData(1, HwatuCardKind.Chaff);
            Assert.Greater(JokboRewardSystem.Weight(chaff, EncounterType.Normal),
                           JokboRewardSystem.Weight(chaff, EncounterType.Boss), "일반 보상이 피 가중이 더 높다");
        }

        [Test]
        public void BossReward_YieldsMoreBrights_ThanNormal_OverManyRolls()
        {
            int normalBrights = CountBrights(EncounterType.Normal);
            int bossBrights = CountBrights(EncounterType.Boss);
            Assert.Greater(bossBrights, normalBrights, $"보스 광 {bossBrights} > 일반 광 {normalBrights}");
        }

        private static int CountBrights(EncounterType enc)
        {
            int count = 0;
            for (ulong s = 1; s <= 200; s++)
            {
                var reward = JokboRewardSystem.RollCardReward(new SplitMix64Random(s), enc);
                foreach (HwatuCardData c in reward)
                {
                    if (c.Kind == HwatuCardKind.Bright) { count++; }
                }
            }
            return count;
        }
    }
}

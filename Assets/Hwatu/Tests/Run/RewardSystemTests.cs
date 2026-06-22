using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class RewardSystemTests
    {
        [Test]
        public void Boss_AlwaysRare()
        {
            IRandom rng = new SplitMix64Random(1);
            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual(CardRarity.Rare, RewardSystem.RollRarity(rng, EncounterType.Boss, 0));
            }
        }

        [Test]
        public void NextOffset_RareResets_OthersIncrementToCap()
        {
            Assert.AreEqual(-5, RewardSystem.NextOffset(CardRarity.Rare, 30));
            Assert.AreEqual(6, RewardSystem.NextOffset(CardRarity.Common, 5));
            Assert.AreEqual(40, RewardSystem.NextOffset(CardRarity.Uncommon, 40));
        }

        [Test]
        public void Reward_ThreeCards_NoDuplicates()
        {
            IRandom rng = new SplitMix64Random(42);
            int offset = -5;
            IReadOnlyList<CardData> pool = LuminaryCards.RewardPool();
            List<CardData> reward = RewardSystem.RollCardReward(rng, EncounterType.Normal, pool, ref offset, 3);
            Assert.AreEqual(3, reward.Count);
            var ids = new HashSet<string>();
            foreach (CardData c in reward)
            {
                Assert.IsTrue(ids.Add(c.Id), $"duplicate {c.Id}");
            }
        }

        [Test]
        public void Reward_Deterministic_SameSeed()
        {
            IReadOnlyList<CardData> pool = LuminaryCards.RewardPool();
            int o1 = -5, o2 = -5;
            List<CardData> r1 = RewardSystem.RollCardReward(new SplitMix64Random(7), EncounterType.Normal, pool, ref o1, 3);
            List<CardData> r2 = RewardSystem.RollCardReward(new SplitMix64Random(7), EncounterType.Normal, pool, ref o2, 3);
            Assert.AreEqual(r1.Count, r2.Count);
            for (int i = 0; i < r1.Count; i++)
            {
                Assert.AreEqual(r1[i].Id, r2[i].Id);
            }
            Assert.AreEqual(o1, o2);
        }

        [Test]
        public void RarePity_AppearsButNotTooOften()
        {
            IRandom rng = new SplitMix64Random(3);
            int rareCount = 0;
            const int n = 1000;
            int offset = -5;
            for (int i = 0; i < n; i++)
            {
                CardRarity r = RewardSystem.RollRarity(rng, EncounterType.Normal, offset);
                if (r == CardRarity.Rare)
                {
                    rareCount++;
                }
                offset = RewardSystem.NextOffset(r, offset);
            }
            Assert.Greater(rareCount, 0, "피티로 Rare가 가끔은 나와야 함");
            Assert.Less(rareCount, n / 3, "일반 전투에서 Rare가 과하면 안 됨");
        }
    }
}

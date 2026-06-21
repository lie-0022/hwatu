using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Rng;

namespace Hwatu.Tests.Combat
{
    public class RngDeterminismTests
    {
        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            var a = new SplitMix64Random(12345UL);
            var b = new SplitMix64Random(12345UL);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(a.NextInt(1000), b.NextInt(1000));
            }
        }

        [Test]
        public void DifferentSeed_ProducesDifferentSequence()
        {
            var a = new SplitMix64Random(1UL);
            var b = new SplitMix64Random(2UL);
            bool anyDifferent = false;
            for (int i = 0; i < 20; i++)
            {
                if (a.NextInt(1_000_000) != b.NextInt(1_000_000))
                {
                    anyDifferent = true;
                    break;
                }
            }
            Assert.IsTrue(anyDifferent);
        }

        [Test]
        public void Shuffle_IsDeterministic_AndPreservesElements()
        {
            var listA = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var listB = new List<int>(listA);

            new SplitMix64Random(42UL).Shuffle(listA);
            new SplitMix64Random(42UL).Shuffle(listB);

            CollectionAssert.AreEqual(listA, listB);                                  // 같은 시드 → 같은 셔플
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, listA); // 원소 보존
        }

        [Test]
        public void Streams_SameName_SameMaster_AreEqual()
        {
            var s1 = new RngStreams(999UL).ForStream("combatShuffle");
            var s2 = new RngStreams(999UL).ForStream("combatShuffle");
            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual(s1.NextInt(10000), s2.NextInt(10000));
            }
        }

        [Test]
        public void Streams_DifferentName_AreIndependent()
        {
            var streams = new RngStreams(999UL);
            var enemyHp = streams.ForStream("enemyHp");
            var combatShuffle = streams.ForStream("combatShuffle");

            // 서로 다른 이름의 스트림은 (사실상 확실히) 다른 수열을 낸다 — 독립성 스모크.
            bool anyDifferent = false;
            for (int i = 0; i < 20; i++)
            {
                if (enemyHp.NextInt(1_000_000) != combatShuffle.NextInt(1_000_000))
                {
                    anyDifferent = true;
                    break;
                }
            }
            Assert.IsTrue(anyDifferent);
        }
    }
}

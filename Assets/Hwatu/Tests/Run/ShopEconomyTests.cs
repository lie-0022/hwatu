using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class ShopEconomyTests
    {
        [Test]
        public void TrySpend_DeductsOrFails()
        {
            var run = new RunState(CharacterData.Luminary(), 1);   // 시작 골드 99
            Assert.IsTrue(run.TrySpend(50));
            Assert.AreEqual(49, run.Gold);
            Assert.IsFalse(run.TrySpend(100));
            Assert.AreEqual(49, run.Gold);
        }

        [Test]
        public void AddRelic_GrowsRelics()
        {
            var run = new RunState(CharacterData.Luminary(), 1);   // 시작 등잔 1
            int before = run.Relics.Count;
            run.AddRelic(RelicContent.Cushion());
            Assert.AreEqual(before + 1, run.Relics.Count);
        }

        [Test]
        public void RollRelicReward_FromPool()
        {
            List<RelicData> pool = RelicContent.AllRelics();
            RelicData r = RewardSystem.RollRelicReward(new SplitMix64Random(3), pool);
            Assert.IsTrue(pool.Contains(r));
        }

        [Test]
        public void RemoveCard_ShrinksDeck()
        {
            var run = new RunState(CharacterData.Luminary(), 1);   // 시작 덱 10
            int before = run.Deck.Count;
            Assert.IsTrue(run.RemoveCard(run.Deck[0]));
            Assert.AreEqual(before - 1, run.Deck.Count);
        }

        [Test]
        public void RemoveCardAt_RemovesThatIndex()
        {
            var run = new RunState(CharacterData.Luminary(), 1);   // 시작 덱 10
            int before = run.Deck.Count;
            Assert.IsTrue(run.RemoveCardAt(2));
            Assert.AreEqual(before - 1, run.Deck.Count);
        }

        [Test]
        public void RemoveCardAt_OutOfRange_ReturnsFalse()
        {
            var run = new RunState(CharacterData.Luminary(), 1);
            int before = run.Deck.Count;
            Assert.IsFalse(run.RemoveCardAt(-1));
            Assert.IsFalse(run.RemoveCardAt(999));
            Assert.AreEqual(before, run.Deck.Count);
        }
    }
}

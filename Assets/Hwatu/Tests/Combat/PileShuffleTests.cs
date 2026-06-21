using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;

namespace Hwatu.Tests.Combat
{
    public class PileShuffleTests
    {
        private static List<CardInstance> MakePile(int count)
        {
            var card = StarterContent.LightStrike();
            var list = new List<CardInstance>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new CardInstance(card, i));
            }
            return list;
        }

        [Test]
        public void Draw_TakesFromTopOfDrawPile()
        {
            var hand = new List<CardInstance>();
            var draw = MakePile(5);   // InstanceId 0..4, top = 4
            var discard = new List<CardInstance>();

            PileSystem.Draw(hand, draw, discard, new SplitMix64Random(1UL), 2);

            Assert.AreEqual(2, hand.Count);
            Assert.AreEqual(3, draw.Count);
            Assert.AreEqual(4, hand[0].InstanceId); // top 먼저
            Assert.AreEqual(3, hand[1].InstanceId);
        }

        [Test]
        public void Draw_ReshufflesDiscard_WhenDrawEmpty()
        {
            var hand = new List<CardInstance>();
            var draw = new List<CardInstance>();
            var discard = MakePile(5);

            PileSystem.Draw(hand, draw, discard, new SplitMix64Random(7UL), 3);

            Assert.AreEqual(3, hand.Count);    // discard 재생성 후 3장
            Assert.AreEqual(2, draw.Count);    // 5 - 3
            Assert.AreEqual(0, discard.Count); // discard 비워짐
        }

        [Test]
        public void Draw_StopsWhenNoCardsLeft_NoCardLoss()
        {
            var hand = new List<CardInstance>();
            var draw = MakePile(2);
            var discard = MakePile(3); // 총 5장

            PileSystem.Draw(hand, draw, discard, new SplitMix64Random(3UL), 10); // 5장 초과 요청

            int total = hand.Count + draw.Count + discard.Count;
            Assert.AreEqual(5, total, "카드 분실 없음");
            Assert.AreEqual(5, hand.Count, "있는 만큼만 뽑음");
        }

        [Test]
        public void Draw_IsDeterministic_WithSameSeed()
        {
            List<int> Run(ulong seed)
            {
                var hand = new List<CardInstance>();
                var draw = new List<CardInstance>();
                var discard = MakePile(10);
                PileSystem.Draw(hand, draw, discard, new SplitMix64Random(seed), 10);
                var ids = new List<int>();
                foreach (var c in hand) ids.Add(c.InstanceId);
                return ids;
            }

            CollectionAssert.AreEqual(Run(99UL), Run(99UL));
        }
    }
}

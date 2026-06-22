using NUnit.Framework;
using System.Collections.Generic;
using Hwatu.Core.Run;
using Hwatu.Core.Rng;

namespace Hwatu.Tests.Run
{
    public class ShopStockTests
    {
        private static ShopStock Make()
        {
            var run = new RunState(CharacterData.Luminary(), 1);
            IRandom rng = new RngStreams(run.Seed).ForStream("shop_test");
            return ShopStock.Generate(run, rng);
        }

        private static int CountKind(ShopStock stock, ShopItemKind kind)
        {
            int n = 0;
            foreach (ShopItem it in stock.Items) { if (it.Kind == kind) { n++; } }
            return n;
        }

        [Test]
        public void Generate_HasFiveCards()
        {
            Assert.AreEqual(ShopStock.CardCount, CountKind(Make(), ShopItemKind.Card));
        }

        [Test]
        public void Generate_HasTwoRelics()
        {
            Assert.AreEqual(ShopStock.RelicCount, CountKind(Make(), ShopItemKind.Relic));
        }

        [Test]
        public void Generate_HasTwoPotions()
        {
            Assert.AreEqual(ShopStock.PotionCount, CountKind(Make(), ShopItemKind.Potion));
        }

        [Test]
        public void Generate_AllPricesPositive()
        {
            foreach (ShopItem it in Make().Items) { Assert.Greater(it.Price, 0); }
        }

        [Test]
        public void Generate_CardsHaveNoDuplicateIds()
        {
            var ids = new HashSet<string>();
            foreach (ShopItem it in Make().Items)
            {
                if (it.Kind == ShopItemKind.Card)
                {
                    Assert.IsTrue(ids.Add(it.Card.Id), "중복 카드 Id: " + it.Card.Id);
                }
            }
        }

        [Test]
        public void Generate_IsDeterministic()
        {
            ShopStock a = Make();
            ShopStock b = Make();
            Assert.AreEqual(a.Items.Count, b.Items.Count);
            for (int i = 0; i < a.Items.Count; i++)
            {
                Assert.AreEqual(a.Items[i].DisplayName, b.Items[i].DisplayName);
                Assert.AreEqual(a.Items[i].Price, b.Items[i].Price);
            }
        }
    }
}

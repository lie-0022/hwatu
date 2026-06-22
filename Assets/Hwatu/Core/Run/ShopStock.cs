using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;

namespace Hwatu.Core.Run
{
    /// <summary>상점 매물 종류.</summary>
    public enum ShopItemKind { Card, Relic, Potion }

    /// <summary>상점 매물 한 칸(카드·유물·포션 중 하나 + 가격 + 판매여부).</summary>
    public sealed class ShopItem
    {
        public ShopItemKind Kind { get; }
        public CardData Card { get; }
        public RelicData Relic { get; }
        public PotionData Potion { get; }
        public int Price { get; }
        public bool Sold { get; set; }

        private ShopItem(ShopItemKind kind, CardData card, RelicData relic, PotionData potion, int price)
        {
            Kind = kind;
            Card = card;
            Relic = relic;
            Potion = potion;
            Price = price;
        }

        public static ShopItem OfCard(CardData card, int price) => new ShopItem(ShopItemKind.Card, card, null, null, price);
        public static ShopItem OfRelic(RelicData relic, int price) => new ShopItem(ShopItemKind.Relic, null, relic, null, price);
        public static ShopItem OfPotion(PotionData potion, int price) => new ShopItem(ShopItemKind.Potion, null, null, potion, price);

        /// <summary>UI 표시용 이름.</summary>
        public string DisplayName
        {
            get
            {
                switch (Kind)
                {
                    case ShopItemKind.Card: return Card.Name;
                    case ShopItemKind.Relic: return Relic.Name;
                    default: return Potion.Name;
                }
            }
        }
    }

    /// <summary>STS2식 상점 재고: 카드 여러 장 + 유물 + 포션을 동시 진열하고 개별 구매한다.</summary>
    public sealed class ShopStock
    {
        public const int CardCount = 5;
        public const int RelicCount = 2;
        public const int PotionCount = 2;

        public List<ShopItem> Items { get; } = new List<ShopItem>();
        public int RemoveCost { get; } = 75;
        public bool RemoveUsed { get; set; }

        /// <summary>캐릭터 카드 풀·유물·포션에서 매물을 추첨한다(결정론 RNG, 카드·유물은 중복 Id 배제).</summary>
        public static ShopStock Generate(RunState run, IRandom rng)
        {
            var stock = new ShopStock();

            List<CardData> pool = CharacterPools.RewardPool(run.Character.Id);
            var seenCard = new HashSet<string>();
            int guard = 0;
            while (CountKind(stock, ShopItemKind.Card) < CardCount && seenCard.Count < pool.Count && guard++ < 200)
            {
                CardData c = pool[rng.NextInt(pool.Count)];
                if (!seenCard.Add(c.Id)) { continue; }
                stock.Items.Add(ShopItem.OfCard(c, CardPrice(c.Rarity, rng)));
            }

            List<RelicData> relics = RelicContent.AllRelics();
            var seenRelic = new HashSet<string>();
            guard = 0;
            while (CountKind(stock, ShopItemKind.Relic) < RelicCount && seenRelic.Count < relics.Count && guard++ < 200)
            {
                RelicData r = relics[rng.NextInt(relics.Count)];
                if (!seenRelic.Add(r.Id)) { continue; }
                stock.Items.Add(ShopItem.OfRelic(r, RelicPrice(rng)));
            }

            for (int i = 0; i < PotionCount; i++)
            {
                PotionData p = PotionContent.Pick(rng);
                stock.Items.Add(ShopItem.OfPotion(p, PotionPrice(rng)));
            }

            return stock;
        }

        private static int CountKind(ShopStock stock, ShopItemKind kind)
        {
            int n = 0;
            foreach (ShopItem it in stock.Items) { if (it.Kind == kind) { n++; } }
            return n;
        }

        // STS식 기본가 + ±편차(결정론). 카드는 레어도별.
        private static int CardPrice(CardRarity rarity, IRandom rng)
        {
            int basePrice = rarity == CardRarity.Rare ? 150 : (rarity == CardRarity.Uncommon ? 75 : 50);
            return Vary(basePrice, rng);
        }

        private static int RelicPrice(IRandom rng) => Vary(150, rng);

        private static int PotionPrice(IRandom rng) => Vary(60, rng);

        private static int Vary(int basePrice, IRandom rng)
        {
            int range = basePrice / 5;          // ±10% 폭
            int delta = rng.NextInt(range + 1) - range / 2;
            int price = basePrice + delta;
            return price < 1 ? 1 : price;
        }
    }
}

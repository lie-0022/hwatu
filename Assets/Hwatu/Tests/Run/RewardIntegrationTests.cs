using NUnit.Framework;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    /// <summary>전투 후 보상 흐름 통합: 카드 3택1 + 포션 피티 + 유물이 한 시드에서 함께 굴러간다.</summary>
    public class RewardIntegrationTests
    {
        [Test]
        public void Reward_Cards_Potion_Relic_RollTogether()
        {
            var rng = new SplitMix64Random(42);
            int offset = -5;
            int chance = 100;   // 포션 확정

            var cards = RewardSystem.RollCardReward(rng, EncounterType.Normal, LuminaryCards.RewardPool(), ref offset, 3);
            PotionData potion = RewardSystem.RollPotion(rng, PotionContent.All(), ref chance);
            RelicData relic = RewardSystem.RollRelicReward(rng, RelicContent.AllRelics());

            Assert.AreEqual(3, cards.Count, "카드 3택1");
            Assert.IsNotNull(potion, "chance 100이면 포션 확정");
            Assert.AreEqual(90, chance, "포션 나오면 피티 −10");
            Assert.IsNotNull(relic, "유물 1개");
        }
    }
}

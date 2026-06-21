using NUnit.Framework;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class PotionRewardTests
    {
        [Test]
        public void RollPotion_None_RaisesPity()
        {
            var pool = PotionContent.All();
            int chance = 0;   // 0%면 절대 안 나옴
            var p = RewardSystem.RollPotion(new SplitMix64Random(1), pool, ref chance);
            Assert.IsNull(p);
            Assert.AreEqual(10, chance);   // +10
        }

        [Test]
        public void RollPotion_Guaranteed_LowersPity()
        {
            var pool = PotionContent.All();
            int chance = 100;   // 항상 나옴
            var p = RewardSystem.RollPotion(new SplitMix64Random(1), pool, ref chance);
            Assert.IsNotNull(p);
            Assert.AreEqual(90, chance);   // −10
        }

        [Test]
        public void RollPotion_Deterministic()
        {
            var pool = PotionContent.All();
            int c1 = 40, c2 = 40;
            var a = RewardSystem.RollPotion(new SplitMix64Random(7), pool, ref c1);
            var b = RewardSystem.RollPotion(new SplitMix64Random(7), pool, ref c2);
            Assert.AreEqual(a?.Id, b?.Id);
            Assert.AreEqual(c1, c2);
        }

        [Test]
        public void RunState_PotionChance_StartsAt40()
        {
            var run = new RunState(CharacterData.Luminary(), 1);
            Assert.AreEqual(40, run.PotionChance);
        }
    }
}

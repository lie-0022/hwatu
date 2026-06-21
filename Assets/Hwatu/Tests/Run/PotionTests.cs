using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class PotionTests
    {
        [Test]
        public void StrengthPotion_GivesRadiance()
        {
            var p = new PlayerState(80);
            PotionContent.Strength().Apply(p);
            Assert.AreEqual(2, p.GetStatus(StatusType.Radiance));
        }

        [Test]
        public void BlockPotion_GivesBlock()
        {
            var p = new PlayerState(80);
            PotionContent.Block().Apply(p);
            Assert.AreEqual(12, p.Block);
        }

        [Test]
        public void SwiftPotion_GivesDexterity()
        {
            var p = new PlayerState(80);
            PotionContent.Swift().Apply(p);
            Assert.AreEqual(2, p.GetStatus(StatusType.Dexterity));
        }

        [Test]
        public void Pick_IsDeterministic()
        {
            Assert.AreEqual(
                PotionContent.Pick(new SplitMix64Random(3)).Id,
                PotionContent.Pick(new SplitMix64Random(3)).Id);
        }

        [Test]
        public void AddPotion_RespectsSlotLimit()
        {
            var run = new RunState(CharacterData.Luminary(), 1);
            Assert.IsTrue(run.AddPotion(PotionContent.Strength()));
            Assert.IsTrue(run.AddPotion(PotionContent.Block()));
            Assert.IsTrue(run.AddPotion(PotionContent.Swift()));
            Assert.IsFalse(run.AddPotion(PotionContent.Strength()));   // 4번째는 슬롯 가득
            Assert.AreEqual(3, run.Potions.Count);
        }

        [Test]
        public void Antidote_RemovesPoison()
        {
            var p = new PlayerState(80);
            p.AddStatus(StatusType.Poison, 5);
            PotionContent.Antidote().Apply(p);
            Assert.AreEqual(0, p.GetStatus(StatusType.Poison));
        }

        [Test]
        public void Heal_RestoresHp_CappedAtMax()
        {
            var p = new PlayerState(80);
            p.SetHp(70);
            PotionContent.Heal().Apply(p);
            Assert.AreEqual(80, p.Hp);   // 70+15=85 → 최대 80
        }
    }
}

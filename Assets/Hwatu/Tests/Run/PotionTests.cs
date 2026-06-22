using NUnit.Framework;
using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class PotionTests
    {
        private static CombatState NewCombat()
        {
            var deck = new List<CardData> { StarterContent.Shield() };
            return CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
        }

        [Test]
        public void Strength_GivesRadiance()
        {
            CombatState s = NewCombat();
            PotionContent.Strength().Apply(s);
            Assert.AreEqual(3, s.Player.GetStatus(StatusType.Radiance));
        }

        [Test]
        public void Block_GivesBlock()
        {
            CombatState s = NewCombat();
            PotionContent.Block().Apply(s);
            Assert.AreEqual(14, s.Player.Block);
        }

        [Test]
        public void Energy_AddsEnergy()
        {
            CombatState s = NewCombat();
            int before = s.Player.Energy;
            PotionContent.Energy().Apply(s);
            Assert.AreEqual(before + 2, s.Player.Energy);
        }

        [Test]
        public void Heal_RestoresHp_CappedAtMax()
        {
            CombatState s = NewCombat();
            s.Player.SetHp(70);
            PotionContent.Heal().Apply(s);
            Assert.AreEqual(80, s.Player.Hp);   // 70+20=90 → 최대 80
        }

        [Test]
        public void Fire_DamagesEnemy()
        {
            CombatState s = NewCombat();
            int before = s.Enemies[0].Hp;
            PotionContent.Fire().Apply(s);
            Assert.AreEqual(System.Math.Max(0, before - 22), s.Enemies[0].Hp);
        }

        [Test]
        public void WeakBrew_WeakensEnemy()
        {
            CombatState s = NewCombat();
            PotionContent.WeakBrew().Apply(s);
            Assert.AreEqual(3, s.Enemies[0].GetStatus(StatusType.Weak));
        }

        [Test]
        public void PoisonVial_PoisonsEnemy()
        {
            CombatState s = NewCombat();
            PotionContent.PoisonVial().Apply(s);
            Assert.AreEqual(7, s.Enemies[0].GetStatus(StatusType.Poison));
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
            Assert.IsTrue(run.AddPotion(PotionContent.Energy()));
            Assert.IsFalse(run.AddPotion(PotionContent.Strength()));   // 4번째는 슬롯 가득
            Assert.AreEqual(3, run.Potions.Count);
        }
    }
}

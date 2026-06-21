using NUnit.Framework;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class EventContentTests
    {
        private static RunState NewRun() => new RunState(CharacterData.Luminary(), 1);

        [Test]
        public void Spring_Drink_Heals()
        {
            RunState run = NewRun();
            run.Hp = 50;   // 최대(80) 미만
            EventContent.Spring().Choices[0].Apply(run);
            Assert.AreEqual(62, run.Hp);
        }

        [Test]
        public void Spring_Search_AddsGold()
        {
            RunState run = NewRun();
            int before = run.Gold;
            EventContent.Spring().Choices[1].Apply(run);
            Assert.AreEqual(before + 25, run.Gold);
        }

        [Test]
        public void Bargain_Accept_SpendsGoldRaisesMaxHp()
        {
            RunState run = NewRun();   // 골드 99
            int gold = run.Gold;
            int maxHp = run.MaxHp;
            EventContent.Bargain().Choices[0].Apply(run);
            Assert.AreEqual(gold - 30, run.Gold);
            Assert.AreEqual(maxHp + 8, run.MaxHp);
        }

        [Test]
        public void Pick_IsDeterministic()
        {
            Assert.AreEqual(
                EventContent.Pick(new SplitMix64Random(5)).Id,
                EventContent.Pick(new SplitMix64Random(5)).Id);
        }
    }
}

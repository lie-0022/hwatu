using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Jokbo;

namespace Hwatu.Tests.Jokbo
{
    /// <summary>원전 48장 구성(화투_룰_정리.md §2)과 공통 시작 덱 14장(설계 §3.1)의 데이터 검증.</summary>
    public class HwatuDeckContentTests
    {
        [Test]
        public void FullDeck_Has48Cards_4PerMonth()
        {
            List<HwatuCardData> deck = HwatuDeckContent.FullDeck();
            Assert.AreEqual(48, deck.Count);
            for (int m = 1; m <= 12; m++)
            {
                int cnt = deck.FindAll(c => c.Month == m).Count;
                Assert.AreEqual(4, cnt, $"{m}월은 4장");
            }
        }

        [Test]
        public void FullDeck_KindCounts_MatchOriginal()
        {
            List<HwatuCardData> deck = HwatuDeckContent.FullDeck();
            Assert.AreEqual(5, deck.FindAll(c => c.Kind == HwatuCardKind.Bright).Count, "광 5장");
            Assert.AreEqual(9, deck.FindAll(c => c.Kind == HwatuCardKind.Animal).Count, "열끗 9장");
            Assert.AreEqual(10, deck.FindAll(c => c.Kind == HwatuCardKind.Ribbon).Count, "띠 10장");
            Assert.AreEqual(24, deck.FindAll(c => c.Kind == HwatuCardKind.Chaff).Count, "피 24장");
            Assert.AreEqual(2, deck.FindAll(c => c.IsDouble).Count, "쌍피 2장(11·12월)");
        }

        [Test]
        public void FullDeck_RibbonColors_AndGodoriBirds()
        {
            List<HwatuCardData> deck = HwatuDeckContent.FullDeck();
            Assert.AreEqual(3, deck.FindAll(c => c.Ribbon == RibbonColor.Red).Count, "홍단 3(1·2·3월)");
            Assert.AreEqual(3, deck.FindAll(c => c.Ribbon == RibbonColor.Blue).Count, "청단 3(6·9·10월)");
            Assert.AreEqual(3, deck.FindAll(c => c.Ribbon == RibbonColor.Plain).Count, "초단 3(4·5·7월)");
            Assert.AreEqual(1, deck.FindAll(c => c.Ribbon == RibbonColor.Rain).Count, "비띠 1(12월)");
            Assert.AreEqual(3, deck.FindAll(c => c.IsGodoriBird).Count, "고도리 새 3(2·4·8월)");
            Assert.AreEqual(1, deck.FindAll(c => c.IsRainBright).Count, "비광 1(12월)");
        }

        [Test]
        public void StarterDeck_14Cards_AllHavePairPartner()
        {
            List<HwatuCardData> deck = HwatuDeckContent.CommonStarterDeck();
            Assert.AreEqual(14, deck.Count);
            foreach (HwatuCardData c in deck)
            {
                int sameMonth = deck.FindAll(x => x.Month == c.Month).Count;
                Assert.AreEqual(2, sameMonth, $"{c.Name}: 같은 월 2장(먹기 짝 보장)");
            }
        }

        [Test]
        public void StarterDeck_KindMix_And_MaterialHooks()
        {
            List<HwatuCardData> deck = HwatuDeckContent.CommonStarterDeck();
            Assert.AreEqual(1, deck.FindAll(c => c.Kind == HwatuCardKind.Bright).Count, "광 1");
            Assert.AreEqual(3, deck.FindAll(c => c.Kind == HwatuCardKind.Animal).Count, "열끗 3");
            Assert.AreEqual(3, deck.FindAll(c => c.Kind == HwatuCardKind.Ribbon).Count, "띠 3");
            Assert.AreEqual(7, deck.FindAll(c => c.Kind == HwatuCardKind.Chaff).Count, "피 7");
            // 명명 족보 재료는 1장씩만(성장 훅)
            Assert.AreEqual(1, deck.FindAll(c => c.Ribbon == RibbonColor.Red).Count, "홍단 재료 1");
            Assert.AreEqual(1, deck.FindAll(c => c.Ribbon == RibbonColor.Blue).Count, "청단 재료 1");
            Assert.AreEqual(1, deck.FindAll(c => c.Ribbon == RibbonColor.Plain).Count, "초단 재료 1");
            Assert.AreEqual(1, deck.FindAll(c => c.IsGodoriBird).Count, "고도리 재료 1");
        }

        [Test]
        public void StarterDeck_BaselineTotals()
        {
            List<HwatuCardData> deck = HwatuDeckContent.CommonStarterDeck();
            Assert.AreEqual(38, JokboRules.AttackSum(deck), "공격치 총합 38 (광8+열18+띠12)");
            Assert.AreEqual(21, JokboRules.DefenseSum(deck), "방어치 총합 21 (피 7×3)");
        }

        [Test]
        public void Card_Upgrade_OncePerCard()
        {
            HwatuCardData claw = new HwatuCardData(2, HwatuCardKind.Animal);
            HwatuCardData up = claw.Upgrade();
            Assert.AreEqual(JokboRules.AnimalAttack + JokboRules.EnchantBonus, JokboRules.AttackOf(up), "강화 +2");
            Assert.AreSame(up, up.Upgrade(), "재강화 무효(카드당 1회)");
        }
    }
}

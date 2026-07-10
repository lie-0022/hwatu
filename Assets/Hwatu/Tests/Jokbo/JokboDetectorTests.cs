using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Jokbo;

namespace Hwatu.Tests.Jokbo
{
    /// <summary>족보 판정 전수 검증(설계 §4 — 범용·명명·우선순위·무효).</summary>
    public class JokboDetectorTests
    {
        private static HwatuCardData C(int month, HwatuCardKind kind, bool dbl = false)
            => new HwatuCardData(month, kind, dbl);

        private static JokboType D(params HwatuCardData[] cards)
            => JokboDetector.Detect(new List<HwatuCardData>(cards));

        // ── 범용 족보 ──

        [Test]
        public void Single_AnyOneCard()
        {
            Assert.AreEqual(JokboType.Single, D(C(3, HwatuCardKind.Chaff)));
        }

        [Test]
        public void Pair_SameMonth_TwoCards()
        {
            Assert.AreEqual(JokboType.Pair, D(C(1, HwatuCardKind.Ribbon), C(1, HwatuCardKind.Chaff)));
        }

        [Test]
        public void TwoCards_DifferentMonth_IsNone()
        {
            Assert.AreEqual(JokboType.None, D(C(1, HwatuCardKind.Chaff), C(2, HwatuCardKind.Chaff)));
        }

        [Test]
        public void KindTriple_SameKind_MixedMonths()
        {
            Assert.AreEqual(JokboType.KindTriple,
                D(C(1, HwatuCardKind.Chaff), C(5, HwatuCardKind.Chaff), C(9, HwatuCardKind.Chaff)));
        }

        [Test]
        public void Shake_SameMonth_ThreeCards()
        {
            Assert.AreEqual(JokboType.Shake,
                D(C(11, HwatuCardKind.Chaff), C(11, HwatuCardKind.Chaff), C(11, HwatuCardKind.Chaff, dbl: true)));
        }

        [Test]
        public void Bomb_SameMonth_FourCards()
        {
            Assert.AreEqual(JokboType.Bomb,
                D(C(11, HwatuCardKind.Bright), C(11, HwatuCardKind.Chaff), C(11, HwatuCardKind.Chaff), C(11, HwatuCardKind.Chaff, dbl: true)));
        }

        // ── 명명 족보 ──

        [Test]
        public void RedRibbons_Months123()
        {
            Assert.AreEqual(JokboType.RedRibbons,
                D(C(1, HwatuCardKind.Ribbon), C(2, HwatuCardKind.Ribbon), C(3, HwatuCardKind.Ribbon)));
        }

        [Test]
        public void BlueRibbons_Months6910()
        {
            Assert.AreEqual(JokboType.BlueRibbons,
                D(C(6, HwatuCardKind.Ribbon), C(9, HwatuCardKind.Ribbon), C(10, HwatuCardKind.Ribbon)));
        }

        [Test]
        public void PlainRibbons_Months457()
        {
            Assert.AreEqual(JokboType.PlainRibbons,
                D(C(4, HwatuCardKind.Ribbon), C(5, HwatuCardKind.Ribbon), C(7, HwatuCardKind.Ribbon)));
        }

        [Test]
        public void MixedColorRibbons_FallBackToKindTriple()
        {
            // 홍단 1·2월 + 청단 6월 — 색이 섞이면 명명 아님, 같은 끗 셋
            Assert.AreEqual(JokboType.KindTriple,
                D(C(1, HwatuCardKind.Ribbon), C(2, HwatuCardKind.Ribbon), C(6, HwatuCardKind.Ribbon)));
        }

        [Test]
        public void Godori_Birds248()
        {
            Assert.AreEqual(JokboType.Godori,
                D(C(2, HwatuCardKind.Animal), C(4, HwatuCardKind.Animal), C(8, HwatuCardKind.Animal)));
        }

        [Test]
        public void ThreeAnimals_NotAllBirds_KindTriple()
        {
            // 5·7·9월 열끗 — 새 아님 → 같은 끗 셋
            Assert.AreEqual(JokboType.KindTriple,
                D(C(5, HwatuCardKind.Animal), C(7, HwatuCardKind.Animal), C(9, HwatuCardKind.Animal)));
        }

        [Test]
        public void ThreeBrights_NoRain()
        {
            Assert.AreEqual(JokboType.ThreeBrights,
                D(C(1, HwatuCardKind.Bright), C(3, HwatuCardKind.Bright), C(8, HwatuCardKind.Bright)));
        }

        [Test]
        public void ThreeBrights_WithRain_IsRainVariant()
        {
            Assert.AreEqual(JokboType.ThreeBrightsRain,
                D(C(1, HwatuCardKind.Bright), C(3, HwatuCardKind.Bright), C(12, HwatuCardKind.Bright)));
        }

        [Test]
        public void FourBrights()
        {
            Assert.AreEqual(JokboType.FourBrights,
                D(C(1, HwatuCardKind.Bright), C(3, HwatuCardKind.Bright), C(8, HwatuCardKind.Bright), C(11, HwatuCardKind.Bright)));
        }

        [Test]
        public void FiveBrights()
        {
            Assert.AreEqual(JokboType.FiveBrights,
                D(C(1, HwatuCardKind.Bright), C(3, HwatuCardKind.Bright), C(8, HwatuCardKind.Bright), C(11, HwatuCardKind.Bright), C(12, HwatuCardKind.Bright)));
        }

        // ── 무효/경계 ──

        [Test]
        public void Empty_And_TooMany_AreNone()
        {
            Assert.AreEqual(JokboType.None, JokboDetector.Detect(new List<HwatuCardData>()));
            var six = new List<HwatuCardData>();
            for (int i = 0; i < 6; i++) { six.Add(C(1, HwatuCardKind.Chaff)); }
            Assert.AreEqual(JokboType.None, JokboDetector.Detect(six));
        }

        [Test]
        public void MixedKinds_ThreeCards_IsNone()
        {
            Assert.AreEqual(JokboType.None,
                D(C(1, HwatuCardKind.Bright), C(2, HwatuCardKind.Animal), C(5, HwatuCardKind.Chaff)));
        }

        [Test]
        public void FourCards_NotBombNorBrights_IsNone()
        {
            Assert.AreEqual(JokboType.None,
                D(C(1, HwatuCardKind.Chaff), C(2, HwatuCardKind.Chaff), C(5, HwatuCardKind.Chaff), C(9, HwatuCardKind.Chaff)));
        }

        // ── 우선순위(겹침) ──

        [Test]
        public void SameMonthChaffTriple_PrefersShake_OverKindTriple()
        {
            // 11월 피 3장은 같은 끗 셋이기도 하지만 흔들기(×2)가 우선
            Assert.AreEqual(JokboType.Shake,
                D(C(11, HwatuCardKind.Chaff), C(11, HwatuCardKind.Chaff), C(11, HwatuCardKind.Chaff, dbl: true)));
        }

        [Test]
        public void ThreeBrights_PreferredOverKindTriple()
        {
            JokboType t = D(C(1, HwatuCardKind.Bright), C(3, HwatuCardKind.Bright), C(8, HwatuCardKind.Bright));
            Assert.AreNotEqual(JokboType.KindTriple, t);
        }

        // ── 배수/규칙 테이블 ──

        [Test]
        public void Rules_MultiplierTable()
        {
            Assert.AreEqual(1, JokboRules.MultNum(JokboType.Single));
            Assert.AreEqual(2, JokboRules.MultDen(JokboType.Single));
            Assert.AreEqual(1, JokboRules.MultNum(JokboType.Pair));
            Assert.AreEqual(1, JokboRules.MultDen(JokboType.Pair));
            Assert.AreEqual(3, JokboRules.MultNum(JokboType.KindTriple));
            Assert.AreEqual(2, JokboRules.MultDen(JokboType.KindTriple));
            Assert.AreEqual(2, JokboRules.MultNum(JokboType.Shake));
            Assert.AreEqual(3, JokboRules.MultNum(JokboType.Bomb));
            Assert.AreEqual(3, JokboRules.MultNum(JokboType.FiveBrights));
        }

        [Test]
        public void Rules_AoeFlags()
        {
            Assert.IsTrue(JokboRules.IsAoe(JokboType.Bomb));
            Assert.IsTrue(JokboRules.IsAoe(JokboType.ThreeBrights));
            Assert.IsTrue(JokboRules.IsAoe(JokboType.FiveBrights));
            Assert.IsFalse(JokboRules.IsAoe(JokboType.Pair));
            Assert.IsFalse(JokboRules.IsAoe(JokboType.Godori));
        }

        [Test]
        public void ExampleDamage_Pair_AnimalPlusChaff()
        {
            // 먹기(열끗6 + 피3) ×1 → 공격 6, 방어 3 (설계 §4.3 예시)
            var cards = new List<HwatuCardData> { C(2, HwatuCardKind.Animal), C(2, HwatuCardKind.Chaff) };
            Assert.AreEqual(JokboType.Pair, JokboDetector.Detect(cards));
            Assert.AreEqual(6, JokboRules.AttackSum(cards));
            Assert.AreEqual(3, JokboRules.DefenseSum(cards));
        }
    }
}

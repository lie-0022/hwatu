using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Content;

namespace Hwatu.Tests.Run
{
    public class CardUpgradeTests
    {
        [Test]
        public void LightStrike_Upgrade_DamageUp_AndName()
        {
            CardData up = StarterContent.LightStrike().Upgrade();
            Assert.AreEqual(6 + 3, up.Effects[0].Amount);
            Assert.AreEqual("빛타격+", up.Name);
            Assert.AreEqual("luminary_light_strike+", up.Id);
        }

        [Test]
        public void Shield_Upgrade_BlockUp()
        {
            CardData up = StarterContent.Shield().Upgrade();
            Assert.AreEqual(5 + 3, up.Effects[0].Amount);
        }

        [Test]
        public void Ignite_Upgrade_ResourceUp()
        {
            // GainResource(광)도 강화 대상 — +1 (연구 문서 §1.3)
            CardData up = StarterContent.Ignite().Upgrade();
            Assert.AreEqual(2, up.Effects[0].Amount);
        }

        [Test]
        public void MultiHitAttack_Upgrade_PerHitPlusOne()
        {
            // 연광(3×3) 강화 → 타격당 +1 = 4×3 (다회 과강화 방지)
            CardData up = LuminaryCards.Volley().Upgrade();
            Assert.AreEqual(4, up.Effects[0].Amount);
            Assert.AreEqual(4, up.Effects[1].Amount);
            Assert.AreEqual(4, up.Effects[2].Amount);
        }

        [Test]
        public void PoisonCard_Upgrade_PoisonPlusThree()
        {
            // 옻칠(독4) 강화 → 독7
            CardData up = InkCards.Lacquer().Upgrade();
            Assert.AreEqual(7, up.Effects[0].Amount);
        }

        [Test]
        public void Debuff_Upgrade_PlusOne()
        {
            // 흑무(취약2) 강화 → 취약3 (디버프 보수적)
            CardData up = InkCards.Veil().Upgrade();
            Assert.AreEqual(3, up.Effects[0].Amount);
        }

        [Test]
        public void MajestyCard_Upgrade_MajestyPlusTwo()
        {
            // 정기 주입(위엄1) 강화 → 위엄3. 위엄은 광/민첩과 같은 +2 그룹(G3 버그 수정 회귀)
            CardData up = BeastCards.InfuseSpirit().Upgrade();
            Assert.AreEqual(3, up.Effects[0].Amount);
        }
    }
}

using NUnit.Framework;
using Hwatu.Core.Jokbo;

namespace Hwatu.Tests.Jokbo
{
    /// <summary>족보 런 상태(J5) — 공통 시작덱·카드 획득/제거/강화·골드·회복.</summary>
    public class JokboRunStateTests
    {
        [Test]
        public void Initial_CommonDeck14_AndBaseStats()
        {
            var run = new JokboRunState(42);
            Assert.AreEqual(14, run.Deck.Count, "공통 시작덱 14장");
            Assert.AreEqual(70, run.Hp);
            Assert.AreEqual(70, run.MaxHp);
            Assert.AreEqual(1, run.Act);
            Assert.IsFalse(run.IsDead);
        }

        [Test]
        public void AddCard_GrowsDeck()
        {
            var run = new JokboRunState(1);
            run.AddCard(new HwatuCardData(8, HwatuCardKind.Bright));
            Assert.AreEqual(15, run.Deck.Count);
        }

        [Test]
        public void RemoveCardAt_Compresses()
        {
            var run = new JokboRunState(1);
            Assert.IsTrue(run.RemoveCardAt(0));
            Assert.AreEqual(13, run.Deck.Count);
            Assert.IsFalse(run.RemoveCardAt(99), "범위 밖 제거 실패");
        }

        [Test]
        public void UpgradeCard_OncePerCard()
        {
            var run = new JokboRunState(1);
            // 첫 카드(1월 홍단 띠) 강화 → 공격치 +2, 재강화 무효
            int before = JokboRules.AttackOf(run.Deck[0]);
            run.UpgradeCard(0);
            Assert.AreEqual(before + JokboRules.EnchantBonus, JokboRules.AttackOf(run.Deck[0]));
            HwatuCardData once = run.Deck[0];
            run.UpgradeCard(0);
            Assert.AreEqual(JokboRules.AttackOf(once), JokboRules.AttackOf(run.Deck[0]), "재강화 없음");
        }

        [Test]
        public void TrySpend_RespectsGold()
        {
            var run = new JokboRunState(1, startGold: 50);
            Assert.IsFalse(run.TrySpend(60), "골드 부족");
            Assert.IsTrue(run.TrySpend(30));
            Assert.AreEqual(20, run.Gold);
        }

        [Test]
        public void Heal_CappedAtMax()
        {
            var run = new JokboRunState(1);
            run.Hp = 60;
            run.Heal(30);
            Assert.AreEqual(70, run.Hp, "최대 HP 캡");
        }
    }
}

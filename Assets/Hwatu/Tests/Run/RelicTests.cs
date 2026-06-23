using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;

namespace Hwatu.Tests.Run
{
    public class RelicTests
    {
        [Test]
        public void Cushion_GivesRadianceOnCombatStart()
        {
            var p = new PlayerState(80);
            RelicContent.Cushion().ApplyCombatStart(p);
            Assert.AreEqual(1, p.GetStatus(StatusType.Radiance));
        }

        [Test]
        public void Blanket_GivesBlockOnCombatStart()
        {
            var p = new PlayerState(80);
            RelicContent.Blanket().ApplyCombatStart(p);
            Assert.AreEqual(5, p.Block);
        }

        [Test]
        public void Lantern_HasNoCombatStartEffect()
        {
            var p = new PlayerState(80);
            RelicContent.Lantern().ApplyCombatStart(p);
            Assert.AreEqual(0, p.Block);
            Assert.AreEqual(0, p.GetStatus(StatusType.Radiance));
        }

        [Test]
        public void Charm_GivesBlock8()
        {
            var p = new PlayerState(80);
            RelicContent.Charm().ApplyCombatStart(p);
            Assert.AreEqual(8, p.Block);
        }

        [Test]
        public void SteelScale_GivesDexterity()
        {
            var p = new PlayerState(80);
            RelicContent.SteelScale().ApplyCombatStart(p);
            Assert.AreEqual(1, p.GetStatus(StatusType.Dexterity));
        }

        [Test]
        public void RiceCake_GivesBlock3()
        {
            var p = new PlayerState(80);
            RelicContent.RiceCake().ApplyCombatStart(p);
            Assert.AreEqual(3, p.Block);
        }

        [Test]
        public void Gourd_GivesRadianceAndBlock()
        {
            var p = new PlayerState(80);
            RelicContent.Gourd().ApplyCombatStart(p);
            Assert.AreEqual(1, p.GetStatus(StatusType.Radiance));
            Assert.AreEqual(3, p.Block);
        }

        [Test]
        public void InkStone_GivesRadianceAndDexterity()
        {
            var p = new PlayerState(80);
            RelicContent.InkStone().ApplyCombatStart(p);
            Assert.AreEqual(1, p.GetStatus(StatusType.Radiance));
            Assert.AreEqual(1, p.GetStatus(StatusType.Dexterity));
        }

        [Test]
        public void Herb_HealsOnCombatStart()
        {
            var p = new PlayerState(80);
            p.SetHp(50);
            RelicContent.Herb().ApplyCombatStart(p);
            Assert.AreEqual(54, p.Hp);
        }

        [Test]
        public void RegenCharm_GivesRegenOnCombatStart()
        {
            var p = new PlayerState(80);
            RelicContent.RegenCharm().ApplyCombatStart(p);
            Assert.AreEqual(3, p.GetStatus(StatusType.Regen));
        }

        [Test]
        public void ThornMail_GivesThornsOnCombatStart()
        {
            var p = new PlayerState(80);
            RelicContent.ThornMail().ApplyCombatStart(p);
            Assert.AreEqual(2, p.GetStatus(StatusType.Thorns));
        }

        [Test]
        public void EchoHide_AddsBlock_OnTurnStartNotCombatStart()
        {
            // 산울림 가죽: 트리거 확장 — 전투시작엔 무효, 매 턴 시작에 방어 +4
            var p = new PlayerState(80);
            RelicContent.EchoHide().ApplyCombatStart(p);
            Assert.AreEqual(0, p.Block, "전투시작엔 효과 없음");
            RelicContent.EchoHide().ApplyTurnStart(p);
            Assert.AreEqual(4, p.Block, "턴 시작에 방어 +4");
        }

        [Test]
        public void Abacus_AddsBlock_OnCardPlayOnly()
        {
            // 주판: 트리거 확장 — 전투시작/턴시작엔 무효, 카드 플레이마다 방어 +1
            var p = new PlayerState(80);
            RelicContent.Abacus().ApplyCombatStart(p);
            RelicContent.Abacus().ApplyTurnStart(p);
            Assert.AreEqual(0, p.Block, "전투시작·턴시작엔 효과 없음");
            RelicContent.Abacus().ApplyCardPlay(p);
            Assert.AreEqual(1, p.Block, "카드 플레이에 방어 +1");
        }

        [Test]
        public void LastStand_AddsBlock_OnlyWhenHpLow()
        {
            // 배수진: HP 임계 트리거 — 절반 초과엔 무효, 절반 이하 턴 시작에 방어 +8
            var p = new PlayerState(80);
            RelicContent.LastStand().ApplyTurnStart(p);
            Assert.AreEqual(0, p.Block, "HP 절반 초과면 무효");
            p.SetHp(40);
            RelicContent.LastStand().ApplyTurnStart(p);
            Assert.AreEqual(8, p.Block, "HP 절반 이하면 방어 +8");
        }

        [Test]
        public void AllRelics_AreSeventeen()
        {
            Assert.AreEqual(17, RelicContent.AllRelics().Count);
        }
    }
}

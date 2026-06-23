using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    /// <summary>재생(Regen) status — 플레이어 턴 시작 시 회복 + 1 감소. 치유광 카드 = Regen 3.</summary>
    public class RegenTests
    {
        [Test]
        public void Mend_GrantsRegenThree()
        {
            var deck = new List<CardData> { LuminaryCards.Mend() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();   // PlayerAction
            engine.PlayCard(0);
            Assert.AreEqual(3, state.Player.GetStatus(StatusType.Regen), "치유광 → 재생 3");
        }

        [Test]
        public void Regen_HealsAtTurnStart_AndDecrements()
        {
            // 치유광(재생3) + 대방패(방어14)로 적 공격을 흡수해 회복만 관찰
            var deck = new List<CardData> { LuminaryCards.Mend(), LuminaryCards.GreatShield() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            state.Player.SetHp(50);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();
            engine.PlayCard(0);   // 손 2장(치유광/대방패) 모두 사용 — Self라 순서 무관
            engine.PlayCard(0);
            Assert.AreEqual(50, state.Player.Hp, "아직 턴 시작 전이라 회복 없음");
            engine.EndTurn();
            while (state.Phase != CombatPhase.PlayerAction && state.Phase != CombatPhase.Win && state.Phase != CombatPhase.Lose)
            {
                engine.Advance();
            }
            Assert.AreEqual(53, state.Player.Hp, "다음 턴 시작 재생 3 회복(적 공격은 방어14로 흡수)");
            Assert.AreEqual(2, state.Player.GetStatus(StatusType.Regen), "재생 1 감소");
        }
    }
}

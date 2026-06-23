using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    /// <summary>가시(Thorns) status — 피격 시 공격자에게 반사(Block 무시·영구). 가시광 = 방어7+가시3.</summary>
    public class ThornsTests
    {
        [Test]
        public void ThornAura_GrantsThornsAndBlock()
        {
            var deck = new List<CardData> { LuminaryCards.ThornAura() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();   // PlayerAction
            engine.PlayCard(0);
            Assert.AreEqual(3, state.Player.GetStatus(StatusType.Thorns), "가시 3");
            Assert.AreEqual(7, state.Player.Block, "방어 7");
        }

        [Test]
        public void Thorns_ReflectsToAttacker_OnEnemyAttack()
        {
            // 잡도깨비 sequence 첫 행동 = swipe(공격 9) → 플레이어 피격 시 가시 반사
            var deck = new List<CardData> { LuminaryCards.ThornAura() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();
            engine.PlayCard(0);   // 가시광 → 가시3 + 방어7
            int enemyHp = state.Enemies[0].Hp;
            engine.EndTurn();
            while (state.Phase != CombatPhase.PlayerAction && state.Phase != CombatPhase.Win && state.Phase != CombatPhase.Lose)
            {
                engine.Advance();
            }
            Assert.AreEqual(enemyHp - 3, state.Enemies[0].Hp, "적 공격 시 가시 3 반사(Block 무시)");
        }

        [Test]
        public void Thorns_OnEnemy_ReflectsToPlayerAttacker()
        {
            // 밤송이도깨비처럼 적이 가시를 두르면, 플레이어가 공격할 때 반사당함(가시 양방향)
            var deck = new List<CardData> { LuminaryCards.HeavyStrike() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.Burr(), 1, 80, 80);
            state.Enemies[0].AddStatus(StatusType.Thorns, 3);
            int playerHp = state.Player.Hp;
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();
            engine.PlayCard(0);   // 강타 공격 → 적 가시 3 반사
            Assert.AreEqual(playerHp - 3, state.Player.Hp, "적 가시에 플레이어 3 반사");
        }
    }
}

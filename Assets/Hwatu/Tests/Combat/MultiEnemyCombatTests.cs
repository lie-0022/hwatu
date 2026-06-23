using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    /// <summary>다중 몬스터 전투 — CombatFactory 다중 오버로드 + 인덱스 타겟팅.</summary>
    public class MultiEnemyCombatTests
    {
        [Test]
        public void CreateCombat_ThreeEnemies_BuildsAllAlive()
        {
            var enemies = new[] { StarterContent.DokkaebiMinion(), StarterContent.Toad(), StarterContent.Crows() };
            CombatState state = CombatFactory.CreateCombat(StarterContent.LuminaryStarterDeck(), enemies, 1, 80, 80);
            Assert.AreEqual(3, state.Enemies.Count, "적 3마리 생성");
            foreach (EnemyState e in state.Enemies)
            {
                Assert.Greater(e.Hp, 0, "각 적 HP 양수");
            }
        }

        [Test]
        public void PlayCard_TargetsChosenEnemyByIndex()
        {
            // 강타(피해9)를 두 번째 적(index 1)에만 명중
            var deck = new List<CardData> { LuminaryCards.HeavyStrike() };
            var enemies = new[] { StarterContent.DokkaebiMinion(), StarterContent.DokkaebiMinion(), StarterContent.DokkaebiMinion() };
            CombatState state = CombatFactory.CreateCombat(deck, enemies, 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();
            int e0 = state.Enemies[0].Hp, e1 = state.Enemies[1].Hp, e2 = state.Enemies[2].Hp;
            engine.PlayCard(0, 1);   // index 1 타겟
            Assert.AreEqual(e0, state.Enemies[0].Hp, "0번 적 무피해");
            Assert.AreEqual(e1 - 9, state.Enemies[1].Hp, "1번 적 9 피해");
            Assert.AreEqual(e2, state.Enemies[2].Hp, "2번 적 무피해");
        }
    }
}

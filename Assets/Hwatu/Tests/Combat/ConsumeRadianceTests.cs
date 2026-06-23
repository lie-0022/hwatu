using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    /// <summary>광 소비(consume_radiance) op — 보유 광 전부 소비 → 광×배수 데미지(STS2 Stars식).</summary>
    public class ConsumeRadianceTests
    {
        [Test]
        public void RadiantNova_ConsumesAllRadiance_DealsRadianceTimesThree()
        {
            var deck = new List<CardData> { LuminaryCards.RadiantNova() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            state.Player.AddStatus(StatusType.Radiance, 4);
            int enemyHp = state.Enemies[0].Hp;
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();   // PlayerAction (광폭발 손에)
            engine.PlayCard(0);
            Assert.AreEqual(0, state.Player.GetStatus(StatusType.Radiance), "광 전부 소비");
            Assert.AreEqual(enemyHp - 12, state.Enemies[0].Hp, "광4 × 3 = 12 데미지");
        }

        [Test]
        public void RadiantNova_NoRadiance_DealsNoDamage()
        {
            var deck = new List<CardData> { LuminaryCards.RadiantNova() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            int enemyHp = state.Enemies[0].Hp;
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();
            engine.PlayCard(0);
            Assert.AreEqual(enemyHp, state.Enemies[0].Hp, "광 0이면 데미지 없음");
        }
    }
}

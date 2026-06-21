using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;

namespace Hwatu.Tests.Combat
{
    /// <summary>자율 확장 콘텐츠 통합: 캐릭터·페이즈 보스·키워드가 한 전투에서 맞물려 동작하는지.</summary>
    public class IntegrationTests
    {
        [Test]
        public void InkDeck_VsGumihoBoss_PhaseTransitionAndDraw()
        {
            var deck = InkCards.InkStarterDeck();
            EnemyData boss = StarterContent.Gumiho();
            CombatState state = CombatFactory.CreateCombat(deck, boss, 777, 70, 70);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction

            EnemyState e = state.Enemies[0];
            Assert.AreEqual("charm", e.CurrentIntent.Id);      // 1페이즈 첫 intent
            Assert.AreEqual(5, state.Hand.Count);              // 묵귀 시작 덱 드로우

            e.SetHp(e.MaxHp / 3);                              // HP < 50%
            e.RefreshIntent();
            Assert.AreEqual("ninetails", e.CurrentIntent.Id);  // 2페이즈 광폭 전환
        }

        [Test]
        public void Luminary_KeywordCards_InCombat()
        {
            // 서광(Innate) + 백광(Exhaust) + 유성(Ethereal) + 기본
            var deck = new List<CardData>
            {
                LuminaryCards.Dawn(), LuminaryCards.WhiteFlash(), LuminaryCards.Meteor(),
                StarterContent.LightStrike(), StarterContent.Shield(),
            };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 555, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction

            Assert.IsTrue(state.Hand.Exists(c => c.Data.Id == "lum_dawn"), "서광(Innate)은 첫 손패에 보장");

            int flashIdx = state.Hand.FindIndex(c => c.Data.Id == "lum_flash");
            Assert.GreaterOrEqual(flashIdx, 0);
            engine.PlayCard(flashIdx);
            Assert.IsTrue(state.ExhaustPile.Exists(c => c.Data.Id == "lum_flash"), "백광(Exhaust)은 소멸 더미로");
        }
    }
}

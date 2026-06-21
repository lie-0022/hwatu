using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;

namespace Hwatu.Tests.Combat
{
    /// <summary>자율 확장 콘텐츠 통합: 묵귀 덱 + 구미호 페이즈 보스가 한 전투에서 맞물려 동작하는지.</summary>
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
    }
}

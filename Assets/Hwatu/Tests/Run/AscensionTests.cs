using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class AscensionTests
    {
        [Test]
        public void EnemyHpPercent_ScalesWithAscension()
        {
            Assert.AreEqual(100, AscensionRules.EnemyHpPercent(0));
            Assert.AreEqual(105, AscensionRules.EnemyHpPercent(1));
            Assert.AreEqual(125, AscensionRules.EnemyHpPercent(5));
            Assert.AreEqual(100, AscensionRules.EnemyHpPercent(-2));   // 음수 가드
        }

        [Test]
        public void StartHpPenalty_Scales()
        {
            Assert.AreEqual(0, AscensionRules.StartHpPenalty(0));
            Assert.AreEqual(15, AscensionRules.StartHpPenalty(5));
        }

        [Test]
        public void CombatFactory_HigherAscension_RaisesEnemyHp()
        {
            CombatState a0 = CombatFactory.CreateCombat(StarterContent.LuminaryStarterDeck(),
                StarterContent.DokkaebiMinion(), 1, 80, 80, null, 0);
            CombatState a5 = CombatFactory.CreateCombat(StarterContent.LuminaryStarterDeck(),
                StarterContent.DokkaebiMinion(), 1, 80, 80, null, 5);
            Assert.Greater(a5.Enemies[0].Hp, a0.Enemies[0].Hp);
        }
    }
}

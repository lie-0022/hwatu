using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class DamageFormulaTests
    {
        private static (EffectDispatcher, TestEffectContext, TestCombatant src, TestCombatant tgt) Setup(
            int srcHp = 50, int tgtHp = 50, int tgtBlock = 0)
        {
            var src = new TestCombatant(srcHp);
            var tgt = new TestCombatant(tgtHp, tgtBlock);
            var ctx = new TestEffectContext { Source = src, Target = tgt };
            return (new EffectDispatcher(), ctx, src, tgt);
        }

        [Test]
        public void BaseDamage_NoModifiers()
        {
            var (d, ctx, _, tgt) = Setup();
            d.Execute(new EffectData(EffectOp.DealDamage, amount: 6), ctx);
            Assert.AreEqual(44, tgt.Hp);
        }

        [Test]
        public void Radiance_AddsToDamage()
        {
            var (d, ctx, src, tgt) = Setup();
            src.AddStatus(StatusType.Radiance, 3);
            d.Execute(new EffectData(EffectOp.DealDamage, amount: 6), ctx); // 6+3=9
            Assert.AreEqual(41, tgt.Hp);
        }

        [Test]
        public void Vulnerable_Multiplies_By_3_over_2()
        {
            var (d, ctx, _, tgt) = Setup();
            tgt.AddStatus(StatusType.Vulnerable, 1);
            d.Execute(new EffectData(EffectOp.DealDamage, amount: 6), ctx); // 6*3/2=9
            Assert.AreEqual(41, tgt.Hp);
        }

        [Test]
        public void Weak_Reduces_By_3_over_4()
        {
            var (d, ctx, src, tgt) = Setup();
            src.AddStatus(StatusType.Weak, 1);
            d.Execute(new EffectData(EffectOp.DealDamage, amount: 8), ctx); // 8*3/4=6
            Assert.AreEqual(44, tgt.Hp);
        }
    }
}

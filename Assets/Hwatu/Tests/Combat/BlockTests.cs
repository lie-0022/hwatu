using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class BlockTests
    {
        [Test]
        public void Block_FullyAbsorbs_NoHpLoss()
        {
            var src = new TestCombatant();
            var tgt = new TestCombatant(hp: 50, block: 10);
            var ctx = new TestEffectContext { Source = src, Target = tgt };

            new EffectDispatcher().Execute(new EffectData(EffectOp.DealDamage, amount: 6), ctx);

            Assert.AreEqual(4, tgt.Block); // 10 - 6
            Assert.AreEqual(50, tgt.Hp);   // HP 무손실
        }

        [Test]
        public void Block_PartiallyAbsorbs_RemainderHitsHp()
        {
            var src = new TestCombatant();
            var tgt = new TestCombatant(hp: 50, block: 4);
            var ctx = new TestEffectContext { Source = src, Target = tgt };

            new EffectDispatcher().Execute(new EffectData(EffectOp.DealDamage, amount: 6), ctx);

            Assert.AreEqual(0, tgt.Block);  // 4 모두 소모
            Assert.AreEqual(48, tgt.Hp);    // 50 - (6-4)
        }

        [Test]
        public void GainBlock_Self_AddsBlockToSource()
        {
            var src = new TestCombatant(hp: 50, block: 0);
            var tgt = new TestCombatant();
            var ctx = new TestEffectContext { Source = src, Target = tgt };

            new EffectDispatcher().Execute(
                new EffectData(EffectOp.GainBlock, amount: 5, target: TargetType.Self), ctx);

            Assert.AreEqual(5, src.Block);
        }
    }
}

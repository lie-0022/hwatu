using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;
using Hwatu.Core.Rng;

namespace Hwatu.Tests.Combat
{
    public class EffectDispatcherTests
    {
        [Test]
        public void ApplyStatus_AddsStatusToTarget()
        {
            var src = new TestCombatant();
            var tgt = new TestCombatant();
            var ctx = new TestEffectContext { Source = src, Target = tgt };

            new EffectDispatcher().Execute(
                new EffectData(EffectOp.ApplyStatus, amount: 2, target: TargetType.Enemy, status: StatusType.Weak), ctx);

            Assert.AreEqual(2, tgt.GetStatus(StatusType.Weak));
        }

        [Test]
        public void GainResource_Radiance_RoutesToStatus()
        {
            var src = new TestCombatant();
            var ctx = new TestEffectContext { Source = src, Target = new TestCombatant() };

            new EffectDispatcher().Execute(
                new EffectData(EffectOp.GainResource, amount: 1, target: TargetType.Self, resource: ResourceType.Radiance), ctx);

            Assert.AreEqual(1, src.GetStatus(StatusType.Radiance));
        }

        [Test]
        public void Draw_AddsCardsToHand()
        {
            var card = StarterContent.LightStrike();
            var draw = new List<CardInstance>();
            for (int i = 0; i < 5; i++) draw.Add(new CardInstance(card, i));

            var ctx = new TestEffectContext
            {
                Source = new TestCombatant(),
                Target = new TestCombatant(),
                DrawPile = draw
            };

            new EffectDispatcher().Execute(new EffectData(EffectOp.Draw, amount: 2), ctx);

            Assert.AreEqual(2, ctx.Hand.Count);
            Assert.AreEqual(3, ctx.DrawPile.Count);
        }
    }
}

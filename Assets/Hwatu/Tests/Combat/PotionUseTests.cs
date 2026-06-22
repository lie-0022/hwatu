using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;

namespace Hwatu.Tests.Combat
{
    public class PotionUseTests
    {
        [Test]
        public void UsePotion_BlockPotion_GivesBlock_InCombat()
        {
            var deck = StarterContent.LuminaryStarterDeck();
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction

            bool used = engine.UsePotion(PotionContent.Block());
            Assert.IsTrue(used);
            Assert.AreEqual(14, state.Player.Block);   // 방패약 +14
        }

        [Test]
        public void UsePotion_FailsOutsidePlayerAction()
        {
            var deck = StarterContent.LuminaryStarterDeck();
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            // Advance 전 → CombatStart phase
            Assert.IsFalse(engine.UsePotion(PotionContent.Block()));
        }

        [Test]
        public void UsePotion_Heal_RestoresHp_InCombat()
        {
            var deck = StarterContent.LuminaryStarterDeck();
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            state.Player.SetHp(60);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction
            engine.UsePotion(PotionContent.Heal());
            Assert.AreEqual(80, state.Player.Hp);   // 60+20 (max 80)
        }
    }
}

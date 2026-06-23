using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Combat
{
    /// <summary>백호 산군 위엄(Majesty) — 모든 Attack 피해 가산(Radiance 백호판). 정기주입으로 성장.</summary>
    public class BeastMajestyTests
    {
        [Test]
        public void Majesty_AddsToAttackDamage()
        {
            // 위엄 3 + 발톱질(base5) → 8 데미지
            var deck = new List<CardData> { BeastCards.Claw() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            state.Player.AddStatus(StatusType.Majesty, 3);
            int enemyHp = state.Enemies[0].Hp;
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();
            engine.PlayCard(0);
            Assert.AreEqual(enemyHp - 8, state.Enemies[0].Hp, "발톱질5 + 위엄3 = 8");
        }

        [Test]
        public void InfuseSpirit_GrantsMajesty()
        {
            var deck = new List<CardData> { BeastCards.InfuseSpirit() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();
            engine.PlayCard(0);
            Assert.AreEqual(1, state.Player.GetStatus(StatusType.Majesty), "정기 주입 → 위엄 1");
        }

        [Test]
        public void BeastStarterDeck_HasTen()
        {
            Assert.AreEqual(10, BeastCards.BeastStarterDeck().Count);
        }

        [Test]
        public void Beast_Character_Hp80_Deck10()
        {
            CharacterData beast = CharacterData.Beast();
            Assert.AreEqual(80, beast.StartMaxHp);
            Assert.AreEqual(10, beast.StartingDeck.Count);
            Assert.AreEqual("백호 산군", beast.Name);
        }

        [Test]
        public void BeastRewardPool_HasTen()
        {
            Assert.AreEqual(10, BeastCards.RewardPool().Count);
        }

        [Test]
        public void CharacterPools_Beast_UsesBeastPool()
        {
            Assert.AreEqual(10, CharacterPools.RewardPool("beast").Count);
        }
    }
}

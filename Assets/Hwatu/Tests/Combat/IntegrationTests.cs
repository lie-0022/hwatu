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

        [Test]
        public void WillOWisp_Scald_PoisonsPlayer_OverTurns()
        {
            var deck = new List<CardData> { StarterContent.Shield() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.WillOWisp(), 999, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            int poison = 0;
            for (int t = 0; t < 14; t++)
            {
                CombatPhase ph = engine.Advance();
                if (ph == CombatPhase.PlayerAction) engine.EndTurn();
                poison = state.Player.GetStatus(StatusType.Poison);
                if (poison > 0) break;
            }
            Assert.AreEqual(3, poison);   // 도깨비불 scald가 플레이어에 중독 3
        }

        [Test]
        public void Ink_Poison_Stacks_FromLacquer()
        {
            var deck = new List<CardData> { InkCards.Lacquer(), InkCards.Lacquer() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 70, 70);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction (옻칠 2장)
            engine.PlayCard(0);   // 적 중독 4
            engine.PlayCard(0);   // 적 중독 8 (누적)
            Assert.AreEqual(8, state.Enemies[0].GetStatus(StatusType.Poison));
        }

        [Test]
        public void Luminary_Radiance_BoostsLightStrike()
        {
            var deck = new List<CardData> { StarterContent.Ignite(), StarterContent.LightStrike() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction

            int igniteIdx = state.Hand.FindIndex(c => c.Data.Id == "luminary_ignite");
            engine.PlayCard(igniteIdx);   // 광(Radiance) +1
            int hp = state.Enemies[0].Hp;
            int strikeIdx = state.Hand.FindIndex(c => c.Data.Id == "luminary_light_strike");
            engine.PlayCard(strikeIdx);   // 빛타격 6 + 광1 = 7
            Assert.AreEqual(hp - 7, state.Enemies[0].Hp);
        }

        [Test]
        public void InkStarterDeck_DrawsAndShadowBladeHits()
        {
            var deck = InkCards.InkStarterDeck();
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 70, 70);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction

            Assert.AreEqual(5, state.Hand.Count);   // 묵귀 시작 덱 드로우 5
            int bladeIdx = state.Hand.FindIndex(c => c.Data.Id == "ink_blade");
            Assert.GreaterOrEqual(bladeIdx, 0);
            int hp = state.Enemies[0].Hp;
            engine.PlayCard(bladeIdx);
            Assert.AreEqual(hp - 6, state.Enemies[0].Hp);   // 그림자칼 6
        }

        [Test]
        public void Gumiho_Enraged_AdvancesToDoom()
        {
            var deck = InkCards.InkStarterDeck();
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.Gumiho(), 1, 70, 70);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance();

            EnemyState e = state.Enemies[0];
            e.SetHp(e.MaxHp / 3);   // enrage → phase2 [ninetails, doom, tail]
            e.RefreshIntent();
            Assert.AreEqual("ninetails", e.CurrentIntent.Id);
            e.Ai.Advance();
            e.RefreshIntent();
            Assert.AreEqual("doom", e.CurrentIntent.Id);
            Assert.AreEqual(IntentType.Doom, e.CurrentIntent.Intent);
        }

        [Test]
        public void General_Rally_GivesSelfRadiance()
        {
            var deck = StarterContent.LuminaryStarterDeck();
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.General(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction
            engine.EndTurn();
            engine.Advance(); engine.Advance(); // EnemyTurn1(rally) → CheckDeath

            // 장군 AiOrder [rally, ...] — 첫 적턴 rally가 자기 광 +3(이후 공격 강화)
            Assert.AreEqual(3, state.Enemies[0].GetStatus(StatusType.Radiance));
        }

        [Test]
        public void Ink_DarkGuard_GivesBlock()
        {
            var deck = new List<CardData> { InkCards.DarkGuard() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 70, 70);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction
            engine.PlayCard(0);
            Assert.AreEqual(8, state.Player.Block);   // 암막 방어 8
        }

        [Test]
        public void Luminary_Check_AppliesWeakToEnemy()
        {
            var deck = new List<CardData> { LuminaryCards.Check() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction
            engine.PlayCard(0);   // 견제 → 적 피해 5 + 약화 1
            Assert.AreEqual(1, state.Enemies[0].GetStatus(StatusType.Weak));
        }

        [Test]
        public void Ink_Veil_AppliesVulnerable()
        {
            var deck = new List<CardData> { InkCards.Veil() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 70, 70);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction
            engine.PlayCard(0);   // 흑무 → 적 취약 2
            Assert.AreEqual(2, state.Enemies[0].GetStatus(StatusType.Vulnerable));
        }

        [Test]
        public void Luminary_RadiantSurge_GivesRadiance3()
        {
            var deck = new List<CardData> { LuminaryCards.RadiantSurge() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 80, 80);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction
            engine.PlayCard(0);   // 광휘진 → 광 +3
            Assert.AreEqual(3, state.Player.GetStatus(StatusType.Radiance));
        }

        [Test]
        public void Ink_InkStrike_DealsDamageAndPoison()
        {
            var deck = new List<CardData> { InkCards.InkStrike() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 70, 70);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction
            int hp = state.Enemies[0].Hp;
            engine.PlayCard(0);   // 먹칼 → 피해 5 + 중독 2
            Assert.AreEqual(hp - 5, state.Enemies[0].Hp);
            Assert.AreEqual(2, state.Enemies[0].GetStatus(StatusType.Poison));
        }

        [Test]
        public void Ink_Seep_DealsDamageAndPoison()
        {
            var deck = new List<CardData> { InkCards.Seep() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.DokkaebiMinion(), 1, 70, 70);
            var engine = new CombatEngine(state, new EffectDispatcher());
            engine.Advance(); engine.Advance(); // PlayerAction
            int hp = state.Enemies[0].Hp;
            engine.PlayCard(0);   // 침습 → 피해 7 + 중독 3
            Assert.AreEqual(hp - 7, state.Enemies[0].Hp);
            Assert.AreEqual(3, state.Enemies[0].GetStatus(StatusType.Poison));
        }

        [Test]
        public void Gumiho_Doom_Telegraphs_ThenFires()
        {
            var deck = new List<CardData> { StarterContent.Shield() };
            CombatState state = CombatFactory.CreateCombat(deck, StarterContent.Gumiho(), 1, 999, 999);
            var engine = new CombatEngine(state, new EffectDispatcher());
            EnemyState e = state.Enemies[0];
            engine.Advance(); engine.Advance();
            e.SetHp(e.MaxHp / 3);   // 광폭 → phase2 [ninetails, doom, tail]
            e.RefreshIntent();

            var dmgs = new List<int>();
            for (int t = 0; t < 16; t++)
            {
                if (state.Phase == CombatPhase.PlayerAction) engine.EndTurn();
                int before = state.Player.Hp;
                engine.Advance();
                if (state.Phase == CombatPhase.CheckDeath) dmgs.Add(before - state.Player.Hp);
            }
            int idx0 = dmgs.IndexOf(0);
            Assert.GreaterOrEqual(idx0, 0, "doom 예고 턴(피해 0)이 있어야 한다");
            Assert.AreEqual(28, dmgs[idx0 + 1], "예고 다음 적 턴에 doom 발동 28");
        }
    }
}

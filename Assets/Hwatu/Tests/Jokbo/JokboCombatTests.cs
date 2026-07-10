using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;
using Hwatu.Core.Jokbo;

namespace Hwatu.Tests.Jokbo
{
    /// <summary>족보 전투 엔진(J2) — 헤드리스 승/패·턴 경제(내기/버리기/유지형 손패)·족보별 효과 적용.</summary>
    public class JokboCombatTests
    {
        private static HwatuCardData C(int month, HwatuCardKind kind, bool dbl = false)
            => new HwatuCardData(month, kind, dbl);

        private static JokboCombatEngine Engine(List<HwatuCardData> deck, EnemyData enemy, int hp = 80)
            => JokboCombatFactory.Create(deck, enemy, masterSeed: 7, playerMaxHp: hp, playerHp: hp);

        private static void ToPlayerAction(JokboCombatEngine eng)
        {
            for (int i = 0; i < 8 && eng.State.Phase != CombatPhase.PlayerAction; i++)
            {
                CombatPhase p = eng.Advance();
                if (p == CombatPhase.Win || p == CombatPhase.Lose) { return; }
            }
        }

        [Test]
        public void FiveBrights_Aoe120_WinsInstantly()
        {
            // 오광: 광 5장 = 공격 40 ×3 = 120 전체 — 잡도깨비 원킬(런의 꿈 검증)
            var deck = new List<HwatuCardData>
            {
                C(1, HwatuCardKind.Bright), C(3, HwatuCardKind.Bright), C(8, HwatuCardKind.Bright),
                C(11, HwatuCardKind.Bright), C(12, HwatuCardKind.Bright),
            };
            JokboCombatEngine eng = Engine(deck, StarterContent.DokkaebiMinion());
            ToPlayerAction(eng);
            bool ok = eng.PlayJokbo(new[] { 0, 1, 2, 3, 4 });
            Assert.IsTrue(ok, "오광 제출 성공");
            Assert.AreEqual(CombatResult.Win, eng.Result, "120 전체로 즉시 승리");
        }

        [Test]
        public void Pair_AppliesAttackAndBlockTogether()
        {
            // 먹기(열끗6+피3) ×1 → 적 피해 6 + 내 방어 3 (각자 합산 — 설계 §4.3 확정)
            var deck = new List<HwatuCardData> { C(2, HwatuCardKind.Animal), C(2, HwatuCardKind.Chaff) };
            JokboCombatEngine eng = Engine(deck, StarterContent.General());
            ToPlayerAction(eng);
            int enemyHp = eng.State.Enemies[0].Hp;
            Assert.IsTrue(eng.PlayJokbo(new[] { 0, 1 }));
            Assert.AreEqual(enemyHp - 6, eng.State.Enemies[0].Hp, "공격합 6");
            Assert.AreEqual(3, eng.State.Player.Block, "방어합 3");
            Assert.AreEqual(JokboRules.PlaysPerTurn - 1, eng.PlaysLeft, "내기 1회 소모");
        }

        [Test]
        public void TurnEconomy_TwoPlays_TwoDiscards_ThenBlocked()
        {
            var deck = new List<HwatuCardData>();
            for (int i = 0; i < 10; i++) { deck.Add(C(1, HwatuCardKind.Chaff)); }
            JokboCombatEngine eng = Engine(deck, StarterContent.General());
            ToPlayerAction(eng);

            Assert.IsTrue(eng.PlayJokbo(new[] { 0 }), "내기 1");
            Assert.IsTrue(eng.PlayJokbo(new[] { 0 }), "내기 2");
            Assert.IsFalse(eng.PlayJokbo(new[] { 0 }), "내기 3회째는 차단");

            Assert.IsTrue(eng.DiscardCards(new[] { 0 }), "버리기 1");
            Assert.IsTrue(eng.DiscardCards(new[] { 0 }), "버리기 2");
            Assert.IsFalse(eng.DiscardCards(new[] { 0 }), "버리기 3회째는 차단");
        }

        [Test]
        public void Discard_DrawsSameCount()
        {
            var deck = new List<HwatuCardData>();
            for (int i = 0; i < 12; i++) { deck.Add(C(1, HwatuCardKind.Chaff)); }
            JokboCombatEngine eng = Engine(deck, StarterContent.General());
            ToPlayerAction(eng);
            Assert.AreEqual(JokboRules.HandSize, eng.Hand.Count, "첫 손패 8");
            Assert.IsTrue(eng.DiscardCards(new[] { 0, 1, 2 }));
            Assert.AreEqual(JokboRules.HandSize, eng.Hand.Count, "3장 버리고 3장 드로우 — 손패 유지");
        }

        [Test]
        public void HandPersists_AcrossTurns_RefillTo8()
        {
            // 손패 유지형(설계 §3): 턴 종료에 버리지 않고, 다음 턴 시작에 8장까지 리필
            var deck = new List<HwatuCardData>();
            for (int i = 0; i < 12; i++) { deck.Add(C(1, HwatuCardKind.Chaff)); }
            JokboCombatEngine eng = Engine(deck, StarterContent.DokkaebiMinion(), hp: 200);
            ToPlayerAction(eng);
            Assert.IsTrue(eng.PlayJokbo(new[] { 0 }));
            int kept = eng.Hand.Count;
            Assert.AreEqual(7, kept, "낱장 1장 소모");
            Assert.IsTrue(eng.EndTurn());
            ToPlayerAction(eng);
            Assert.AreEqual(JokboRules.HandSize, eng.Hand.Count, "다음 턴 8장 리필");
        }

        [Test]
        public void Godori_SplitsThreeHits_AndDraws2()
        {
            // 고도리(2·4·8 새): 공격 18 ×2 = 36을 3연타(12×3) + 드로우 2
            var deck = new List<HwatuCardData>
            {
                C(2, HwatuCardKind.Animal), C(4, HwatuCardKind.Animal), C(8, HwatuCardKind.Animal),
                C(1, HwatuCardKind.Chaff), C(1, HwatuCardKind.Chaff),
                C(3, HwatuCardKind.Chaff), C(3, HwatuCardKind.Chaff), C(5, HwatuCardKind.Chaff),
            };
            JokboCombatEngine eng = Engine(deck, StarterContent.General());
            ToPlayerAction(eng);
            var birds = new List<int>();
            for (int i = 0; i < eng.Hand.Count; i++)
            {
                if (eng.Hand[i].Data.IsGodoriBird) { birds.Add(i); }
            }
            Assert.AreEqual(3, birds.Count, "새 3장이 손에");
            int enemyHp = eng.State.Enemies[0].Hp;
            Assert.IsTrue(eng.PlayJokbo(birds));
            Assert.AreEqual(enemyHp - 36, eng.State.Enemies[0].Hp, "총 36 피해(12×3)");
            Assert.AreEqual(7, eng.Hand.Count, "8-3장 제출 +2 드로우 = 7");
        }

        [Test]
        public void RedRibbons_AppliesBurnSignature()
        {
            // 홍단: 공격 12 ×2 = 24 + 화상(중독) 4
            var deck = new List<HwatuCardData>
            {
                C(1, HwatuCardKind.Ribbon), C(2, HwatuCardKind.Ribbon), C(3, HwatuCardKind.Ribbon),
                C(5, HwatuCardKind.Chaff), C(5, HwatuCardKind.Chaff),
            };
            JokboCombatEngine eng = Engine(deck, StarterContent.General());
            ToPlayerAction(eng);
            var ribbons = new List<int>();
            for (int i = 0; i < eng.Hand.Count; i++)
            {
                if (eng.Hand[i].Data.Kind == HwatuCardKind.Ribbon) { ribbons.Add(i); }
            }
            int enemyHp = eng.State.Enemies[0].Hp;
            Assert.IsTrue(eng.PlayJokbo(ribbons));
            Assert.AreEqual(enemyHp - 24, eng.State.Enemies[0].Hp, "12×2 = 24");
            Assert.AreEqual(JokboRules.RedRibbonsBurn, eng.State.Enemies[0].GetStatus(StatusType.Poison), "화상 4");
        }

        [Test]
        public void Bomb_HitsAllEnemies_AndBlocks()
        {
            // 폭탄(11월 4장: 광8 + 피3+피3+쌍피6): 공격 8×3=24 전체, 방어 12×3=36
            var deck = new List<HwatuCardData>
            {
                C(11, HwatuCardKind.Bright), C(11, HwatuCardKind.Chaff),
                C(11, HwatuCardKind.Chaff), C(11, HwatuCardKind.Chaff, dbl: true),
            };
            var enemies = new List<EnemyData> { StarterContent.General(), StarterContent.General() };
            JokboCombatEngine eng = JokboCombatFactory.Create(deck, enemies, 7, 80, 80);
            ToPlayerAction(eng);
            int hp0 = eng.State.Enemies[0].Hp;
            int hp1 = eng.State.Enemies[1].Hp;
            Assert.IsTrue(eng.PlayJokbo(new[] { 0, 1, 2, 3 }));
            Assert.AreEqual(hp0 - 24, eng.State.Enemies[0].Hp, "적1도 24");
            Assert.AreEqual(hp1 - 24, eng.State.Enemies[1].Hp, "적2도 24");
            Assert.AreEqual(36, eng.State.Player.Block, "방어 12×3");
        }

        [Test]
        public void InvalidJokbo_Rejected_NoPlayConsumed()
        {
            var deck = new List<HwatuCardData> { C(1, HwatuCardKind.Chaff), C(2, HwatuCardKind.Chaff) };
            JokboCombatEngine eng = Engine(deck, StarterContent.General());
            ToPlayerAction(eng);
            Assert.IsFalse(eng.PlayJokbo(new[] { 0, 1 }), "다른 월 2장은 족보 아님");
            Assert.AreEqual(JokboRules.PlaysPerTurn, eng.PlaysLeft, "내기 미소모");
        }

        [Test]
        public void IdlePlayer_EventuallyLoses()
        {
            var deck = new List<HwatuCardData> { C(1, HwatuCardKind.Chaff) };
            JokboCombatEngine eng = Engine(deck, StarterContent.General());
            for (int t = 0; t < 200 && eng.Result == CombatResult.InProgress; t++)
            {
                CombatPhase p = eng.Advance();
                if (p == CombatPhase.PlayerAction) { eng.EndTurn(); }
            }
            Assert.AreEqual(CombatResult.Lose, eng.Result, "아무것도 안 내면 패배");
        }

        [Test]
        public void StarterDeck_CanFightFullCombat_WithPairsOnly()
        {
            // 공통 시작 덱(일곱 달) — 페어 그리디만으로 잡도깨비를 이길 수 있어야 한다(기본기 성립 검증)
            JokboCombatEngine eng = Engine(HwatuDeckContent.CommonStarterDeck(), StarterContent.DokkaebiMinion());
            for (int t = 0; t < 400 && eng.Result == CombatResult.InProgress; t++)
            {
                CombatPhase p = eng.Advance();
                if (p != CombatPhase.PlayerAction) { continue; }

                // 그리디: 손패에서 같은 월 페어를 찾아 낸다(없으면 낱장 최강 공격, 그것도 없으면 종료)
                bool played = false;
                for (int pl = 0; pl < 2; pl++)
                {
                    int a = -1, b = -1;
                    for (int i = 0; i < eng.Hand.Count && a < 0; i++)
                    {
                        for (int j = i + 1; j < eng.Hand.Count; j++)
                        {
                            if (eng.Hand[i].Data.Month == eng.Hand[j].Data.Month) { a = i; b = j; break; }
                        }
                    }
                    if (a >= 0) { played = eng.PlayJokbo(new[] { a, b }) || played; }
                    else if (eng.Hand.Count > 0) { played = eng.PlayJokbo(new[] { 0 }) || played; }
                    if (eng.Result == CombatResult.Win) { break; }
                }
                if (eng.Result == CombatResult.InProgress) { eng.EndTurn(); }
            }
            Assert.AreEqual(CombatResult.Win, eng.Result, "시작 덱 페어 그리디로 일반 적 승리");
        }
    }
}

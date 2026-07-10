using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Jokbo;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Jokbo
{
    /// <summary>족보 런 흐름(J5) — 맵→전투→보상→다음의 전체 루프 헤드리스 검증.</summary>
    public class JokboRunFlowTests
    {
        [Test]
        public void StartNewRun_GeneratesMap_AndCommonDeck()
        {
            var flow = new JokboRunFlow();
            flow.StartNewRun(42);
            Assert.AreEqual(JokboRunPhase.Map, flow.Phase);
            Assert.IsNotNull(flow.Run.Map);
            Assert.AreEqual(14, flow.Run.Deck.Count);
        }

        [Test]
        public void EnterCombatNode_StartsCombat_ThenWinGivesReward()
        {
            var flow = new JokboRunFlow();
            flow.StartNewRun(42);
            MapNode combat = FirstCombatNode(flow);
            Assert.IsNotNull(combat, "맵에 전투 노드 존재");

            flow.EnterNode(combat);
            Assert.AreEqual(JokboRunPhase.Combat, flow.Phase);
            Assert.IsNotNull(flow.Combat);

            // 시뮬 AI로 전투 자동 진행 → 종료 콜백
            CombatResult result = JokboSimAi.RunCombat(flow.Combat);
            flow.OnCombatEnded(result == CombatResult.Win);

            if (result == CombatResult.Win)
            {
                Assert.AreEqual(JokboRunPhase.Reward, flow.Phase);
                Assert.AreEqual(3, flow.RewardCards.Count, "카드 보상 3택");
            }
            else
            {
                Assert.AreEqual(JokboRunPhase.GameOver, flow.Phase);
            }
        }

        [Test]
        public void TakeReward_AddsChosenCard_ReturnsToMap()
        {
            var flow = new JokboRunFlow();
            flow.StartNewRun(7);
            MapNode combat = FirstCombatNode(flow);
            flow.EnterNode(combat);
            CombatResult result = JokboSimAi.RunCombat(flow.Combat);
            if (result != CombatResult.Win) { Assert.Ignore("시드가 패배 — 다른 테스트에서 커버"); }
            flow.OnCombatEnded(true);

            int before = flow.Run.Deck.Count;
            flow.TakeReward(0);
            Assert.AreEqual(before + 1, flow.Run.Deck.Count, "선택 카드 덱 추가");
            Assert.AreEqual(JokboRunPhase.Map, flow.Phase);
            Assert.IsNull(flow.RewardCards);
        }

        [Test]
        public void SkipReward_DeckUnchanged()
        {
            var flow = new JokboRunFlow();
            flow.StartNewRun(7);
            MapNode combat = FirstCombatNode(flow);
            flow.EnterNode(combat);
            CombatResult result = JokboSimAi.RunCombat(flow.Combat);
            if (result != CombatResult.Win) { Assert.Ignore("시드 패배"); }
            flow.OnCombatEnded(true);
            int before = flow.Run.Deck.Count;
            flow.TakeReward(-1);
            Assert.AreEqual(before, flow.Run.Deck.Count, "넘기면 덱 불변");
        }

        [Test]
        public void CombatHpCarriesToRun()
        {
            var flow = new JokboRunFlow();
            flow.StartNewRun(3, startHp: 80);
            MapNode combat = FirstCombatNode(flow);
            flow.EnterNode(combat);
            JokboSimAi.RunCombat(flow.Combat);
            int combatHp = flow.Combat.State.Player.Hp;
            flow.OnCombatEnded(flow.Combat.Result == CombatResult.Win);
            Assert.AreEqual(combatHp, flow.Run.Hp, "전투 종료 HP가 런에 반영");
        }

        [Test]
        public void Rest_HealsTowardMax()
        {
            var flow = new JokboRunFlow();
            flow.StartNewRun(1, startHp: 80);
            flow.Run.Hp = 40;
            flow.OnRest(0);
            Assert.AreEqual(40 + 80 * 30 / 100, flow.Run.Hp);
            Assert.AreEqual(JokboRunPhase.Map, flow.Phase);
        }

        private static MapNode FirstCombatNode(JokboRunFlow flow)
        {
            for (int id = 0; id < 256; id++)
            {
                MapNode n = flow.Run.Map.GetNode(id);
                if (n != null && n.Type == NodeType.Combat && n.OnPath)
                {
                    return n;
                }
            }
            return null;
        }
    }
}

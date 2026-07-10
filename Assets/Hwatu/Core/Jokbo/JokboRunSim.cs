using System.Collections.Generic;
using Hwatu.Core.Combat;
using Hwatu.Core.Run;

namespace Hwatu.Core.Jokbo
{
    /// <summary>
    /// 족보 런 자동 완주 시뮬(J5 완료 기준 검증) — 맵 경로를 따라 전투(JokboSimAi)/보상/휴식/상점/이벤트를
    /// 자동 처리해 Victory 또는 GameOver까지 구동한다. UI 없이 "캐릭터 없는 공통 덱으로 런이 완주 가능한가"를 측정.
    /// </summary>
    public static class JokboRunSim
    {
        public static JokboRunPhase RunFull(JokboRunFlow flow, int maxSteps = 2000)
        {
            int steps = 0;
            while (steps++ < maxSteps)
            {
                switch (flow.Phase)
                {
                    case JokboRunPhase.Map:
                        MapNode next = PickNextNode(flow.Run);
                        if (next == null) { return flow.Phase; }
                        flow.EnterNode(next);
                        break;
                    case JokboRunPhase.Combat:
                        CombatResult r = JokboSimAi.RunCombat(flow.Combat);
                        flow.OnCombatEnded(r == CombatResult.Win);
                        break;
                    case JokboRunPhase.Reward:
                        flow.TakeReward(0);   // 항상 첫 보상 수령(그리디)
                        break;
                    case JokboRunPhase.Rest:
                        flow.OnRest(0);       // 회복 우선
                        break;
                    case JokboRunPhase.Shop:
                        flow.LeaveShop();
                        break;
                    case JokboRunPhase.Event:
                        flow.ResolveEventSkip();
                        break;
                    case JokboRunPhase.GameOver:
                    case JokboRunPhase.Victory:
                        return flow.Phase;
                }
            }
            return flow.Phase;
        }

        // 현재 노드에서 다음 노드: 시작 전이면 첫 시작 노드, 최상단이면 보스, 아니면 첫 연결 노드.
        private static MapNode PickNextNode(JokboRunState run)
        {
            if (run.CurrentNodeId < 0)
            {
                List<MapNode> starts = run.Map.StartNodes();
                return starts.Count > 0 ? starts[0] : null;
            }
            MapNode cur = run.Map.GetNode(run.CurrentNodeId);
            if (cur == null || cur.NextIds.Count == 0)
            {
                return run.Map.Boss;
            }
            return run.Map.GetNode(cur.NextIds[0]);
        }
    }
}

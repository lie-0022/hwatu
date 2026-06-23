using Hwatu.Core.Cards;
using Hwatu.Core.Combat;

namespace Hwatu.Core.Content
{
    /// <summary>
    /// 밸런스 시뮬용 간단 휴리스틱 AI. 플레이어 턴을 자동 진행한다.
    /// 에너지로 낼 수 있는 카드를 순서대로 소진(공격 우선) 후 턴 종료.
    /// 헤드리스 자동 대전으로 승률·턴수·잔여HP 집계에 사용한다(BalanceSim).
    /// </summary>
    public static class SimAi
    {
        /// <summary>한 전투를 끝까지 자동 진행하고 결과를 반환한다(turnCap 초과 시 방어 종료).</summary>
        public static CombatResult RunCombat(CombatEngine engine, int turnCap = 60)
        {
            int guard = 0;
            while (engine.Result == CombatResult.InProgress && guard++ < turnCap * 6)
            {
                CombatPhase ph = engine.Advance();
                if (ph == CombatPhase.PlayerAction)
                {
                    PlayPlayerTurn(engine);
                }
            }
            return engine.Result;
        }

        /// <summary>플레이어 턴: 에너지로 가능한 카드를 우선순위로 소진한 뒤 턴 종료.</summary>
        private static void PlayPlayerTurn(CombatEngine engine)
        {
            CombatState s = engine.State;
            int safety = 0;
            while (s.Phase == CombatPhase.PlayerAction && safety++ < 30)
            {
                int pick = ChooseCard(s);
                if (pick < 0)
                {
                    break;
                }
                engine.PlayCard(pick, FirstAliveEnemy(s));
            }
            if (s.Phase == CombatPhase.PlayerAction)
            {
                engine.EndTurn();
            }
        }

        /// <summary>낼 수 있는 카드 중 공격 우선, 없으면 첫 가능 스킬. 가능 카드 없으면 -1.</summary>
        private static int ChooseCard(CombatState s)
        {
            int fallback = -1;
            for (int i = 0; i < s.Hand.Count; i++)
            {
                CardInstance card = s.Hand[i];
                if (card.Data.Cost > s.Player.Energy)
                {
                    continue;
                }
                if (card.Data.Type == CardType.Attack)
                {
                    return i;
                }
                if (fallback < 0)
                {
                    fallback = i;
                }
            }
            return fallback;
        }

        /// <summary>살아있는 첫 적 인덱스(다중 적 — 순차 처치로 적 수를 빠르게 줄인다). 없으면 0.</summary>
        private static int FirstAliveEnemy(CombatState s)
        {
            for (int i = 0; i < s.Enemies.Count; i++)
            {
                if (!s.Enemies[i].IsDead) { return i; }
            }
            return 0;
        }
    }
}

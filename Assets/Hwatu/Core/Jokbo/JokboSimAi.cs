using System.Collections.Generic;
using Hwatu.Core.Combat;

namespace Hwatu.Core.Jokbo
{
    /// <summary>
    /// 족보 전투 자동 플레이어(J3 시뮬용 그리디). 손패(≤8장)의 모든 부분집합(≤5장)을 전수 평가해
    /// 가장 가치(공격+방어, 배수 적용 후) 높은 족보를 낸다. 버리기는 짝 없는 월부터.
    /// 밸런스 측정용 baseline — 명명 족보 셋업(재료 보존) 같은 고급 판단은 하지 않는다.
    /// </summary>
    public static class JokboSimAi
    {
        /// <summary>전투를 끝까지 자동 진행(최대 maxTurns). 종료 시 Result 반환.</summary>
        public static CombatResult RunCombat(JokboCombatEngine engine, int maxTurns = 60)
        {
            while (engine.Result == CombatResult.InProgress && engine.State.Turn <= maxTurns)
            {
                CombatPhase phase = engine.Advance();
                if (phase == CombatPhase.Win || phase == CombatPhase.Lose)
                {
                    break;
                }
                if (phase != CombatPhase.PlayerAction)
                {
                    continue;
                }

                // 1) 버리기: 짝(같은 월) 없는 카드를 최대 3장씩 교체 — 페어 확률을 올린다
                while (engine.DiscardsLeft > 0)
                {
                    List<int> junk = UnpairableIndices(engine.Hand, 3);
                    if (junk.Count == 0) { break; }
                    if (!engine.DiscardCards(junk)) { break; }
                }

                // 2) 내기: 남은 횟수만큼 최고 가치 족보 제출
                while (engine.PlaysLeft > 0 && engine.Result == CombatResult.InProgress)
                {
                    List<int> best = FindBestSubset(engine.Hand);
                    if (best == null) { break; }
                    int target = FirstAliveEnemy(engine.State);
                    if (target < 0) { break; }
                    if (!engine.PlayJokbo(best, target)) { break; }
                }

                if (engine.Result == CombatResult.InProgress)
                {
                    engine.EndTurn();
                }
            }
            return engine.Result;
        }

        /// <summary>손패 부분집합(1~5장) 전수 평가 — (공격+방어)×배수가 최대인 유효 족보. 없으면 null.</summary>
        public static List<int> FindBestSubset(IReadOnlyList<JokboCardInstance> hand)
        {
            int n = hand.Count;
            if (n == 0) { return null; }
            if (n > 12) { n = 12; }   // 안전 상한(손패 8 기준 255 마스크)

            List<int> best = null;
            int bestValue = 0;
            int bestCount = int.MaxValue;
            var cards = new List<HwatuCardData>(5);
            var indices = new List<int>(5);

            for (int mask = 1; mask < (1 << n); mask++)
            {
                int bits = PopCount(mask);
                if (bits > JokboRules.MaxSubmitCards) { continue; }

                cards.Clear();
                indices.Clear();
                for (int i = 0; i < n; i++)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        cards.Add(hand[i].Data);
                        indices.Add(i);
                    }
                }

                JokboType jokbo = JokboDetector.Detect(cards);
                if (jokbo == JokboType.None) { continue; }

                int num = JokboRules.MultNum(jokbo);
                int den = JokboRules.MultDen(jokbo);
                int value = JokboRules.AttackSum(cards) * num / den + JokboRules.DefenseSum(cards) * num / den;
                if (value > bestValue || (value == bestValue && bits < bestCount))
                {
                    bestValue = value;
                    bestCount = bits;
                    best = new List<int>(indices);
                }
            }
            return best;
        }

        // 같은 월 짝이 손에 없는 카드 인덱스(최대 limit장) — 버리기 후보.
        private static List<int> UnpairableIndices(IReadOnlyList<JokboCardInstance> hand, int limit)
        {
            var result = new List<int>();
            for (int i = 0; i < hand.Count && result.Count < limit; i++)
            {
                bool hasPartner = false;
                for (int j = 0; j < hand.Count; j++)
                {
                    if (i != j && hand[i].Data.Month == hand[j].Data.Month)
                    {
                        hasPartner = true;
                        break;
                    }
                }
                if (!hasPartner) { result.Add(i); }
            }
            return result;
        }

        private static int FirstAliveEnemy(CombatState state)
        {
            for (int i = 0; i < state.Enemies.Count; i++)
            {
                if (!state.Enemies[i].IsDead) { return i; }
            }
            return -1;
        }

        private static int PopCount(int v)
        {
            int c = 0;
            while (v != 0) { v &= v - 1; c++; }
            return c;
        }
    }
}

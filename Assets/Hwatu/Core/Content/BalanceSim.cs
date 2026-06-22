using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;

namespace Hwatu.Core.Content
{
    /// <summary>
    /// 헤드리스 자동 대전 배치 집계(<see cref="SimAi"/> 사용).
    /// 캐릭터 시작덱 vs 적을 N회(시드 1..N) 자동 전투해 승률·평균턴·승리 시 평균 잔여HP를 낸다.
    /// 밸런스 튜닝 후보(승률 극단) 식별용. 결정론 시드라 재현 가능.
    /// </summary>
    public static class BalanceSim
    {
        /// <summary>한 (덱 vs 적) 대진의 집계 결과.</summary>
        public struct Matchup
        {
            public string Enemy;
            public int Wins;
            public int Trials;
            public int WinRatePct;
            public int AvgTurns;
            public int AvgHpLeftOnWin;
        }

        /// <summary>deck로 enemy를 trials회(시드 1..trials) 자동 전투해 집계한다.</summary>
        public static Matchup Simulate(IReadOnlyList<CardData> deck, EnemyData enemy, int maxHp, int trials)
        {
            int wins = 0;
            int totalTurns = 0;
            int totalHpOnWin = 0;
            for (int t = 0; t < trials; t++)
            {
                CombatState state = CombatFactory.CreateCombat(deck, enemy, (ulong)(t + 1), maxHp, maxHp);
                var engine = new CombatEngine(state, new EffectDispatcher());
                CombatResult result = SimAi.RunCombat(engine);
                totalTurns += state.Turn;
                if (result == CombatResult.Win)
                {
                    wins++;
                    totalHpOnWin += state.Player.Hp;
                }
            }
            return new Matchup
            {
                Enemy = enemy.Name,
                Wins = wins,
                Trials = trials,
                WinRatePct = trials > 0 ? wins * 100 / trials : 0,
                AvgTurns = trials > 0 ? totalTurns / trials : 0,
                AvgHpLeftOnWin = wins > 0 ? totalHpOnWin / wins : 0,
            };
        }
    }
}

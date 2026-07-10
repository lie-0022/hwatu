using System.Collections.Generic;
using Hwatu.Core.Combat;
using Hwatu.Core.Enemies;

namespace Hwatu.Core.Jokbo
{
    /// <summary>족보 전투 밸런스 시뮬(J3) — JokboSimAi로 다회 대전해 승률·평균 턴·잔여 HP를 잰다(기존 BalanceSim 대응).</summary>
    public static class JokboBalanceSim
    {
        public sealed class Matchup
        {
            public int WinRatePct;
            public int AvgTurns;
            public int AvgHpLeftOnWin;
        }

        public static Matchup Simulate(IReadOnlyList<HwatuCardData> deck, EnemyData enemy, int maxHp, int trials, ulong baseSeed = 1000)
        {
            return SimulateMulti(deck, new[] { enemy }, maxHp, trials, baseSeed);
        }

        public static Matchup SimulateMulti(IReadOnlyList<HwatuCardData> deck, IReadOnlyList<EnemyData> enemies, int maxHp, int trials, ulong baseSeed = 1000)
        {
            int wins = 0, turnSum = 0, hpSum = 0;
            for (int t = 0; t < trials; t++)
            {
                JokboCombatEngine engine = JokboCombatFactory.Create(deck, enemies, baseSeed + (ulong)t, maxHp, maxHp);
                CombatResult result = JokboSimAi.RunCombat(engine);
                if (result == CombatResult.Win)
                {
                    wins++;
                    turnSum += engine.State.Turn;
                    hpSum += engine.State.Player.Hp;
                }
            }
            var m = new Matchup { WinRatePct = wins * 100 / (trials > 0 ? trials : 1) };
            if (wins > 0)
            {
                m.AvgTurns = turnSum / wins;
                m.AvgHpLeftOnWin = hpSum / wins;
            }
            return m;
        }
    }
}

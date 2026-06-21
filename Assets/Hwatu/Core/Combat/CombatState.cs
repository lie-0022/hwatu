using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Rng;

namespace Hwatu.Core.Combat
{
    /// <summary>전투 전역 상태 컨테이너(SPEC §4.4). 엔진이 phase/turn/result를 갱신한다.</summary>
    public sealed class CombatState
    {
        public CombatState(PlayerState player, List<EnemyState> enemies, IRandom shuffleRng)
        {
            Player = player;
            Enemies = enemies;
            ShuffleRng = shuffleRng;
        }

        public CombatPhase Phase { get; internal set; } = CombatPhase.CombatStart;
        public int Turn { get; internal set; }
        public CombatResult Result { get; internal set; } = CombatResult.InProgress;

        public PlayerState Player { get; }
        public List<EnemyState> Enemies { get; }

        public List<CardInstance> Hand { get; } = new List<CardInstance>();
        public List<CardInstance> DrawPile { get; } = new List<CardInstance>();
        public List<CardInstance> DiscardPile { get; } = new List<CardInstance>();
        public List<CardInstance> ExhaustPile { get; } = new List<CardInstance>();

        public IRandom ShuffleRng { get; }

        /// <summary>전투 이벤트 로그(턴/카드/적 행동/결과). UI·디버그용.</summary>
        public List<string> Log { get; } = new List<string>();

        public bool AllEnemiesDead()
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (!Enemies[i].IsDead)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

using Hwatu.Core.Cards;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;

namespace Hwatu.Core.Combat
{
    /// <summary>
    /// 전투 턴 상태머신(SPEC §3.2). <see cref="Advance"/>가 자동 phase를 한 단계씩 소화하고,
    /// PlayerAction/Win/Lose에서는 멈춰 외부 입력(<see cref="PlayCard"/>/<see cref="EndTurn"/>)을 받는다.
    /// 헤드리스 러너/테스트가 한 스텝씩 구동할 수 있다.
    /// </summary>
    public sealed class CombatEngine
    {
        private readonly EffectDispatcher _dispatcher;

        public CombatEngine(CombatState state, EffectDispatcher dispatcher)
        {
            State = state;
            _dispatcher = dispatcher;
        }

        public CombatState State { get; }
        public CombatResult Result => State.Result;

        /// <summary>자동 phase를 한 단계 진행하고 진행 후의 phase를 반환한다. PlayerAction/Win/Lose에서는 멈춘다.</summary>
        public CombatPhase Advance()
        {
            switch (State.Phase)
            {
                case CombatPhase.CombatStart:
                    StartCombat();
                    State.Phase = CombatPhase.PlayerTurnStart;
                    break;
                case CombatPhase.PlayerTurnStart:
                    StartPlayerTurn();
                    State.Phase = CombatPhase.PlayerAction;
                    break;
                case CombatPhase.PlayerAction:
                    break; // 입력 대기 — 멈춤
                case CombatPhase.PlayerTurnEnd:
                    EndPlayerTurn();
                    State.Phase = CombatPhase.EnemyTurn;
                    break;
                case CombatPhase.EnemyTurn:
                    EnemyTurn();
                    State.Phase = CombatPhase.CheckDeath;
                    break;
                case CombatPhase.CheckDeath:
                    CheckDeath();
                    break;
                case CombatPhase.Win:
                case CombatPhase.Lose:
                    break; // 종료 — 멈춤
            }
            return State.Phase;
        }

        /// <summary>PlayerAction에서만 유효. 코스트/타깃 검증 → 효과 순차 실행 → 카드 이동. 성공 시 true.</summary>
        public bool PlayCard(int handIndex, int enemyTargetIndex = 0)
        {
            if (State.Phase != CombatPhase.PlayerAction)
            {
                return false;
            }
            if (handIndex < 0 || handIndex >= State.Hand.Count)
            {
                return false;
            }

            CardInstance card = State.Hand[handIndex];
            if (State.Player.Energy < card.Data.Cost)
            {
                return false;
            }

            ICombatant target = ResolveCardTarget(card, enemyTargetIndex);
            if (target == null && CardNeedsEnemy(card))
            {
                return false;
            }

            State.Player.Energy -= card.Data.Cost;

            var ctx = new CombatEffectContext(State, State.Player, target);
            var effects = card.Data.Effects;
            for (int i = 0; i < effects.Count; i++)
            {
                _dispatcher.Execute(effects[i], ctx);
            }

            State.Hand.RemoveAt(handIndex);
            if (card.Data.Exhaust)
            {
                State.ExhaustPile.Add(card);
            }
            else
            {
                State.DiscardPile.Add(card);
            }

            // 플레이어 턴 중 적을 전멸시키면 즉시 승리(STS 규칙).
            if (State.AllEnemiesDead())
            {
                State.Result = CombatResult.Win;
                State.Phase = CombatPhase.Win;
            }

            return true;
        }

        /// <summary>PlayerAction에서만 유효. 턴을 종료한다(PlayerTurnEnd로 전이).</summary>
        public bool EndTurn()
        {
            if (State.Phase != CombatPhase.PlayerAction)
            {
                return false;
            }
            State.Phase = CombatPhase.PlayerTurnEnd;
            return true;
        }

        // --- phase 동작 ---

        private void StartCombat()
        {
            State.ShuffleRng.Shuffle(State.DrawPile);
            for (int i = 0; i < State.Enemies.Count; i++)
            {
                State.Enemies[i].RefreshIntent();
            }
        }

        private void StartPlayerTurn()
        {
            State.Turn++;
            State.Player.SetBlock(0);
            State.Player.Energy = State.Player.BaseEnergy;
            PileSystem.Draw(State.Hand, State.DrawPile, State.DiscardPile, State.ShuffleRng, State.Player.HandSize);
        }

        private void EndPlayerTurn()
        {
            State.DiscardPile.AddRange(State.Hand);
            State.Hand.Clear();
        }

        private void EnemyTurn()
        {
            for (int i = 0; i < State.Enemies.Count; i++)
            {
                EnemyState enemy = State.Enemies[i];
                if (enemy.IsDead)
                {
                    continue;
                }

                enemy.SetBlock(0);
                EnemyMoveData move = enemy.CurrentIntent ?? enemy.Ai.PeekNext();
                var ctx = new CombatEffectContext(State, enemy, State.Player);
                var effects = move.Effects;
                for (int j = 0; j < effects.Count; j++)
                {
                    _dispatcher.Execute(effects[j], ctx);
                }

                enemy.Ai.Advance();
                enemy.RefreshIntent();
            }
        }

        private void CheckDeath()
        {
            if (State.AllEnemiesDead())
            {
                State.Result = CombatResult.Win;
                State.Phase = CombatPhase.Win;
            }
            else if (State.Player.Hp <= 0)
            {
                State.Result = CombatResult.Lose;
                State.Phase = CombatPhase.Lose;
            }
            else
            {
                State.Phase = CombatPhase.PlayerTurnStart;
            }
        }

        // --- 헬퍼 ---

        private ICombatant ResolveCardTarget(CardInstance card, int enemyTargetIndex)
        {
            switch (card.Data.Target)
            {
                case TargetType.Self:
                case TargetType.None:
                    return State.Player; // 디스패처가 Self를 Source로 리졸브
                case TargetType.Enemy:
                    if (enemyTargetIndex < 0 || enemyTargetIndex >= State.Enemies.Count)
                    {
                        return null;
                    }
                    return State.Enemies[enemyTargetIndex];
                case TargetType.AllEnemies:
                    return State.Enemies.Count > 0 ? State.Enemies[0] : null; // 이번 스코프 단일 적
                default:
                    return null;
            }
        }

        private static bool CardNeedsEnemy(CardInstance card)
            => card.Data.Target == TargetType.Enemy || card.Data.Target == TargetType.AllEnemies;
    }
}

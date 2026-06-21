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
                    if (State.Phase == CombatPhase.PlayerTurnStart)   // 독사로 Lose가 안 됐으면 입력 대기로
                    {
                        State.Phase = CombatPhase.PlayerAction;
                    }
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

        /// <summary>
        /// PlayerAction에서만 유효. 코스트/타깃 검증 → 효과 순차 실행 → 카드 이동. 성공 시 true.
        /// AllEnemies 카드는 살아있는 모든 적에게 효과를 각각 적용한다.
        /// </summary>
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

            // 타깃 유효성 검증
            TargetType targetType = card.Data.Target;
            if (targetType == TargetType.Enemy)
            {
                if (enemyTargetIndex < 0 || enemyTargetIndex >= State.Enemies.Count)
                {
                    return false;
                }
            }
            else if (targetType == TargetType.AllEnemies)
            {
                if (State.Enemies.Count == 0)
                {
                    return false;
                }
            }

            State.Player.Energy -= card.Data.Cost;
            State.Log.Add($"플레이어: {card.Data.Name}");

            var effects = card.Data.Effects;
            if (targetType == TargetType.AllEnemies)
            {
                for (int ei = 0; ei < State.Enemies.Count; ei++)
                {
                    EnemyState enemy = State.Enemies[ei];
                    if (enemy.IsDead)
                    {
                        continue;
                    }
                    var ctxAll = new CombatEffectContext(State, State.Player, enemy);
                    for (int i = 0; i < effects.Count; i++)
                    {
                        _dispatcher.Execute(effects[i], ctxAll);
                    }
                }
            }
            else
            {
                // Self/None은 시전자(디스패처가 Self를 Source로 리졸브), Enemy는 지정된 적.
                ICombatant target = targetType == TargetType.Enemy
                    ? State.Enemies[enemyTargetIndex]
                    : State.Player;
                var ctx = new CombatEffectContext(State, State.Player, target);
                for (int i = 0; i < effects.Count; i++)
                {
                    _dispatcher.Execute(effects[i], ctx);
                }
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
            MoveInnateToTop();
            for (int i = 0; i < State.Enemies.Count; i++)
            {
                State.Enemies[i].RefreshIntent();
            }
        }

        // Innate 카드를 더미 맨 위(리스트 끝)로 올려 첫 손패에 들어오게 한다.
        private void MoveInnateToTop()
        {
            for (int i = State.DrawPile.Count - 1; i >= 0; i--)
            {
                if (State.DrawPile[i].Data.Innate)
                {
                    CardInstance c = State.DrawPile[i];
                    State.DrawPile.RemoveAt(i);
                    State.DrawPile.Add(c);
                }
            }
        }

        private void StartPlayerTurn()
        {
            State.Turn++;
            State.Log.Add($"── {State.Turn}턴 ──");
            State.Player.SetBlock(0);
            TickPoison(State.Player);
            if (State.Player.Hp <= 0)
            {
                State.Result = CombatResult.Lose;
                State.Phase = CombatPhase.Lose;
                return;
            }
            State.Player.Energy = State.Player.BaseEnergy;
            PileSystem.Draw(State.Hand, State.DrawPile, State.DiscardPile, State.ShuffleRng, State.Player.HandSize);
        }

        private void EndPlayerTurn()
        {
            // Retain 카드는 손패에 남기고, 나머지만 버린다.
            for (int i = State.Hand.Count - 1; i >= 0; i--)
            {
                if (!State.Hand[i].Data.Retain)
                {
                    State.DiscardPile.Add(State.Hand[i]);
                    State.Hand.RemoveAt(i);
                }
            }
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
                TickPoison(enemy);
                if (enemy.IsDead)
                {
                    continue;   // 독으로 쓰러지면 행동하지 않음
                }

                // 실행은 항상 현재 AI 상태(PeekNext)를 직접 사용한다.
                // CurrentIntent는 UI 표시 전용 캐시이므로 실행 소스로 겸용하지 않는다(의도 변경 효과 대비).
                EnemyMoveData move = enemy.Ai.PeekNext(enemy);
                State.Log.Add($"{enemy.Data.Name}: {move.Intent} {move.Value}");
                var ctx = new CombatEffectContext(State, enemy, State.Player);
                var effects = move.Effects;
                for (int j = 0; j < effects.Count; j++)
                {
                    _dispatcher.Execute(effects[j], ctx);
                }

                enemy.Ai.Advance();
                enemy.RefreshIntent(); // 다음 턴에 보여줄 의도 갱신
            }
        }

        // Poison(중독): 턴 시작 시 스택만큼 피해(Block 무시), 그 후 1 감소.
        private static void TickPoison(ICombatant c)
        {
            int p = c.GetStatus(StatusType.Poison);
            if (p > 0)
            {
                c.SetHp(System.Math.Max(0, c.Hp - p));
                c.AddStatus(StatusType.Poison, -1);
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
    }
}

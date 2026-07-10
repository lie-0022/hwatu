using System.Collections.Generic;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;

namespace Hwatu.Core.Jokbo
{
    /// <summary>
    /// 족보 전투 상태머신(설계 §3~4) — 기존 CombatEngine의 병행 구현.
    /// 적 턴·상태이상·유물 훅·승패는 기존 CombatState/EffectDispatcher를 재사용하고,
    /// 손패(유지형 8장)·내기/버리기 횟수·족보 제출만 이 엔진이 소유한다. 검증 완료 시 기존 엔진을 대체한다.
    /// </summary>
    public sealed class JokboCombatEngine
    {
        private readonly EffectDispatcher _dispatcher;
        private int _nextInstanceId;

        public JokboCombatEngine(CombatState state, EffectDispatcher dispatcher, IReadOnlyList<HwatuCardData> deck)
        {
            State = state;
            _dispatcher = dispatcher;
            for (int i = 0; i < deck.Count; i++)
            {
                DrawPile.Add(new JokboCardInstance(deck[i], _nextInstanceId++));
            }
        }

        /// <summary>공유 전투 상태(플레이어/적/phase/로그/훅). 구 카드 더미는 비워 둔다.</summary>
        public CombatState State { get; }
        public CombatResult Result => State.Result;

        // ── 족보 전용 상태 ──
        public List<JokboCardInstance> Hand { get; } = new List<JokboCardInstance>();
        public List<JokboCardInstance> DrawPile { get; } = new List<JokboCardInstance>();
        public List<JokboCardInstance> DiscardPile { get; } = new List<JokboCardInstance>();
        /// <summary>이번 턴 남은 내기 횟수.</summary>
        public int PlaysLeft { get; private set; }
        /// <summary>이번 턴 남은 버리기 횟수.</summary>
        public int DiscardsLeft { get; private set; }

        /// <summary>자동 phase를 한 단계 진행(PlayerAction/Win/Lose에서 멈춤) — 기존 엔진과 동일 계약.</summary>
        public CombatPhase Advance()
        {
            switch (State.Phase)
            {
                case CombatPhase.CombatStart:
                    State.ShuffleRng.Shuffle(DrawPile);
                    for (int i = 0; i < State.Enemies.Count; i++)
                    {
                        State.Enemies[i].RefreshIntent(State.Player);
                    }
                    State.Phase = CombatPhase.PlayerTurnStart;
                    break;
                case CombatPhase.PlayerTurnStart:
                    StartPlayerTurn();
                    if (State.Phase == CombatPhase.PlayerTurnStart)
                    {
                        State.Phase = CombatPhase.PlayerAction;
                    }
                    break;
                case CombatPhase.PlayerAction:
                    break;   // 입력 대기
                case CombatPhase.PlayerTurnEnd:
                    DecayDebuffs(State.Player);   // 손패 유지형 — 버리지 않는다(설계 §3)
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
                    break;
            }
            return State.Phase;
        }

        /// <summary>
        /// 손패의 카드들(인덱스)을 족보로 제출한다(PlayerAction·내기 잔여 필요).
        /// 공격합×배수 → 대상(전체 족보는 전원), 방어합×배수 → 자신, 명명 시그니처 적용. 성공 시 true.
        /// </summary>
        public bool PlayJokbo(IReadOnlyList<int> handIndices, int enemyTargetIndex = 0)
        {
            if (State.Phase != CombatPhase.PlayerAction || PlaysLeft <= 0)
            {
                return false;
            }
            if (handIndices == null || handIndices.Count == 0)
            {
                return false;
            }
            var seen = new HashSet<int>();
            var cards = new List<HwatuCardData>(handIndices.Count);
            for (int i = 0; i < handIndices.Count; i++)
            {
                int idx = handIndices[i];
                if (idx < 0 || idx >= Hand.Count || !seen.Add(idx))
                {
                    return false;   // 범위 밖/중복 인덱스
                }
                cards.Add(Hand[idx].Data);
            }

            JokboType jokbo = JokboDetector.Detect(cards);
            if (jokbo == JokboType.None)
            {
                return false;
            }

            bool aoe = JokboRules.IsAoe(jokbo);
            if (!aoe && (enemyTargetIndex < 0 || enemyTargetIndex >= State.Enemies.Count))
            {
                return false;
            }

            int num = JokboRules.MultNum(jokbo);
            int den = JokboRules.MultDen(jokbo);
            int attack = JokboRules.AttackSum(cards) * num / den;
            int defense = JokboRules.DefenseSum(cards) * num / den;

            State.Log.Add($"플레이어: {JokboRules.DisplayName(jokbo)} ({cards.Count}장)");
            for (int hi = 0; hi < State.OnCardPlayHooks.Count; hi++) { State.OnCardPlayHooks[hi](State.Player); }   // 족보 제출 = 카드 플레이 훅(유물)

            // 방어 먼저(민첩 가산은 디스패처가 처리)
            if (defense > 0)
            {
                var selfCtx = new CombatEffectContext(State, State.Player, State.Player);
                _dispatcher.Execute(new EffectData(EffectOp.GainBlock, defense, Cards.TargetType.Self), selfCtx);
            }

            // 공격(위엄/광 가산·약화/취약 보정은 DamageMath 경유)
            if (attack > 0)
            {
                if (aoe)
                {
                    for (int ei = 0; ei < State.Enemies.Count; ei++)
                    {
                        EnemyState enemy = State.Enemies[ei];
                        if (enemy.IsDead) { continue; }
                        DealHits(enemy, attack, 1);
                    }
                }
                else
                {
                    EnemyState target = State.Enemies[enemyTargetIndex];
                    if (!target.IsDead)
                    {
                        int hits = jokbo == JokboType.Godori ? JokboRules.GodoriHits : 1;
                        DealHits(target, attack, hits);
                    }
                }
            }

            ApplySignature(jokbo, aoe ? -1 : enemyTargetIndex);

            // 낸 카드 제거(내림차순) → 버린 더미
            var sorted = new List<int>(handIndices);
            sorted.Sort();
            for (int i = sorted.Count - 1; i >= 0; i--)
            {
                JokboCardInstance card = Hand[sorted[i]];
                Hand.RemoveAt(sorted[i]);
                DiscardPile.Add(card);
            }

            if (jokbo == JokboType.Godori)
            {
                Draw(JokboRules.GodoriDraw);   // 고도리: 새 떼의 인도 — 드로우
            }

            PlaysLeft--;

            if (State.AllEnemiesDead())
            {
                State.Result = CombatResult.Win;
                State.Phase = CombatPhase.Win;
            }
            return true;
        }

        /// <summary>선택 카드를 버리고 같은 장수만큼 드로우(PlayerAction·버리기 잔여 필요). 성공 시 true.</summary>
        public bool DiscardCards(IReadOnlyList<int> handIndices)
        {
            if (State.Phase != CombatPhase.PlayerAction || DiscardsLeft <= 0)
            {
                return false;
            }
            if (handIndices == null || handIndices.Count == 0)
            {
                return false;
            }
            var seen = new HashSet<int>();
            for (int i = 0; i < handIndices.Count; i++)
            {
                if (handIndices[i] < 0 || handIndices[i] >= Hand.Count || !seen.Add(handIndices[i]))
                {
                    return false;
                }
            }
            var sorted = new List<int>(handIndices);
            sorted.Sort();
            int count = sorted.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                JokboCardInstance card = Hand[sorted[i]];
                Hand.RemoveAt(sorted[i]);
                DiscardPile.Add(card);
            }
            Draw(count);
            DiscardsLeft--;
            return true;
        }

        /// <summary>턴 종료(PlayerAction에서만).</summary>
        public bool EndTurn()
        {
            if (State.Phase != CombatPhase.PlayerAction)
            {
                return false;
            }
            State.Phase = CombatPhase.PlayerTurnEnd;
            return true;
        }

        // ── phase 내부 ──

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
            TickRegen(State.Player);
            if (Awakening.Tier(State.Player.GetStatus(StatusType.Majesty)) >= 2)
            {
                State.Player.AddStatus(StatusType.Majesty, 1);   // 각성 2단계 유지(캐릭터 레이어 휴면 — 위엄 원천 없으면 무해)
            }
            for (int i = 0; i < State.OnTurnStartHooks.Count; i++) { State.OnTurnStartHooks[i](State.Player); }
            PlaysLeft = JokboRules.PlaysPerTurn;
            DiscardsLeft = JokboRules.DiscardsPerTurn;
            Draw(JokboRules.HandSize - Hand.Count);   // 손패 유지형 리필(설계 §3)
        }

        // 대상 1체에 총 피해를 hits회로 분할해 가한다(나머지는 첫 타에). 다회는 타격당 위엄/광 가산 — 고도리 시너지.
        private void DealHits(EnemyState target, int totalAttack, int hits)
        {
            int baseHit = totalAttack / hits;
            int remainder = totalAttack % hits;
            for (int h = 0; h < hits; h++)
            {
                if (target.IsDead) { return; }
                int amount = h == 0 ? baseHit + remainder : baseHit;
                var ctx = new CombatEffectContext(State, State.Player, target);
                _dispatcher.Execute(new EffectData(EffectOp.DealDamage, amount, Cards.TargetType.Enemy), ctx);
            }
        }

        // 명명 족보 시그니처(설계 §4.2): 홍단 화상 / 청단 약화 / 초단 취약 / 오광 전체 약화.
        private void ApplySignature(JokboType jokbo, int targetIndex)
        {
            switch (jokbo)
            {
                case JokboType.RedRibbons:
                    ApplyStatusTo(targetIndex, StatusType.Poison, JokboRules.RedRibbonsBurn);
                    break;
                case JokboType.BlueRibbons:
                    ApplyStatusTo(targetIndex, StatusType.Weak, JokboRules.BlueRibbonsWeak);
                    break;
                case JokboType.PlainRibbons:
                    ApplyStatusTo(targetIndex, StatusType.Vulnerable, JokboRules.PlainRibbonsVulnerable);
                    break;
                case JokboType.FiveBrights:
                    for (int i = 0; i < State.Enemies.Count; i++)
                    {
                        if (!State.Enemies[i].IsDead)
                        {
                            State.Enemies[i].AddStatus(StatusType.Weak, JokboRules.FiveBrightsWeakAll);
                        }
                    }
                    break;
            }
        }

        private void ApplyStatusTo(int targetIndex, StatusType status, int amount)
        {
            if (targetIndex < 0 || targetIndex >= State.Enemies.Count) { return; }
            EnemyState target = State.Enemies[targetIndex];
            if (!target.IsDead)
            {
                target.AddStatus(status, amount);
            }
        }

        // 드로우(부족하면 버린 더미 셔플 재생성 — PileSystem과 동일 규약: 리스트 끝 = top).
        private void Draw(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (DrawPile.Count == 0)
                {
                    if (DiscardPile.Count == 0) { return; }
                    DrawPile.AddRange(DiscardPile);
                    DiscardPile.Clear();
                    State.ShuffleRng.Shuffle(DrawPile);
                }
                JokboCardInstance top = DrawPile[DrawPile.Count - 1];
                DrawPile.RemoveAt(DrawPile.Count - 1);
                Hand.Add(top);
            }
        }

        // 적 턴(기존 CombatEngine.EnemyTurn과 동일 로직 — 병행 기간 중복, 전환 완료 시 구 엔진 제거).
        private void EnemyTurn()
        {
            for (int i = 0; i < State.Enemies.Count; i++)
            {
                EnemyState enemy = State.Enemies[i];
                if (enemy.IsDead) { continue; }

                enemy.SetBlock(0);
                TickPoison(enemy);
                if (enemy.IsDead) { continue; }

                EnemyMoveData move = enemy.Ai.PeekNext(enemy, State.Player);

                if (move.Intent == IntentType.Doom && move.DoomTurns > 0)
                {
                    if (enemy.DoomTimer < 0)
                    {
                        enemy.SetDoomTimer(move.DoomTurns);
                    }
                    if (enemy.DoomTimer > 0)
                    {
                        enemy.SetDoomTimer(enemy.DoomTimer - 1);
                        State.Log.Add($"{enemy.Data.Name}: 파멸 예고({enemy.DoomTimer + 1})");
                        enemy.RefreshIntent(State.Player);
                        continue;
                    }
                    enemy.SetDoomTimer(-1);
                }

                State.Log.Add($"{enemy.Data.Name}: {move.Intent} {move.Value}");
                var ctx = new CombatEffectContext(State, enemy, State.Player);
                var effects = move.Effects;
                for (int j = 0; j < effects.Count; j++)
                {
                    _dispatcher.Execute(effects[j], ctx);
                }

                enemy.Ai.Advance();
                enemy.RefreshIntent(State.Player);
                DecayDebuffs(enemy);
            }
        }

        private static void TickPoison(ICombatant c)
        {
            int p = c.GetStatus(StatusType.Poison);
            if (p > 0)
            {
                c.SetHp(System.Math.Max(0, c.Hp - p));
                c.AddStatus(StatusType.Poison, -1);
            }
        }

        private static void TickRegen(PlayerState p)
        {
            int r = p.GetStatus(StatusType.Regen);
            if (r > 0)
            {
                p.SetHp(System.Math.Min(p.MaxHp, p.Hp + r));
                p.AddStatus(StatusType.Regen, -1);
            }
        }

        private static void DecayDebuffs(ICombatant c)
        {
            if (c.GetStatus(StatusType.Weak) > 0) { c.AddStatus(StatusType.Weak, -1); }
            if (c.GetStatus(StatusType.Vulnerable) > 0) { c.AddStatus(StatusType.Vulnerable, -1); }
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

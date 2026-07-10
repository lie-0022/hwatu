using System;
using System.Collections.Generic;
using UnityEngine;
using Hwatu.Core.Combat;
using Hwatu.Core.Enemies;
using Hwatu.Core.Jokbo;
using Hwatu.Core.Run;

namespace Hwatu.Game
{
    /// <summary>
    /// 족보 전투 엔진 구동(UI 비의존) — 기존 CombatController의 족보판 병행 구현.
    /// <see cref="JokboCombatView"/>가 이 상태를 읽어 그리고, 내기/버리기/턴종료 입력을 전달한다.
    /// </summary>
    public sealed class JokboCombatController : MonoBehaviour
    {
        [SerializeField] private ulong _seed = 12345UL;
        [SerializeField] private bool _autoStart = false;

        private JokboCombatEngine _engine;
        private bool _ended;

        public JokboCombatEngine Engine => _engine;
        public CombatState State => _engine?.State;
        public CombatResult Result => _engine != null ? _engine.Result : CombatResult.InProgress;

        /// <summary>전투 종료 시 1회 발생(true=승리, false=패배).</summary>
        public event Action<bool> OnCombatEnded;

        private void Awake()
        {
            if (_autoStart)
            {
                StartCombat(HwatuDeckContent.CommonStarterDeck(), Hwatu.Core.Content.StarterContent.DokkaebiMinion(),
                    _seed, 80, 80);
            }
        }

        /// <summary>단일 적 전투 시작(공통 시작 덱 데모 등).</summary>
        public void StartCombat(IReadOnlyList<HwatuCardData> deck, EnemyData enemy, ulong seed, int maxHp, int hp, IReadOnlyList<RelicData> relics = null)
        {
            BeginWith(JokboCombatFactory.Create(deck, enemy, seed, maxHp, hp, relics));
        }

        /// <summary>다중 적 전투 시작.</summary>
        public void StartCombat(IReadOnlyList<HwatuCardData> deck, IReadOnlyList<EnemyData> enemies, ulong seed, int maxHp, int hp, IReadOnlyList<RelicData> relics = null)
        {
            BeginWith(JokboCombatFactory.Create(deck, enemies, seed, maxHp, hp, relics));
        }

        private void BeginWith(JokboCombatEngine engine)
        {
            _engine = engine;
            _ended = false;
            AdvanceToInput();
            CheckEnd();
        }

        /// <summary>손패 인덱스들을 족보로 제출(대상 적 지정). 성공 시 true. 내기 잔여를 소모하며 턴은 유지.</summary>
        public bool PlayJokbo(IReadOnlyList<int> handIndices, int enemyTargetIndex = 0)
        {
            if (_engine == null) { return false; }
            bool ok = _engine.PlayJokbo(handIndices, enemyTargetIndex);
            if (ok) { CheckEnd(); }   // 적 전멸 시 즉시 승리 반영
            return ok;
        }

        /// <summary>손패 인덱스들을 버리고 같은 장수만큼 드로우. 성공 시 true.</summary>
        public bool Discard(IReadOnlyList<int> handIndices)
        {
            return _engine != null && _engine.DiscardCards(handIndices);
        }

        /// <summary>턴 종료 → 적 턴 처리 후 다음 PlayerAction까지 진행.</summary>
        public void EndTurn()
        {
            if (_engine == null) { return; }
            _engine.EndTurn();
            AdvanceToInput();
            CheckEnd();
        }

        private void AdvanceToInput()
        {
            int guard = 0;
            while (_engine.Result == CombatResult.InProgress
                   && _engine.State.Phase != CombatPhase.PlayerAction
                   && guard++ < 200)
            {
                _engine.Advance();
            }
        }

        private void CheckEnd()
        {
            if (_ended) { return; }
            if (_engine.Result == CombatResult.Win)
            {
                _ended = true;
                OnCombatEnded?.Invoke(true);
            }
            else if (_engine.Result == CombatResult.Lose)
            {
                _ended = true;
                OnCombatEnded?.Invoke(false);
            }
        }
    }
}

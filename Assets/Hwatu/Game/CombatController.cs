using System;
using System.Collections.Generic;
using UnityEngine;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;
using Hwatu.Core.Run;

namespace Hwatu.Game
{
    /// <summary>
    /// 전투 엔진 구동(UI 비의존). <see cref="CombatView"/>가 이 상태를 읽어 그리고, 입력을 전달한다.
    /// 단독 데모(_autoStart=true)거나, GameFlow가 <see cref="StartCombat"/>로 런 덱·HP를 주입한다.
    /// </summary>
    public sealed class CombatController : MonoBehaviour
    {
        [SerializeField] private ulong _seed = 12345UL;
        [SerializeField] private bool _autoStart = true;

        private CombatEngine _engine;
        private bool _ended;

        public CombatState State => _engine?.State;
        public CombatResult Result => _engine != null ? _engine.Result : CombatResult.InProgress;

        /// <summary>전투 종료 시 1회 발생(true=승리, false=패배).</summary>
        public event Action<bool> OnCombatEnded;

        private void Awake()
        {
            if (_autoStart)
            {
                NewCombat();
            }
        }

        /// <summary>단독 데모 전투(광객 vs 잡도깨비, 고정 시드).</summary>
        public void NewCombat()
        {
            BeginWith(CombatFactory.CreateLuminaryVsDokkaebi(_seed));
        }

        /// <summary>외부(GameFlow)에서 런 덱·적·플레이어 HP를 주입해 전투 시작.</summary>
        public void StartCombat(IReadOnlyList<CardData> deck, EnemyData enemyData, ulong seed, int maxHp, int hp, IReadOnlyList<RelicData> relics = null)
        {
            BeginWith(CombatFactory.CreateCombat(deck, enemyData, seed, maxHp, hp, relics));
        }

        private void BeginWith(CombatState state)
        {
            _engine = new CombatEngine(state, new EffectDispatcher());
            _ended = false;
            AdvanceToInput();
            CheckEnd();
        }

        /// <summary>손패의 카드를 사용(enemyTargetIndex로 적 지정, 기본 0번 적). 성공 시 true.</summary>
        public bool PlayCard(int handIndex, int enemyTargetIndex = 0)
        {
            if (_engine == null)
            {
                return false;
            }
            bool ok = _engine.PlayCard(handIndex, enemyTargetIndex);
            if (ok)
            {
                AdvanceToInput();
                CheckEnd();
            }
            return ok;
        }

        public void EndTurn()
        {
            if (_engine == null)
            {
                return;
            }
            _engine.EndTurn();
            AdvanceToInput();
            CheckEnd();
        }

        /// <summary>전투 중 포션 사용(즉시 효과). 성공 시 true.</summary>
        public bool UsePotion(Hwatu.Core.Run.PotionData potion)
        {
            return _engine != null && _engine.UsePotion(potion);
        }

        /// <summary>자동 phase를 PlayerAction(또는 전투 종료)까지 진행.</summary>
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

        // 승/패가 확정되면 OnCombatEnded를 1회 발생.
        private void CheckEnd()
        {
            if (_ended)
            {
                return;
            }
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

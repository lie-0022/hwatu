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

        /// <summary>다중 몬스터 전투 시작(STS식 1~다수).</summary>
        public void StartCombat(IReadOnlyList<CardData> deck, IReadOnlyList<EnemyData> enemies, ulong seed, int maxHp, int hp, IReadOnlyList<RelicData> relics = null)
        {
            BeginWith(CombatFactory.CreateCombat(deck, enemies, seed, maxHp, hp, relics));
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

        /// <summary>전투 중 포션 사용(즉시 효과). 성공 시 true.
        /// <param name="potion">사용할 포션.</param>
        /// <param name="enemyTargetIndex">Enemy 포션 대상 적 인덱스(기본 0). 범위 밖이거나 죽었으면 첫 생존 적으로 폴백.</param>
        /// </summary>
        public bool UsePotion(Hwatu.Core.Run.PotionData potion, int enemyTargetIndex = 0)
        {
            return _engine != null && _engine.UsePotion(potion, enemyTargetIndex);
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

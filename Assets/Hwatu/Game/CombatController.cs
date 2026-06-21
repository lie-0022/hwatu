using UnityEngine;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;

namespace Hwatu.Game
{
    /// <summary>
    /// 전투 엔진 구동(UI 비의존). <see cref="CombatView"/>가 이 상태를 읽어 그리고, 입력을 여기로 전달한다.
    /// </summary>
    public sealed class CombatController : MonoBehaviour
    {
        [SerializeField] private ulong _seed = 12345UL;

        private CombatEngine _engine;

        public CombatState State => _engine?.State;
        public CombatResult Result => _engine != null ? _engine.Result : CombatResult.InProgress;

        private void Awake()
        {
            NewCombat();
        }

        public void NewCombat()
        {
            CombatState state = CombatFactory.CreateLuminaryVsDokkaebi(_seed);
            _engine = new CombatEngine(state, new EffectDispatcher());
            AdvanceToInput();
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
    }
}

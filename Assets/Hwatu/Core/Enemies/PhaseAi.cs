using System.Collections.Generic;
using Hwatu.Core.Combat;

namespace Hwatu.Core.Enemies
{
    /// <summary>
    /// 페이즈 AI(보스용). HP가 임계% 미만으로 떨어지면 phase2 AiOrder로 '광폭화'한다(되돌아가지 않음).
    /// 전환 시 인덱스를 리셋해 phase2의 첫 수부터 낸다. PeekNext는 같은 상태면 같은 값(멱등).
    /// </summary>
    public sealed class PhaseAi : IEnemyAi
    {
        private readonly EnemyData _data;
        private readonly IReadOnlyList<string> _phase1;
        private readonly IReadOnlyList<string> _phase2;
        private readonly int _thresholdPercent;
        private int _index;
        private bool _enraged;

        public PhaseAi(EnemyData data, IReadOnlyList<string> phase1, IReadOnlyList<string> phase2, int thresholdPercent = 50)
        {
            _data = data;
            _phase1 = phase1;
            _phase2 = phase2;
            _thresholdPercent = thresholdPercent;
        }

        /// <summary>광폭화(2페이즈) 진입 여부.</summary>
        public bool IsEnraged => _enraged;

        public EnemyMoveData PeekNext(EnemyState self, PlayerState player = null)
        {
            if (!_enraged && self != null && self.Hp * 100 <= self.MaxHp * _thresholdPercent)
            {
                _enraged = true;
                _index = 0;   // phase2 첫 수부터
            }
            IReadOnlyList<string> order = _enraged ? _phase2 : _phase1;
            return _data.FindMove(order[_index % order.Count]);
        }

        public void Advance()
        {
            _index++;
        }
    }
}

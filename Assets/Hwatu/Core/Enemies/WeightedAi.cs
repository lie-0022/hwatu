using System.Collections.Generic;
using Hwatu.Core.Combat;
using Hwatu.Core.Rng;

namespace Hwatu.Core.Enemies
{
    /// <summary>
    /// AiOrder를 가중치 풀(중복 ID = 높은 확률)로 보고 시드 RNG로 다음 move를 고른다.
    /// 같은 move를 연속 repeatLimit회 초과하지 않는다(STS식 연속 제한 — 연구 문서 §4.2).
    /// PeekNext는 고른 move를 캐시해 완전정보(미리보기=실제 실행)를 유지한다.
    /// </summary>
    public sealed class WeightedAi : IEnemyAi
    {
        private readonly EnemyData _data;
        private readonly IRandom _rng;
        private readonly int _repeatLimit;
        private string _cached;     // PeekNext가 정한 다음 실행 move(완전정보 유지)
        private string _lastMove;   // 직전 실행 move
        private int _repeatCount;   // 직전 move 연속 횟수

        public WeightedAi(EnemyData data, IRandom rng, int repeatLimit = 2)
        {
            _data = data;
            _rng = rng;
            _repeatLimit = repeatLimit;
        }

        public EnemyMoveData PeekNext(EnemyState self, PlayerState player = null)
        {
            if (_cached == null)
            {
                _cached = Choose();
            }
            return _data.FindMove(_cached);
        }

        // 가중치 풀에서 선택하되, 직전 move가 연속 제한에 도달했으면 그 move는 후보 제외.
        private string Choose()
        {
            var pool = new List<string>(_data.AiOrder.Count);
            for (int i = 0; i < _data.AiOrder.Count; i++)
            {
                string id = _data.AiOrder[i];
                if (id == _lastMove && _repeatCount >= _repeatLimit) { continue; }
                pool.Add(id);
            }
            if (pool.Count == 0)   // 전부 제외되면(단일 move 적 등) 제한 무시하고 전체
            {
                for (int i = 0; i < _data.AiOrder.Count; i++) { pool.Add(_data.AiOrder[i]); }
            }
            return pool[_rng.NextInt(pool.Count)];
        }

        public void Advance()
        {
            if (_cached == _lastMove)
            {
                _repeatCount++;
            }
            else
            {
                _lastMove = _cached;
                _repeatCount = 1;
            }
            _cached = null;
        }
    }
}

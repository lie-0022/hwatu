using Hwatu.Core.Combat;

namespace Hwatu.Core.Enemies
{
    /// <summary>
    /// 플레이어 상태에 반응하는 조건부 AI(연구 §4.3). 플레이어 Block이 임계 이상이면 디버프(방어 무력화),
    /// 아니면 공격적 move를 낸다. AiOrder를 [디버프, 공격, (보조)] 우선순위 풀로 본다.
    /// 완전정보(미리보기=실제 실행)를 위해 PeekNext가 고른 move를 캐시한다.
    /// </summary>
    public sealed class ConditionalAi : IEnemyAi
    {
        private readonly EnemyData _data;
        private readonly int _blockThreshold;
        private string _cached;
        private int _step;   // Block 낮을 때 order[1..] 순환용(매턴 같은 강타 반복 방지)

        public ConditionalAi(EnemyData data, int blockThreshold = 8)
        {
            _data = data;
            _blockThreshold = blockThreshold;
        }

        public EnemyMoveData PeekNext(EnemyState self, PlayerState player = null)
        {
            if (_cached == null)
            {
                _cached = Choose(player);
            }
            return _data.FindMove(_cached);
        }

        // 플레이어 Block ≥ 임계면 order[0](디버프)로 방어를 무력화, 아니면 order[1](공격). player가 없으면 공격.
        private string Choose(PlayerState player)
        {
            var order = _data.AiOrder;
            if (order.Count == 0) { return null; }
            if (player != null && player.Block >= _blockThreshold)
            {
                return order[0];   // 디버프(약화/취약)로 방어 무력화
            }
            // Block 낮음: order[1..]를 순환(공격↔버프 — 매턴 같은 강타만 반복하지 않게)
            if (order.Count <= 1) { return order[0]; }
            return order[1 + (_step % (order.Count - 1))];
        }

        public void Advance()
        {
            _step++;
            _cached = null;
        }
    }
}

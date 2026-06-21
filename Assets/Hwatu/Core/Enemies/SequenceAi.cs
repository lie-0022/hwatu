namespace Hwatu.Core.Enemies
{
    /// <summary>AiOrder를 순서대로 순환하며 move를 낸다(SPEC §4.2의 sequence AI).</summary>
    public sealed class SequenceAi : IEnemyAi
    {
        private readonly EnemyData _data;
        private int _index;

        public SequenceAi(EnemyData data)
        {
            _data = data;
        }

        public EnemyMoveData PeekNext()
        {
            string moveId = _data.AiOrder[_index % _data.AiOrder.Count];
            return _data.FindMove(moveId);
        }

        public void Advance()
        {
            _index++;
        }
    }
}

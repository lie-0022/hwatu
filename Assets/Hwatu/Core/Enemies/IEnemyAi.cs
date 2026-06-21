namespace Hwatu.Core.Enemies
{
    /// <summary>
    /// 적이 다음 행동을 고르는 AI. 완전정보 원칙(SPEC §2.2)을 위해 실행 전 <see cref="PeekNext"/>로
    /// 다음 move를 노출하고, 실제 실행 후 <see cref="Advance"/>로 진행한다.
    /// </summary>
    public interface IEnemyAi
    {
        /// <summary>다음에 실행할 move(아직 소비하지 않음). 여러 번 호출해도 같은 값을 반환한다.</summary>
        EnemyMoveData PeekNext();

        /// <summary>현재 move를 소비하고 다음으로 진행한다.</summary>
        void Advance();
    }
}

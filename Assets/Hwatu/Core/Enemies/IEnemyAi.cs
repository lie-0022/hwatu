using Hwatu.Core.Combat;

namespace Hwatu.Core.Enemies
{
    /// <summary>
    /// 적이 다음 행동을 고르는 AI. 완전정보 원칙(SPEC §2.2)을 위해 실행 전 <see cref="PeekNext"/>로
    /// 다음 move를 노출하고, 실제 실행 후 <see cref="Advance"/>로 진행한다.
    /// </summary>
    public interface IEnemyAi
    {
        /// <summary>다음에 실행할 move(self의 HP 등으로 페이즈 분기 가능). 같은 상태면 같은 값.</summary>
        EnemyMoveData PeekNext(EnemyState self);

        /// <summary>현재 move를 소비하고 다음으로 진행한다.</summary>
        void Advance();
    }
}

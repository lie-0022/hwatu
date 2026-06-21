namespace Hwatu.Core.Combat
{
    /// <summary>
    /// 전투 참가자(플레이어/적) 공용 인터페이스. 효과가 HP·Block·상태를 읽고 쓴다.
    /// 이 추상 덕분에 deal_damage 등 효과를 플레이어→적, 적→플레이어 한 코드로 처리한다.
    /// </summary>
    public interface ICombatant
    {
        int Hp { get; }
        int Block { get; }
        void SetHp(int value);
        void SetBlock(int value);
        int GetStatus(StatusType status);
        void AddStatus(StatusType status, int amount);
    }
}

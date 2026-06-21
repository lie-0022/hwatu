namespace Hwatu.Core.Enemies
{
    /// <summary>적 행동 의도. 완전정보 원칙(SPEC §2.2)에 따라 실행 전 항상 공개된다.</summary>
    public enum IntentType
    {
        Attack,
        AttackMulti,
        Block,
        Buff,
        Debuff,
        Summon,
        Doom,
        Unknown
    }
}

namespace Hwatu.Core.Combat
{
    /// <summary>
    /// 전투 중 부여되는 상태/스택. radiance(광)는 자원이자 모든 Attack 피해 가산의 단일 출처다(SPEC §6.1).
    /// </summary>
    public enum StatusType
    {
        Radiance,   // 광: 누적 스택 → 모든 Attack 피해에 +radiance
        Weak,       // 약화: 공격자가 가하는 피해 ×3/4 (EffectDispatcher.DealDamage에서 적용)
        Vulnerable, // 취약: 대상이 받는 피해 ×3/2 (적용됨)
        Poison,     // 독: 턴 시작 시 스택만큼 피해(Block 무시) + 1 감소 (CombatEngine.TickPoison)
        Dexterity   // 민첩: GainBlock에 +Dexterity (Radiance의 방어판)
    }
}

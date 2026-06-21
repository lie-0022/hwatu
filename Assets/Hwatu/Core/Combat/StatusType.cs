namespace Hwatu.Core.Combat
{
    /// <summary>
    /// 전투 중 부여되는 상태/스택. radiance(광)는 자원이자 모든 Attack 피해 가산의 단일 출처다(SPEC §6.1).
    /// </summary>
    public enum StatusType
    {
        Radiance,   // 광: 누적 스택 → 모든 Attack 피해에 +radiance
        Weak,       // 약화: 가하는 피해 ×3/4 (이번 스코프는 자리만)
        Vulnerable, // 취약: 받는 피해 ×3/2
        Poison      // 독 (이번 스코프는 자리만)
    }
}

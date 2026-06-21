namespace Hwatu.Core.Combat
{
    /// <summary>
    /// 캐릭터 고유 2차 자원. radiance는 누적을 StatusType.Radiance로 통합 라우팅하므로,
    /// 여기서는 gain_resource 데이터 호환을 위한 식별자로만 존재한다(stakes/go/chaff는 후순위 캐릭터용).
    /// </summary>
    public enum ResourceType
    {
        Radiance, // → 실제 누적은 StatusType.Radiance로 라우팅
        Stakes,   // 판돈 (타짜)
        Go,       // 고 (타짜)
        Chaff     // 피 (피바라기)
    }
}

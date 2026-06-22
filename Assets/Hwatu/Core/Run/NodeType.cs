namespace Hwatu.Core.Run
{
    /// <summary>맵 노드(방) 타입. 골격은 STS, 화투 테마는 표시 계층에서 매핑.</summary>
    public enum NodeType
    {
        Combat,    // 일반 전투
        Elite,     // 엘리트
        Rest,      // 휴식처(모닥불)
        Shop,      // 상점
        Treasure,  // 보물(유물)
        Event,     // 이벤트(?)
        Boss       // 보스
    }
}

namespace Hwatu.Core
{
    /// <summary>
    /// Hwatu.Core 어셈블리 마커 겸 참조 검증용 상수. 실제 게임 로직 타입은
    /// Combat / Effects / Cards / Enemies / Rng / Content 폴더에 위치한다.
    /// 이 어셈블리는 UnityEngine을 참조하지 않는다(noEngineReferences) — 헤드리스 테스트 가능.
    /// </summary>
    public static class CoreAssemblyMarker
    {
        public const string Name = "Hwatu.Core";
    }
}

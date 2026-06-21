namespace Hwatu.Core.Enemies
{
    /// <summary>적 AI가 다음 행동을 고르는 방식(SPEC §4.2). 이번 스코프는 Sequence만 구현.</summary>
    public enum EnemyAiKind
    {
        Sequence,
        WeightedRandom,
        Conditional
    }
}

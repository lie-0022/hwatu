namespace Hwatu.Core.Effects
{
    /// <summary>
    /// 효과 원자 op 식별자(SPEC §5.1). 문자열로 둬서 데이터(JSON/ScriptableObject)와 1:1 호환된다.
    /// </summary>
    public static class EffectOp
    {
        public const string DealDamage = "deal_damage";
        public const string GainBlock = "gain_block";
        public const string Draw = "draw";
        public const string ApplyStatus = "apply_status";
        public const string GainResource = "gain_resource";
        public const string ClearStatus = "clear_status";
        public const string ConsumeRadiance = "consume_radiance";   // 광 전부 소비 → (소비한 광 × amount) 데미지 (STS2 Stars식)
    }
}

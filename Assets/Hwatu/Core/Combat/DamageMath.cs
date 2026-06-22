namespace Hwatu.Core.Combat
{
    /// <summary>데미지 보정 계산(광·약화·취약). 실제 적용(EffectDispatcher)과 인텐트 미리보기(EnemyView)가 공유한다.</summary>
    public static class DamageMath
    {
        /// <summary>
        /// Block 차감 전, 보정만 적용한 데미지를 반환한다:
        /// (기본 + 광) → 약화면 ×3/4 → 취약이면 ×3/2 → max(0). float·Mathf 금지(결정론 보장).
        /// </summary>
        public static int RawDamage(ICombatant source, ICombatant target, int baseAmount)
        {
            int dmg = baseAmount + source.GetStatus(StatusType.Radiance);
            if (source.GetStatus(StatusType.Weak) > 0)
            {
                dmg = dmg * 3 / 4;
            }
            if (target.GetStatus(StatusType.Vulnerable) > 0)
            {
                dmg = dmg * 3 / 2;
            }
            return dmg < 0 ? 0 : dmg;
        }
    }
}

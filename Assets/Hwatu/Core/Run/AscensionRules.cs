namespace Hwatu.Core.Run
{
    /// <summary>어센션(난이도) 모디파이어. 값이 클수록 어렵다(STS의 Ascension). 0=기본.</summary>
    public static class AscensionRules
    {
        public const int MaxAscension = 5;

        /// <summary>적 최대 HP에 곱하는 퍼센트(100=기본). asc당 +5%.</summary>
        public static int EnemyHpPercent(int ascension)
        {
            int a = ascension < 0 ? 0 : ascension;
            return 100 + a * 5;
        }

        /// <summary>플레이어 시작 최대 HP 페널티(asc당 -3).</summary>
        public static int StartHpPenalty(int ascension)
        {
            int a = ascension < 0 ? 0 : ascension;
            return a * 3;
        }
    }
}

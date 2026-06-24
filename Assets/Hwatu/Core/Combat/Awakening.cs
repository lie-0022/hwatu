namespace Hwatu.Core.Combat
{
    /// <summary>
    /// 백호 산군 각성 단계(位階) — 위엄(Majesty) 누적에 따라 형상·패시브가 변모(캐릭터 문서 §2.4).
    /// 1단계(0~4) 기본 / 2단계(5~9) 매 턴 위엄+1 / 3단계(10~14) 공격 +25% / 4단계(15+) 받는 피해 -25%.
    /// (4단계 '전체 공격'은 타깃팅 변경이 커서 후속.) 위엄 값에서 파생되므로 별도 상태를 두지 않는다.
    /// </summary>
    public static class Awakening
    {
        /// <summary>위엄 누적 → 각성 단계(1~4).</summary>
        public static int Tier(int majesty)
            => majesty >= 15 ? 4 : majesty >= 10 ? 3 : majesty >= 5 ? 2 : 1;

        /// <summary>단계 이름(UI 표시 — 형상 변모).</summary>
        public static string TierName(int tier)
        {
            switch (tier)
            {
                case 2:  return "분노한 산군";
                case 3:  return "영물";
                case 4:  return "신수";
                default: return "산군";
            }
        }
    }
}

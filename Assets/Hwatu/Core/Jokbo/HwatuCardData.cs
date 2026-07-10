namespace Hwatu.Core.Jokbo
{
    /// <summary>
    /// 족보 전투의 카드(불변) — 월(1~12) × 등급이 정체성의 전부(고유 텍스트 없음, 설계 §2).
    /// 효과 수치는 JokboRules가 계산하고, 여기는 데이터만 든다. 강화(+)는 Enchant로 기본치 가산.
    /// </summary>
    public sealed class HwatuCardData
    {
        /// <summary>월(1~12).</summary>
        public int Month { get; }
        public HwatuCardKind Kind { get; }
        /// <summary>쌍피 여부(피 2장 취급 — 방어치 2배).</summary>
        public bool IsDouble { get; }
        /// <summary>강화 누적치(휴식 강화 시 +2, 카드당 1회).</summary>
        public int Enchant { get; }
        /// <summary>표시명(예: "1월 송학 홍단").</summary>
        public string Name { get; }

        public HwatuCardData(int month, HwatuCardKind kind, bool isDouble = false, int enchant = 0, string name = null)
        {
            Month = month;
            Kind = kind;
            IsDouble = isDouble;
            Enchant = enchant;
            Name = name ?? $"{month}월 {KindKor(kind)}";
        }

        /// <summary>띠 색(월에서 파생). 띠가 아니면 None.</summary>
        public RibbonColor Ribbon
        {
            get
            {
                if (Kind != HwatuCardKind.Ribbon) { return RibbonColor.None; }
                switch (Month)
                {
                    case 1: case 2: case 3:  return RibbonColor.Red;
                    case 6: case 9: case 10: return RibbonColor.Blue;
                    case 4: case 5: case 7:  return RibbonColor.Plain;
                    default:                 return RibbonColor.Rain;   // 12월 비띠
                }
            }
        }

        /// <summary>고도리 재료(2·4·8월의 새 열끗)인가.</summary>
        public bool IsGodoriBird => Kind == HwatuCardKind.Animal && (Month == 2 || Month == 4 || Month == 8);

        /// <summary>비광(12월 광 — 삼광에서 반 대접)인가.</summary>
        public bool IsRainBright => Kind == HwatuCardKind.Bright && Month == 12;

        /// <summary>강화된 사본을 만든다(카드당 1회 — 이미 강화됐으면 그대로, 기존 규칙 유지).</summary>
        public HwatuCardData Upgrade()
        {
            if (Enchant > 0) { return this; }
            return new HwatuCardData(Month, Kind, IsDouble, JokboRules.EnchantBonus, Name + "+");
        }

        private static string KindKor(HwatuCardKind k)
        {
            switch (k)
            {
                case HwatuCardKind.Bright: return "광";
                case HwatuCardKind.Animal: return "열끗";
                case HwatuCardKind.Ribbon: return "띠";
                default:                   return "피";
            }
        }
    }
}

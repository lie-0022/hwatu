namespace Hwatu.Core.Jokbo
{
    /// <summary>화투 카드 등급(원전 4종). 공격치·방어치는 JokboRules가 결정한다.</summary>
    public enum HwatuCardKind
    {
        Bright,   // 광(光) — 5장, 최고 등급
        Animal,   // 열끗(동물) — 9장
        Ribbon,   // 띠(단) — 10장
        Chaff,    // 피(皮) — 24장(쌍피 포함)
    }

    /// <summary>띠의 세부 색(월에서 파생). 홍단 1·2·3 / 청단 6·9·10 / 초단 4·5·7 / 비띠 12.</summary>
    public enum RibbonColor
    {
        None,
        Red,     // 홍단
        Blue,    // 청단
        Plain,   // 초단
        Rain,    // 비띠(12월 — 어느 단에도 속하지 않음)
    }
}

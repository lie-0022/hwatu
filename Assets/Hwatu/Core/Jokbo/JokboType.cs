namespace Hwatu.Core.Jokbo
{
    /// <summary>제출 가능한 족보(설계 §4). 판정은 JokboDetector, 배수·효과는 JokboRules.</summary>
    public enum JokboType
    {
        None,             // 족보 아님(제출 불가)
        Single,           // 낱장 ×1/2 — 비상용
        Pair,             // 먹기: 같은 월 2장 ×1 — 기본기
        KindTriple,       // 같은 등급 3장 ×3/2
        Shake,            // 흔들기: 같은 월 3장 ×2
        Bomb,             // 폭탄: 같은 월 4장 ×3, 전체
        RedRibbons,       // 홍단(1·2·3월 띠) ×2 + 화상
        BlueRibbons,      // 청단(6·9·10월 띠) ×2 + 약화
        PlainRibbons,     // 초단(4·5·7월 띠) ×2 + 취약
        Godori,           // 고도리(2·4·8월 열끗) ×2, 3연타 + 드로우
        ThreeBrights,     // 삼광(비광 제외) ×3/2, 전체
        ThreeBrightsRain, // 비삼광(비광 포함) ×1, 전체
        FourBrights,      // 사광 ×2, 전체
        FiveBrights,      // 오광 ×3, 전체 + 전체 약화
    }
}

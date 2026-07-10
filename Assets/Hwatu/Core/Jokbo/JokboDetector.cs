using System.Collections.Generic;

namespace Hwatu.Core.Jokbo
{
    /// <summary>
    /// 선택한 카드 묶음 → 족보 판정(순수 함수, 설계 §4). 겹치면 항상 더 높은 족보를 고른다:
    /// 오광 > 사광 > 폭탄 > 흔들기 > 홍·청·초단·고도리 > 삼광(비삼광) > 같은 끗 셋 > 먹기 > 낱장.
    /// </summary>
    public static class JokboDetector
    {
        public static JokboType Detect(IReadOnlyList<HwatuCardData> cards)
        {
            if (cards == null || cards.Count == 0 || cards.Count > JokboRules.MaxSubmitCards)
            {
                return JokboType.None;
            }

            int n = cards.Count;
            bool allBright = AllKind(cards, HwatuCardKind.Bright);
            bool sameMonth = SameMonth(cards);

            if (n == 5)
            {
                return allBright ? JokboType.FiveBrights : JokboType.None;
            }
            if (n == 4)
            {
                if (allBright) { return JokboType.FourBrights; }
                if (sameMonth) { return JokboType.Bomb; }
                return JokboType.None;
            }
            if (n == 3)
            {
                if (sameMonth) { return JokboType.Shake; }   // 광 3장은 월이 전부 달라 충돌 없음
                if (allBright)
                {
                    return HasRainBright(cards) ? JokboType.ThreeBrightsRain : JokboType.ThreeBrights;
                }
                if (AllKind(cards, HwatuCardKind.Ribbon))
                {
                    RibbonColor c0 = cards[0].Ribbon;
                    if (c0 != RibbonColor.None && c0 != RibbonColor.Rain
                        && cards[1].Ribbon == c0 && cards[2].Ribbon == c0)
                    {
                        switch (c0)
                        {
                            case RibbonColor.Red:  return JokboType.RedRibbons;
                            case RibbonColor.Blue: return JokboType.BlueRibbons;
                            default:               return JokboType.PlainRibbons;
                        }
                    }
                    return JokboType.KindTriple;   // 색 섞인 띠 3장
                }
                if (AllKind(cards, HwatuCardKind.Animal))
                {
                    if (cards[0].IsGodoriBird && cards[1].IsGodoriBird && cards[2].IsGodoriBird
                        && !SameMonthAny(cards))
                    {
                        return JokboType.Godori;   // 2·4·8월 새 셋(중복 월 없음)
                    }
                    return JokboType.KindTriple;
                }
                if (AllKind(cards, HwatuCardKind.Chaff)) { return JokboType.KindTriple; }
                return JokboType.None;   // 등급 잡탕 3장
            }
            if (n == 2)
            {
                return sameMonth ? JokboType.Pair : JokboType.None;
            }
            return JokboType.Single;
        }

        private static bool AllKind(IReadOnlyList<HwatuCardData> cards, HwatuCardKind kind)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Kind != kind) { return false; }
            }
            return true;
        }

        private static bool SameMonth(IReadOnlyList<HwatuCardData> cards)
        {
            for (int i = 1; i < cards.Count; i++)
            {
                if (cards[i].Month != cards[0].Month) { return false; }
            }
            return true;
        }

        // 어느 두 장이라도 같은 월인가(고도리는 2·4·8 서로 다른 월 3장이어야 함).
        private static bool SameMonthAny(IReadOnlyList<HwatuCardData> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                for (int j = i + 1; j < cards.Count; j++)
                {
                    if (cards[i].Month == cards[j].Month) { return true; }
                }
            }
            return false;
        }

        private static bool HasRainBright(IReadOnlyList<HwatuCardData> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].IsRainBright) { return true; }
            }
            return false;
        }
    }
}

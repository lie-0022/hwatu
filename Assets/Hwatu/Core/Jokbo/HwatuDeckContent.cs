using System.Collections.Generic;

namespace Hwatu.Core.Jokbo
{
    /// <summary>
    /// 화투 48장 원전 구성(화투_룰_정리.md §2.1)과 공통 시작 덱(설계 §3.1)의 코드 빌더 — 단일 진실원.
    /// </summary>
    public static class HwatuDeckContent
    {
        private static readonly string[] s_monthNames =
        {
            "송학", "매조", "벚꽃", "흑싸리", "난초", "모란",
            "홍싸리", "공산", "국진", "단풍", "오동", "비",
        };

        /// <summary>월 이름(1~12).</summary>
        public static string MonthName(int month) => s_monthNames[month - 1];

        private static HwatuCardData Make(int month, HwatuCardKind kind, bool isDouble = false)
        {
            string kindKor;
            switch (kind)
            {
                case HwatuCardKind.Bright: kindKor = month == 12 ? "비광" : "광"; break;
                case HwatuCardKind.Animal: kindKor = "열끗"; break;
                case HwatuCardKind.Ribbon:
                    switch (new HwatuCardData(month, kind).Ribbon)
                    {
                        case RibbonColor.Red:   kindKor = "홍단"; break;
                        case RibbonColor.Blue:  kindKor = "청단"; break;
                        case RibbonColor.Plain: kindKor = "초단"; break;
                        default:                kindKor = "비띠"; break;
                    }
                    break;
                default: kindKor = isDouble ? "쌍피" : "피"; break;
            }
            return new HwatuCardData(month, kind, isDouble, 0, $"{month}월 {MonthName(month)} {kindKor}");
        }

        /// <summary>원전 48장 전체(월별 4장 — 광5·열끗9·띠10·피24, 쌍피는 11·12월).</summary>
        public static List<HwatuCardData> FullDeck()
        {
            var deck = new List<HwatuCardData>(48);
            for (int m = 1; m <= 12; m++)
            {
                switch (m)
                {
                    case 1: case 3:   // 송학·벚꽃: 광 + 홍단 + 피2
                        deck.Add(Make(m, HwatuCardKind.Bright));
                        deck.Add(Make(m, HwatuCardKind.Ribbon));
                        deck.Add(Make(m, HwatuCardKind.Chaff));
                        deck.Add(Make(m, HwatuCardKind.Chaff));
                        break;
                    case 8:           // 공산: 광 + 열끗(새) + 피2
                        deck.Add(Make(m, HwatuCardKind.Bright));
                        deck.Add(Make(m, HwatuCardKind.Animal));
                        deck.Add(Make(m, HwatuCardKind.Chaff));
                        deck.Add(Make(m, HwatuCardKind.Chaff));
                        break;
                    case 11:          // 오동: 광 + 피2 + 쌍피
                        deck.Add(Make(m, HwatuCardKind.Bright));
                        deck.Add(Make(m, HwatuCardKind.Chaff));
                        deck.Add(Make(m, HwatuCardKind.Chaff));
                        deck.Add(Make(m, HwatuCardKind.Chaff, isDouble: true));
                        break;
                    case 12:          // 비: 비광 + 열끗 + 비띠 + 쌍피
                        deck.Add(Make(m, HwatuCardKind.Bright));
                        deck.Add(Make(m, HwatuCardKind.Animal));
                        deck.Add(Make(m, HwatuCardKind.Ribbon));
                        deck.Add(Make(m, HwatuCardKind.Chaff, isDouble: true));
                        break;
                    default:          // 2·4·5·6·7·9·10: 열끗 + 띠 + 피2
                        deck.Add(Make(m, HwatuCardKind.Animal));
                        deck.Add(Make(m, HwatuCardKind.Ribbon));
                        deck.Add(Make(m, HwatuCardKind.Chaff));
                        deck.Add(Make(m, HwatuCardKind.Chaff));
                        break;
                }
            }
            return deck;
        }

        /// <summary>
        /// 공통 시작 덱 "일곱 달의 화투패" 14장(설계 §3.1 확정) — 7개 월 × 2장, 모든 카드가 먹기 짝 보유.
        /// 명명 족보 재료는 전부 1장씩(홍단·초단·청단·고도리·삼광 훅).
        /// </summary>
        public static List<HwatuCardData> CommonStarterDeck()
        {
            return new List<HwatuCardData>
            {
                Make(1, HwatuCardKind.Ribbon),  Make(1, HwatuCardKind.Chaff),    // 홍단 1/3
                Make(2, HwatuCardKind.Animal),  Make(2, HwatuCardKind.Chaff),    // 고도리 1/3
                Make(5, HwatuCardKind.Ribbon),  Make(5, HwatuCardKind.Chaff),    // 초단 1/3
                Make(6, HwatuCardKind.Ribbon),  Make(6, HwatuCardKind.Chaff),    // 청단 1/3
                Make(7, HwatuCardKind.Animal),  Make(7, HwatuCardKind.Chaff),
                Make(9, HwatuCardKind.Animal),  Make(9, HwatuCardKind.Chaff),
                Make(11, HwatuCardKind.Bright), Make(11, HwatuCardKind.Chaff),   // 삼광 1/5
            };
        }
    }
}

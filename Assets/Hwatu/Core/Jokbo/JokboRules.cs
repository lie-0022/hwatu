using System.Collections.Generic;

namespace Hwatu.Core.Jokbo
{
    /// <summary>
    /// 족보 전투 수치의 단일 출처(설계 §2·§4 baseline v0 — 시뮬로 튜닝, 추후 SO 외부화).
    /// 배수는 정수 분수(분자/분모)로 정의해 float 없이 결정론을 지킨다(내림).
    /// </summary>
    public static class JokboRules
    {
        // ── 카드 기본치 ──
        public const int BrightAttack = 8;
        public const int AnimalAttack = 6;
        public const int RibbonAttack = 4;
        public const int ChaffDefense = 3;
        public const int DoubleChaffDefense = 6;
        /// <summary>강화(+) 1회의 기본치 가산.</summary>
        public const int EnchantBonus = 2;

        // ── 턴 경제(설계 §3) ──
        public const int HandSize = 8;
        public const int PlaysPerTurn = 2;
        public const int DiscardsPerTurn = 2;
        public const int MaxSubmitCards = 5;

        /// <summary>카드의 공격 기본치(피는 0). 강화 가산 포함.</summary>
        public static int AttackOf(HwatuCardData c)
        {
            switch (c.Kind)
            {
                case HwatuCardKind.Bright: return BrightAttack + c.Enchant;
                case HwatuCardKind.Animal: return AnimalAttack + c.Enchant;
                case HwatuCardKind.Ribbon: return RibbonAttack + c.Enchant;
                default:                   return 0;
            }
        }

        /// <summary>카드의 방어 기본치(피만, 쌍피 2배). 강화 가산 포함.</summary>
        public static int DefenseOf(HwatuCardData c)
        {
            if (c.Kind != HwatuCardKind.Chaff) { return 0; }
            return (c.IsDouble ? DoubleChaffDefense : ChaffDefense) + c.Enchant;
        }

        /// <summary>족보 배수 분자(설계 §4 — ×분자/분모, 내림).</summary>
        public static int MultNum(JokboType t)
        {
            switch (t)
            {
                case JokboType.Single:           return 1;
                case JokboType.Pair:             return 1;
                case JokboType.KindTriple:       return 3;
                case JokboType.Shake:            return 2;
                case JokboType.Bomb:             return 3;
                case JokboType.RedRibbons:
                case JokboType.BlueRibbons:
                case JokboType.PlainRibbons:     return 2;
                case JokboType.Godori:           return 2;
                case JokboType.ThreeBrights:     return 3;
                case JokboType.ThreeBrightsRain: return 1;
                case JokboType.FourBrights:      return 2;
                case JokboType.FiveBrights:      return 3;
                default:                         return 0;
            }
        }

        /// <summary>족보 배수 분모.</summary>
        public static int MultDen(JokboType t)
        {
            switch (t)
            {
                case JokboType.Single:       return 2;   // ×1/2
                case JokboType.KindTriple:   return 2;   // ×3/2
                case JokboType.ThreeBrights: return 2;   // ×3/2
                default:                     return 1;
            }
        }

        /// <summary>전체 공격 족보인가(폭탄·광 계열).</summary>
        public static bool IsAoe(JokboType t)
        {
            switch (t)
            {
                case JokboType.Bomb:
                case JokboType.ThreeBrights:
                case JokboType.ThreeBrightsRain:
                case JokboType.FourBrights:
                case JokboType.FiveBrights:
                    return true;
                default:
                    return false;
            }
        }

        // ── 명명 족보 시그니처(설계 §4.2 v0 — 결정 #5 초안) ──
        /// <summary>홍단: 적 화상(중독) 부여량.</summary>
        public const int RedRibbonsBurn = 4;
        /// <summary>청단: 적 약화 부여량.</summary>
        public const int BlueRibbonsWeak = 2;
        /// <summary>초단: 적 취약 부여량.</summary>
        public const int PlainRibbonsVulnerable = 2;
        /// <summary>고도리: 타격 분할 수 / 드로우.</summary>
        public const int GodoriHits = 3;
        public const int GodoriDraw = 2;
        /// <summary>오광: 적 전체 약화 부여량.</summary>
        public const int FiveBrightsWeakAll = 2;

        /// <summary>족보 한글 표시명.</summary>
        public static string DisplayName(JokboType t)
        {
            switch (t)
            {
                case JokboType.Single:           return "낱장";
                case JokboType.Pair:             return "먹기";
                case JokboType.KindTriple:       return "같은 끗 셋";
                case JokboType.Shake:            return "흔들기";
                case JokboType.Bomb:             return "폭탄";
                case JokboType.RedRibbons:       return "홍단";
                case JokboType.BlueRibbons:      return "청단";
                case JokboType.PlainRibbons:     return "초단";
                case JokboType.Godori:           return "고도리";
                case JokboType.ThreeBrights:     return "삼광";
                case JokboType.ThreeBrightsRain: return "비삼광";
                case JokboType.FourBrights:      return "사광";
                case JokboType.FiveBrights:      return "오광";
                default:                         return "족보 없음";
            }
        }

        /// <summary>제출 카드의 공격합(강화 포함) — 족보 배수 적용 전.</summary>
        public static int AttackSum(IReadOnlyList<HwatuCardData> cards)
        {
            int sum = 0;
            for (int i = 0; i < cards.Count; i++) { sum += AttackOf(cards[i]); }
            return sum;
        }

        /// <summary>제출 카드의 방어합(강화 포함) — 족보 배수 적용 전.</summary>
        public static int DefenseSum(IReadOnlyList<HwatuCardData> cards)
        {
            int sum = 0;
            for (int i = 0; i < cards.Count; i++) { sum += DefenseOf(cards[i]); }
            return sum;
        }
    }
}

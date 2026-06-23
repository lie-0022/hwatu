using UnityEngine;

namespace Hwatu.Game
{
    /// <summary>
    /// 디자인 토큰 — 색·폰트크기·간격·아이콘 규격의 단일 출처(SSOT).
    /// 모든 UI는 하드코딩 대신 이 토큰을 참조해 컨셉(화투 먹·금) 일치 + 크기 일관성을 보장한다.
    /// 색 근거: docs/캐릭터_백호_산군.md §8.4(백색·먹빛·금·영혼불) + 화투 전통(먹·금·적·청).
    /// 아이콘은 IconSm/Md/Lg 3규격만 사용해 화면마다 크기가 들쑥날쑥하지 않게 한다.
    /// </summary>
    public static class DesignTokens
    {
        // ── 색 팔레트 (화투 먹·금 테마) ──
        public static readonly Color Ink      = new Color(0.10f, 0.10f, 0.11f);   // 먹빛 배경 (#1A1A1C)
        public static readonly Color Panel    = new Color(0.16f, 0.15f, 0.17f);   // 패널/카드 배경
        public static readonly Color PanelHi   = new Color(0.22f, 0.21f, 0.24f);   // 패널 강조(hover/선택)
        public static readonly Color Paper    = new Color(0.96f, 0.96f, 0.94f);   // 한지 백색 (#F5F5F0)
        public static readonly Color Gold      = new Color(0.83f, 0.69f, 0.22f);   // 금 강조/테두리 (#D4AF37)
        public static readonly Color Spirit    = new Color(0.50f, 0.85f, 0.91f);   // 영혼불 청백 (#7FD8E8)
        public static readonly Color TextMain  = new Color(0.93f, 0.93f, 0.90f);   // 본문 텍스트
        public static readonly Color TextDim   = new Color(0.62f, 0.62f, 0.60f);   // 흐린 텍스트(보조)

        // ── 의미 색 (status·속성 — 아이콘/칩 통일) ──
        public static readonly Color Danger    = new Color(0.86f, 0.24f, 0.22f);   // 공격/위험(적)
        public static readonly Color Defense   = new Color(0.36f, 0.62f, 0.86f);   // 방어(청)
        public static readonly Color Radiance  = new Color(0.95f, 0.82f, 0.35f);   // 광(금빛)
        public static readonly Color Majesty   = new Color(0.85f, 0.70f, 0.30f);   // 위엄(금)
        public static readonly Color Poison    = new Color(0.55f, 0.78f, 0.30f);   // 독(독녹)
        public static readonly Color Weak      = new Color(0.66f, 0.50f, 0.80f);   // 약화(보라)
        public static readonly Color Vulnerable = new Color(0.90f, 0.55f, 0.30f);  // 취약(주황)
        public static readonly Color Heal      = new Color(0.45f, 0.80f, 0.50f);   // 회복/재생(녹)
        public static readonly Color Thorns    = new Color(0.70f, 0.60f, 0.55f);   // 가시(갈)

        // ── 폰트 크기 (일관 스케일) ──
        public const int FontTitle   = 40;
        public const int FontHeading = 28;
        public const int FontBody    = 20;
        public const int FontSmall   = 16;
        public const int FontNumber  = 32;   // HP·데미지 등 강조 숫자

        // ── 간격(spacing) ──
        public const float SpaceXs = 4f;
        public const float SpaceSm = 8f;
        public const float SpaceMd = 16f;
        public const float SpaceLg = 24f;

        // ── 아이콘 크기 규격 (3단계만 — 들쑥날쑥 방지) ──
        public const float IconSm = 24f;   // status 칩·인라인 숫자 옆
        public const float IconMd = 40f;   // 유물·적 인텐트
        public const float IconLg = 64f;   // 보상·캐릭터 등 강조

        /// <summary>status별 의미 색을 돌려준다(아이콘·칩·텍스트 통일에 사용).</summary>
        public static Color StatusColor(Hwatu.Core.Combat.StatusType s)
        {
            switch (s)
            {
                case Hwatu.Core.Combat.StatusType.Radiance:   return Radiance;
                case Hwatu.Core.Combat.StatusType.Majesty:    return Majesty;
                case Hwatu.Core.Combat.StatusType.Poison:     return Poison;
                case Hwatu.Core.Combat.StatusType.Weak:       return Weak;
                case Hwatu.Core.Combat.StatusType.Vulnerable: return Vulnerable;
                case Hwatu.Core.Combat.StatusType.Regen:      return Heal;
                case Hwatu.Core.Combat.StatusType.Thorns:     return Thorns;
                case Hwatu.Core.Combat.StatusType.Dexterity:  return Defense;
                default:                                      return TextMain;
            }
        }
    }
}

using Hwatu.Core.Combat;
using Hwatu.Core.Enemies;

namespace Hwatu.Game
{
    /// <summary>
    /// status·인텐트·자원 → Iconify game-icons 아이콘 이름 매핑(디자인 시스템 §4 코드화·SSOT).
    /// 실제 로딩은 IconLoader가 담당. 여기는 "무엇을 그릴지"만 결정한다.
    /// 모든 이름은 api.iconify.design/game-icons 에서 존재(HTTP 200) 검증 완료.
    /// </summary>
    public static class IconCatalog
    {
        public const string Prefix = "game-icons";

        /// <summary>status별 game-icons 이름. 색은 DesignTokens.StatusColor로 별도.</summary>
        public static string ForStatus(StatusType s)
        {
            switch (s)
            {
                case StatusType.Radiance:   return "sun";
                case StatusType.Majesty:    return "tiger-head";
                case StatusType.Poison:     return "poison-bottle";
                case StatusType.Weak:       return "broken-bone";
                case StatusType.Vulnerable: return "cracked-shield";
                case StatusType.Regen:      return "health-normal";
                case StatusType.Thorns:     return "spiked-shield";
                case StatusType.Dexterity:  return "run";
                default:                    return "help";
            }
        }

        /// <summary>적 인텐트별 game-icons 이름.</summary>
        public static string ForIntent(IntentType intent)
        {
            switch (intent)
            {
                case IntentType.Attack:
                case IntentType.AttackMulti: return "bloody-sword";
                case IntentType.Block:       return "round-shield";
                case IntentType.Buff:        return "upgrade";
                case IntentType.Debuff:      return "broken-bone";
                case IntentType.Doom:        return "death-skull";
                default:                     return "help";
            }
        }

        // 공용 리소스 아이콘
        public const string Hp = "health-normal";
        public const string Gold = "two-coins";
        public const string Energy = "lightning-arc";
    }
}

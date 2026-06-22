using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Run;

namespace Hwatu.Game
{
    /// <summary>
    /// UI 툴팁용 한글 설명 사전(status·키워드·유물·포션). STS2식 "거의 완전 정보" — hover로 능력 확인.
    /// 설계 출처: docs/systems/04-ui-ux.md §7.
    /// </summary>
    public static class GameInfo
    {
        /// <summary>status 한글 표시명.</summary>
        public static string StatusName(StatusType s)
        {
            switch (s)
            {
                case StatusType.Radiance:   return "광";
                case StatusType.Weak:       return "약화";
                case StatusType.Vulnerable: return "취약";
                case StatusType.Poison:     return "중독";
                case StatusType.Dexterity:  return "민첩";
                default:                    return s.ToString();
            }
        }

        /// <summary>status 툴팁(이름+수치 + 효과 설명).</summary>
        public static string StatusDesc(StatusType s, int amount)
        {
            switch (s)
            {
                case StatusType.Radiance:   return $"<b>광 {amount}</b>\n공격 피해 +{amount}";
                case StatusType.Weak:       return $"<b>약화 {amount}</b>\n주는 공격 피해 25% 감소 ({amount}턴)";
                case StatusType.Vulnerable: return $"<b>취약 {amount}</b>\n받는 공격 피해 50% 증가 ({amount}턴)";
                case StatusType.Poison:     return $"<b>중독 {amount}</b>\n턴 시작 시 {amount} 피해, 이후 1 감소";
                case StatusType.Dexterity:  return $"<b>민첩 {amount}</b>\n방어 획득량 +{amount}";
                default:                    return $"{s} {amount}";
            }
        }

        /// <summary>카드 키워드 설명(없으면 빈 문자열).</summary>
        public static string KeywordDesc(CardData d)
        {
            var lines = new List<string>();
            if (d.Innate)   { lines.Add("<b>선제</b>: 전투 첫 손패에 보장"); }
            if (d.Retain)   { lines.Add("<b>유지</b>: 턴 끝에 버리지 않음"); }
            if (d.Exhaust)  { lines.Add("<b>소멸</b>: 사용 시 이번 전투에서 덱 제거"); }
            if (d.Ethereal) { lines.Add("<b>휘발</b>: 턴 끝까지 미사용 시 소멸"); }
            return string.Join("\n", lines);
        }

        /// <summary>유물 툴팁(이름 + 효과).</summary>
        public static string RelicDesc(RelicData r)
        {
            return $"<b>{r.Name}</b>\n{r.Description}";
        }

        /// <summary>포션 툴팁(이름 + 효과).</summary>
        public static string PotionDesc(PotionData p)
        {
            return $"<b>{p.Name}</b>\n{p.Description}";
        }
    }
}

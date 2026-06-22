using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;
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

        /// <summary>카드 전체 효과 상세(hover 툴팁): 이름·코스트·레어도 + 효과 줄별 + 키워드.</summary>
        public static string CardDesc(CardData d)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"<b>{d.Name}</b>  <size=78%>(코스트 {d.Cost} · {RarityName(d.Rarity)})</size>\n");
            foreach (EffectData e in d.Effects)
            {
                sb.Append("• ").Append(EffectLine(e)).Append('\n');
            }
            string kw = KeywordDesc(d);
            if (kw.Length > 0)
            {
                sb.Append('\n').Append(kw);
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>효과 1개를 사람이 읽는 한 줄로.</summary>
        private static string EffectLine(EffectData e)
        {
            if (e.Op == EffectOp.DealDamage)   { return $"{e.Amount} 피해"; }
            if (e.Op == EffectOp.GainBlock)    { return $"방어 {e.Amount} 획득"; }
            if (e.Op == EffectOp.GainResource) { return $"광 +{e.Amount} (공격 피해 증가)"; }
            if (e.Op == EffectOp.Draw)         { return $"카드 {e.Amount}장 뽑기"; }
            if (e.Op == EffectOp.ApplyStatus)
            {
                string tgt = e.Target == TargetType.Enemy ? "적에게 " : "나에게 ";
                return $"{tgt}{StatusName(e.Status)} {e.Amount} — {StatusShort(e.Status)}";
            }
            if (e.Op == EffectOp.ClearStatus)  { return $"{StatusName(e.Status)} 제거"; }
            return e.Op;
        }

        private static string StatusShort(StatusType s)
        {
            switch (s)
            {
                case StatusType.Radiance:   return "공격 피해 증가";
                case StatusType.Weak:       return "주는 피해 감소";
                case StatusType.Vulnerable: return "받는 피해 증가";
                case StatusType.Poison:     return "매 턴 피해";
                case StatusType.Dexterity:  return "방어 증가";
                default:                    return "";
            }
        }

        private static string RarityName(CardRarity r)
        {
            switch (r)
            {
                case CardRarity.Common:   return "일반";
                case CardRarity.Uncommon: return "고급";
                case CardRarity.Rare:     return "희귀";
                default:                  return r.ToString();
            }
        }
    }
}

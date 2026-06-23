using System.Collections.Generic;
using Hwatu.Core.Combat;
using Hwatu.Core.Run;

namespace Hwatu.Core.Content
{
    /// <summary>유물 5종(SPEC §8). MVP는 전투 시작 효과 중심.</summary>
    public static class RelicContent
    {
        public static RelicData Cushion() => new RelicData("relic_cushion", "방석", "전투 시작 시 광 +1.",
            p => p.AddStatus(StatusType.Radiance, 1));

        public static RelicData Blanket() => new RelicData("relic_blanket", "담요", "전투 시작 시 방어 +5.",
            p => p.SetBlock(p.Block + 5));

        public static RelicData Lantern() => new RelicData("relic_lantern", "등잔", "시작 유물(상징).");

        public static RelicData CoinPouch() => new RelicData("relic_coin", "엽전꾸러미", "전투 보상 골드 증가(런 처리, 후속).");

        public static RelicData Whetstone() => new RelicData("relic_whet", "숫돌", "전투 시작 시 광 +1(임시; 후속에 '첫 공격 +3'으로).",
            p => p.AddStatus(StatusType.Radiance, 1));

        public static RelicData Charm() => new RelicData("relic_charm", "부적", "전투 시작 시 방어 +8.",
            p => p.SetBlock(p.Block + 8));

        public static RelicData RuneStone() => new RelicData("relic_rune", "룬돌", "전투 시작 시 광 +2.",
            p => p.AddStatus(StatusType.Radiance, 2));

        public static RelicData SteelScale() => new RelicData("relic_steel", "강철비늘", "전투 시작 시 민첩 +1.",
            p => p.AddStatus(StatusType.Dexterity, 1));

        public static RelicData RiceCake() => new RelicData("relic_ricecake", "떡", "전투 시작 시 방어 +3.",
            p => p.SetBlock(p.Block + 3));

        public static RelicData Gourd() => new RelicData("relic_gourd", "호리병", "전투 시작 시 광 +1, 방어 +3.",
            p => { p.AddStatus(StatusType.Radiance, 1); p.SetBlock(p.Block + 3); });

        public static RelicData InkStone() => new RelicData("relic_inkstone", "먹", "전투 시작 시 광 +1, 민첩 +1.",
            p => { p.AddStatus(StatusType.Radiance, 1); p.AddStatus(StatusType.Dexterity, 1); });

        public static RelicData Herb() => new RelicData("relic_herb", "약초", "전투 시작 시 HP +4.",
            p => p.SetHp(System.Math.Min(p.MaxHp, p.Hp + 4)));

        public static RelicData RegenCharm() => new RelicData("relic_regen", "재생부적", "전투 시작 시 재생 3(이후 턴마다 회복).",
            p => p.AddStatus(StatusType.Regen, 3));

        public static RelicData ThornMail() => new RelicData("relic_thornmail", "가시갑옷", "전투 시작 시 가시 2(피격 시 반사).",
            p => p.AddStatus(StatusType.Thorns, 2));

        // 트리거 다양성(연구 §3.2): '전투 시작'이 아니라 '매 턴 시작'에 발동하는 유물
        public static RelicData EchoHide() => new RelicData("relic_echohide", "산울림 가죽", "매 턴 시작 시 방어 +4.",
            null, p => p.SetBlock(p.Block + 4));

        // 트리거 다양성(연구 §3.2): '카드를 낼 때마다' 발동하는 유물
        public static RelicData Abacus() => new RelicData("relic_abacus", "주판", "카드를 1장 낼 때마다 방어 +1.",
            null, null, p => p.SetBlock(p.Block + 1));

        /// <summary>전체 유물 풀(보물/엘리트/보스 보상 추첨용).</summary>
        public static List<RelicData> AllRelics()
        {
            return new List<RelicData>
            {
                Cushion(), Blanket(), Lantern(), CoinPouch(), Whetstone(),
                Charm(), RuneStone(), SteelScale(), RiceCake(), Gourd(), InkStone(), Herb(), RegenCharm(), ThornMail(), EchoHide(), Abacus(),
            };
        }
    }
}

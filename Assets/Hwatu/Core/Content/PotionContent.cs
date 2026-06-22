using System.Collections.Generic;
using Hwatu.Core.Combat;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Core.Content
{
    /// <summary>포션 정의 + 시드 선택. 전투 중 PlayerState에 즉시 효과(보유 슬롯/사용 UI는 후속).</summary>
    public static class PotionContent
    {
        public static PotionData Strength() => new PotionData("pot_str", "힘약", "광 +2.",
            p => p.AddStatus(StatusType.Radiance, 2));

        public static PotionData Block() => new PotionData("pot_block", "방패약", "방어 +12.",
            p => p.SetBlock(p.Block + 12));

        public static PotionData Swift() => new PotionData("pot_swift", "민첩약", "민첩 +2.",
            p => p.AddStatus(StatusType.Dexterity, 2));

        public static PotionData Antidote() => new PotionData("pot_antidote", "해독약", "중독 제거.",
            p => { int pz = p.GetStatus(StatusType.Poison); if (pz > 0) { p.AddStatus(StatusType.Poison, -pz); } });

        public static PotionData Heal() => new PotionData("pot_heal", "회복약", "HP 15 회복.",
            p => p.SetHp(System.Math.Min(p.MaxHp, p.Hp + 15)));

        public static PotionData Valor() => new PotionData("pot_valor", "강심약", "광 +3, 방어 +6.",
            p => { p.AddStatus(StatusType.Radiance, 3); p.SetBlock(p.Block + 6); });

        public static List<PotionData> All()
        {
            return new List<PotionData> { Strength(), Block(), Swift(), Antidote(), Heal(), Valor() };
        }

        /// <summary>시드로 포션 1개 선택.</summary>
        public static PotionData Pick(IRandom rng)
        {
            List<PotionData> all = All();
            return all[rng.NextInt(all.Count)];
        }
    }
}

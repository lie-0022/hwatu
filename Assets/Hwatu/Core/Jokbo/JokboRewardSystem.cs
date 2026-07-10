using System.Collections.Generic;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Core.Jokbo
{
    /// <summary>
    /// 족보 전투 카드 보상(J5) — 화투 48장 풀에서 등급 가중으로 N택(기본 3). 시드 결정론.
    /// 광은 귀하게, 피는 흔하게. 엘리트·보스일수록 상위 카드(광·핵심 띠·고도리 새) 가중↑.
    /// </summary>
    public static class JokboRewardSystem
    {
        /// <summary>보상 후보 N장(월·등급 중복 없이).</summary>
        public static List<HwatuCardData> RollCardReward(IRandom rng, EncounterType enc, int count = 3)
        {
            List<HwatuCardData> pool = HwatuDeckContent.FullDeck();
            var result = new List<HwatuCardData>();
            var used = new HashSet<string>();
            for (int i = 0; i < count; i++)
            {
                HwatuCardData pick = WeightedPick(rng, pool, enc, used);
                if (pick == null) { break; }
                result.Add(pick);
                used.Add(Key(pick));
            }
            return result;
        }

        private static HwatuCardData WeightedPick(IRandom rng, List<HwatuCardData> pool, EncounterType enc, HashSet<string> used)
        {
            int total = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                if (used.Contains(Key(pool[i]))) { continue; }
                total += Weight(pool[i], enc);
            }
            if (total <= 0) { return null; }
            int roll = rng.NextInt(total);
            for (int i = 0; i < pool.Count; i++)
            {
                if (used.Contains(Key(pool[i]))) { continue; }
                int w = Weight(pool[i], enc);
                if (roll < w) { return pool[i]; }
                roll -= w;
            }
            return null;
        }

        /// <summary>카드 등장 가중(클수록 흔함). 광=귀함, 피=흔함. 엘리트·보스는 상위 카드 가중↑.</summary>
        public static int Weight(HwatuCardData c, EncounterType enc)
        {
            bool bossOrElite = enc == EncounterType.Boss || enc == EncounterType.Elite;
            switch (c.Kind)
            {
                case HwatuCardKind.Bright:
                    return enc == EncounterType.Boss ? 10 : enc == EncounterType.Elite ? 5 : 2;
                case HwatuCardKind.Animal:
                    return c.IsGodoriBird ? (bossOrElite ? 5 : 3) : 4;
                case HwatuCardKind.Ribbon:
                    return c.Ribbon == RibbonColor.Rain ? 3 : (bossOrElite ? 5 : 4);
                default:   // 피
                    return enc == EncounterType.Boss ? 2 : enc == EncounterType.Elite ? 3 : 6;
            }
        }

        // 월+등급+쌍피 = 중복 방지 키(보상 3택이 서로 다르게).
        private static string Key(HwatuCardData c) => $"{c.Month}-{c.Kind}-{(c.IsDouble ? "D" : "")}";
    }
}

using System;
using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Rng;

namespace Hwatu.Core.Run
{
    /// <summary>
    /// 전투 후 카드 보상(03-card-reward). STS 방식: 밴드 + 피티 오프셋으로 레어도를 정하고,
    /// 그 레어도 풀에서 중복 없이 N장을 뽑는다. 시드 결정론.
    /// </summary>
    public static class RewardSystem
    {
        /// <summary>카드 보상 N장(기본 3). rareOffset은 호출자(RunState)가 보존·전달.</summary>
        public static List<CardData> RollCardReward(IRandom rng, EncounterType enc,
            IReadOnlyList<CardData> pool, ref int rareOffset, int count = 3)
        {
            var result = new List<CardData>();
            var used = new HashSet<string>();
            for (int i = 0; i < count; i++)
            {
                CardRarity rarity = RollRarity(rng, enc, rareOffset);
                rareOffset = NextOffset(rarity, rareOffset);
                CardData card = PickCard(rng, pool, rarity, used);
                if (card != null)
                {
                    result.Add(card);
                    used.Add(card.Id);
                }
            }
            return result;
        }

        /// <summary>레어도 1장 추첨(밴드 + 오프셋). 보스는 Rare 확정.</summary>
        public static CardRarity RollRarity(IRandom rng, EncounterType enc, int rareOffset)
        {
            if (enc == EncounterType.Boss)
            {
                return CardRarity.Rare;
            }
            int baseRare = enc == EncounterType.Elite ? 10 : 3;
            int uncommonBand = enc == EncounterType.Elite ? 40 : 37;
            int rareChance = Math.Max(0, baseRare + rareOffset);
            int roll = rng.NextInt(100);
            if (roll < rareChance)
            {
                return CardRarity.Rare;
            }
            if (roll < rareChance + uncommonBand)
            {
                return CardRarity.Uncommon;
            }
            return CardRarity.Common;
        }

        /// <summary>카드 1장 생성 직후의 오프셋 갱신: Rare면 -5 리셋, 아니면 +1(최대 +40).</summary>
        public static int NextOffset(CardRarity rolled, int rareOffset)
        {
            return rolled == CardRarity.Rare ? -5 : Math.Min(40, rareOffset + 1);
        }

        /// <summary>유물 보상 1개(풀에서 랜덤). 보물/엘리트/보스용.</summary>
        public static RelicData RollRelicReward(IRandom rng, IReadOnlyList<RelicData> pool)
        {
            if (pool == null || pool.Count == 0)
            {
                return null;
            }
            return pool[rng.NextInt(pool.Count)];
        }

        // 해당 레어도 풀에서 중복 없이 1장(없으면 미사용 카드 아무거나 폴백).
        private static CardData PickCard(IRandom rng, IReadOnlyList<CardData> pool, CardRarity rarity, HashSet<string> used)
        {
            var candidates = new List<CardData>();
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].Rarity == rarity && !used.Contains(pool[i].Id))
                {
                    candidates.Add(pool[i]);
                }
            }
            if (candidates.Count == 0)
            {
                for (int i = 0; i < pool.Count; i++)
                {
                    if (!used.Contains(pool[i].Id))
                    {
                        candidates.Add(pool[i]);
                    }
                }
            }
            if (candidates.Count == 0)
            {
                return null;
            }
            return candidates[rng.NextInt(candidates.Count)];
        }
    }
}

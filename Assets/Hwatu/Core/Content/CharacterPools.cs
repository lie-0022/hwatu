using System.Collections.Generic;
using Hwatu.Core.Cards;

namespace Hwatu.Core.Content
{
    /// <summary>캐릭터 id별 보상 카드 풀 선택. RunUI/보상이 Run.Character.Id로 호출.</summary>
    public static class CharacterPools
    {
        public static List<CardData> RewardPool(string characterId)
        {
            switch (characterId)
            {
                case "ink_spirit": return InkCards.RewardPool();
                case "beast": return BeastCards.RewardPool();
                default: return LuminaryCards.RewardPool();
            }
        }
    }
}

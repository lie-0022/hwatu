using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Content;

namespace Hwatu.Core.Run
{
    /// <summary>
    /// 플레이 가능한 캐릭터 정의(시작 HP·골드·덱). STS 캐릭터 차이 4축 중 MVP는 HP·덱을 다루고,
    /// 시작 유물·고유 자원은 후속(SPEC §6).
    /// </summary>
    public sealed class CharacterData
    {
        public string Id { get; }
        public string Name { get; }
        public int StartMaxHp { get; }
        public int StartGold { get; }
        public IReadOnlyList<CardData> StartingDeck { get; }

        public CharacterData(string id, string name, int startMaxHp, int startGold, IReadOnlyList<CardData> startingDeck)
        {
            Id = id;
            Name = name;
            StartMaxHp = startMaxHp;
            StartGold = startGold;
            StartingDeck = startingDeck;
        }

        /// <summary>광객(Luminary): HP 80, 골드 99, 시작 덱 10장(빛타격×5/방패×4/점화×1).</summary>
        public static CharacterData Luminary()
        {
            return new CharacterData("luminary", "광객", 80, 99, StarterContent.LuminaryStarterDeck());
        }

        /// <summary>묵귀(Ink Spirit): HP 70, 골드 99, 시작 덱 10장(그림자칼×5/그늘×4/옻칠×1). 독 특화.</summary>
        public static CharacterData InkSpirit()
        {
            return new CharacterData("ink_spirit", "묵귀", 70, 99, InkCards.InkStarterDeck());
        }
    }
}

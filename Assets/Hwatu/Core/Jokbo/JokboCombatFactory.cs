using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;
using Hwatu.Core.Run;

namespace Hwatu.Core.Jokbo
{
    /// <summary>
    /// 족보 전투 조립 — 기존 CombatFactory를 재사용(적 HP 롤·AI 배선·유물 훅)하고,
    /// 구 카드 더미는 비운 채 화투 덱만 족보 엔진에 주입한다.
    /// </summary>
    public static class JokboCombatFactory
    {
        public static JokboCombatEngine Create(IReadOnlyList<HwatuCardData> deck, IReadOnlyList<EnemyData> enemies,
            ulong masterSeed, int playerMaxHp, int playerHp, IReadOnlyList<RelicData> relics = null, int ascension = 0)
        {
            CombatState state = CombatFactory.CreateCombat(
                new List<CardData>(), enemies, masterSeed, playerMaxHp, playerHp, relics, ascension);
            return new JokboCombatEngine(state, new EffectDispatcher(), deck);
        }

        /// <summary>단일 적 편의 오버로드.</summary>
        public static JokboCombatEngine Create(IReadOnlyList<HwatuCardData> deck, EnemyData enemy,
            ulong masterSeed, int playerMaxHp, int playerHp, IReadOnlyList<RelicData> relics = null, int ascension = 0)
        {
            return Create(deck, new[] { enemy }, masterSeed, playerMaxHp, playerHp, relics, ascension);
        }
    }
}

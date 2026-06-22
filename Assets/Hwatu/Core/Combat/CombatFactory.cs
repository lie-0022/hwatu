using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Core.Combat
{
    /// <summary>
    /// 마일스톤1 전투를 조립하는 팩토리(런타임 와이어링). 마스터 시드 하나로 적 HP 롤·덱 셔플이
    /// 결정론적으로 정해진다. 데이터 정의(<see cref="Content.StarterContent"/>)와 달리 "조립 로직"이라 Combat에 둔다.
    /// </summary>
    public static class CombatFactory
    {
        /// <summary>Luminary(HP 80, 시작덱 10장) vs 잡도깨비 1마리 전투(단독 데모용).</summary>
        public static CombatState CreateLuminaryVsDokkaebi(ulong masterSeed)
        {
            return CreateCombat(StarterContent.LuminaryStarterDeck(), StarterContent.DokkaebiMinion(),
                masterSeed, playerMaxHp: 80, playerHp: 80);
        }

        /// <summary>임의의 덱·적·플레이어 HP로 전투를 조립한다(한 판 루프: RunState가 주입). 시드로 결정론.</summary>
        public static CombatState CreateCombat(IReadOnlyList<CardData> deck, EnemyData enemyData,
            ulong masterSeed, int playerMaxHp, int playerHp, IReadOnlyList<RelicData> relics = null, int ascension = 0)
        {
            var streams = new RngStreams(masterSeed);

            var player = new PlayerState(maxHp: playerMaxHp);
            if (playerHp < playerMaxHp)
            {
                player.SetHp(playerHp);
            }
            if (relics != null)
            {
                for (int i = 0; i < relics.Count; i++)
                {
                    relics[i].ApplyCombatStart(player);
                }
            }

            int hpRange = enemyData.MaxHpMax - enemyData.MaxHpMin + 1;
            int baseHp = enemyData.MaxHpMin + streams.ForStream("enemyHp").NextInt(hpRange);
            int enemyHp = baseHp * AscensionRules.EnemyHpPercent(ascension) / 100;
            IEnemyAi ai = enemyData.AiKind == EnemyAiKind.Phase && enemyData.SecondPhaseOrder != null
                ? new PhaseAi(enemyData, enemyData.AiOrder, enemyData.SecondPhaseOrder)
                : (IEnemyAi)new SequenceAi(enemyData);
            var enemy = new EnemyState(enemyData, enemyHp, ai);

            var state = new CombatState(player, new List<EnemyState> { enemy }, streams.ForStream("combatShuffle"));

            int instanceId = 0;
            foreach (CardData card in deck)
            {
                state.DrawPile.Add(new CardInstance(card, instanceId++));
            }

            return state;
        }
    }
}

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

        /// <summary>임의의 덱·적(1마리)·플레이어 HP로 전투를 조립한다(한 판 루프: RunState가 주입). 다중 오버로드에 위임.</summary>
        public static CombatState CreateCombat(IReadOnlyList<CardData> deck, EnemyData enemyData,
            ulong masterSeed, int playerMaxHp, int playerHp, IReadOnlyList<RelicData> relics = null, int ascension = 0)
        {
            return CreateCombat(deck, new[] { enemyData }, masterSeed, playerMaxHp, playerHp, relics, ascension);
        }

        /// <summary>다중 적 전투 조립(STS식 1~다수 몬스터). 각 적 HP는 같은 enemyHp 스트림에서 순차 롤(결정론).</summary>
        public static CombatState CreateCombat(IReadOnlyList<CardData> deck, IReadOnlyList<EnemyData> enemyDatas,
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

            var hpStream = streams.ForStream("enemyHp");
            var aiStream = streams.ForStream("enemyAi");
            var enemies = new List<EnemyState>();
            for (int i = 0; i < enemyDatas.Count; i++)
            {
                EnemyData ed = enemyDatas[i];
                int hpRange = ed.MaxHpMax - ed.MaxHpMin + 1;
                int baseHp = ed.MaxHpMin + hpStream.NextInt(hpRange);
                int enemyHp = baseHp * AscensionRules.EnemyHpPercent(ascension) / 100;
                IEnemyAi ai;
                if (ed.AiKind == EnemyAiKind.Phase && ed.SecondPhaseOrder != null)
                {
                    ai = new PhaseAi(ed, ed.AiOrder, ed.SecondPhaseOrder);
                }
                else if (ed.AiKind == EnemyAiKind.WeightedRandom)
                {
                    ai = new WeightedAi(ed, aiStream);   // 가중치+연속제한(연구 §4.2)
                }
                else
                {
                    ai = new SequenceAi(ed);
                }
                enemies.Add(new EnemyState(ed, enemyHp, ai));
            }

            var state = new CombatState(player, enemies, streams.ForStream("combatShuffle"));

            int instanceId = 0;
            foreach (CardData card in deck)
            {
                state.DrawPile.Add(new CardInstance(card, instanceId++));
            }

            return state;
        }
    }
}

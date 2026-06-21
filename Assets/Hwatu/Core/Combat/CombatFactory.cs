using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;
using Hwatu.Core.Rng;

namespace Hwatu.Core.Combat
{
    /// <summary>
    /// 마일스톤1 전투를 조립하는 팩토리(런타임 와이어링). 마스터 시드 하나로 적 HP 롤·덱 셔플이
    /// 결정론적으로 정해진다. 데이터 정의(<see cref="Content.StarterContent"/>)와 달리 "조립 로직"이라 Combat에 둔다.
    /// </summary>
    public static class CombatFactory
    {
        /// <summary>Luminary(HP 80, 시작덱 10장) vs 잡도깨비 1마리 전투를 만든다.</summary>
        public static CombatState CreateLuminaryVsDokkaebi(ulong masterSeed)
        {
            var streams = new RngStreams(masterSeed);

            var player = new PlayerState(maxHp: 80);

            var enemyData = StarterContent.DokkaebiMinion();
            int hpRange = enemyData.MaxHpMax - enemyData.MaxHpMin + 1;
            int enemyHp = enemyData.MaxHpMin + streams.ForStream("enemyHp").NextInt(hpRange);
            var enemy = new EnemyState(enemyData, enemyHp, new SequenceAi(enemyData));

            var state = new CombatState(player, new List<EnemyState> { enemy }, streams.ForStream("combatShuffle"));

            int instanceId = 0;
            foreach (CardData card in StarterContent.LuminaryStarterDeck())
            {
                state.DrawPile.Add(new CardInstance(card, instanceId++));
            }

            return state;
        }
    }
}

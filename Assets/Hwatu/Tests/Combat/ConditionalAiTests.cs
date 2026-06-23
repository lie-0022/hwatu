using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;

namespace Hwatu.Tests.Combat
{
    /// <summary>조건부 AI(ConditionalAi) — 플레이어 Block 반응: 높으면 디버프(order[0]), 낮으면 공격(order[1]). 연구 §4.3.</summary>
    public class ConditionalAiTests
    {
        [Test]
        public void HighPlayerBlock_PicksFirstMove_Debuff()
        {
            EnemyData oni = StarterContent.CyclopsOni();   // AiOrder = [glare, enrage, oni_smash]
            var ai = new ConditionalAi(oni, 8);
            var enemy = new EnemyState(oni, 60, ai);
            var player = new PlayerState(80);
            player.SetBlock(12);   // ≥ 임계 8
            Assert.AreEqual(oni.AiOrder[0], ai.PeekNext(enemy, player).Id, "Block 높으면 order[0](디버프)");
        }

        [Test]
        public void LowPlayerBlock_PicksSecondMove()
        {
            EnemyData oni = StarterContent.CyclopsOni();
            var ai = new ConditionalAi(oni, 8);
            var enemy = new EnemyState(oni, 60, ai);
            var player = new PlayerState(80);   // Block 0
            Assert.AreEqual(oni.AiOrder[1], ai.PeekNext(enemy, player).Id, "Block 낮으면 order[1]");
        }

        [Test]
        public void NullPlayer_PicksSecondMove_NoCrash()
        {
            EnemyData oni = StarterContent.CyclopsOni();
            var ai = new ConditionalAi(oni, 8);
            var enemy = new EnemyState(oni, 60, ai);
            Assert.AreEqual(oni.AiOrder[1], ai.PeekNext(enemy, null).Id, "player 없으면 order[1](안전)");
        }
    }
}

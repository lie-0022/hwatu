using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;

namespace Hwatu.Tests.Combat
{
    /// <summary>백호 산군 각성 단계(位階) — 위엄 임계별 패시브(매턴위엄+1·공격+25%·받는피해-25%). 캐릭터 문서 §2.4.</summary>
    public class AwakeningTests
    {
        [Test]
        public void Tier_Thresholds()
        {
            Assert.AreEqual(1, Awakening.Tier(4), "0~4 = 1단계");
            Assert.AreEqual(2, Awakening.Tier(5), "5~9 = 2단계");
            Assert.AreEqual(2, Awakening.Tier(9));
            Assert.AreEqual(3, Awakening.Tier(10), "10~14 = 3단계");
            Assert.AreEqual(4, Awakening.Tier(15), "15+ = 4단계");
        }

        [Test]
        public void Tier3_AttackPlus25Percent()
        {
            // 위엄 10(3단계) + 발톱질5 → (5+위엄10)×1.25 = 18
            var p = new PlayerState(80);
            p.AddStatus(StatusType.Majesty, 10);
            var foe = StarterContent.DokkaebiMinion();
            var e = new EnemyState(foe, 50, new SequenceAi(foe));
            Assert.AreEqual(18, DamageMath.RawDamage(p, e, 5));
        }

        [Test]
        public void Tier4_IncomingDamageMinus25Percent()
        {
            // 위엄 15(4단계) 플레이어가 받는 적 공격 12 → 12×0.75 = 9
            var p = new PlayerState(80);
            p.AddStatus(StatusType.Majesty, 15);
            var foe = StarterContent.DokkaebiMinion();
            var e = new EnemyState(foe, 50, new SequenceAi(foe));
            Assert.AreEqual(9, DamageMath.RawDamage(e, p, 12));
        }

        [Test]
        public void Tier1_NoPassive()
        {
            // 위엄 4(1단계): 공격 강화·피해감소 없음 — (5+4)=9 그대로
            var p = new PlayerState(80);
            p.AddStatus(StatusType.Majesty, 4);
            var foe = StarterContent.DokkaebiMinion();
            var e = new EnemyState(foe, 50, new SequenceAi(foe));
            Assert.AreEqual(9, DamageMath.RawDamage(p, e, 5));
        }
    }
}

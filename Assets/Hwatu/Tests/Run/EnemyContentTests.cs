using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class EnemyContentTests
    {
        [Test]
        public void Boss_FromBossPool()
        {
            var pool = new HashSet<string> { "달그림자 도깨비", "구미호", "장군" };
            for (ulong s = 1; s <= 20; s++)
            {
                Assert.IsTrue(pool.Contains(EnemyContent.PickEnemy(NodeType.Boss, new SplitMix64Random(s)).Name));
            }
        }

        [Test]
        public void Elite_FromElitePool()
        {
            var pool = new HashSet<string> { "광귀", "외눈도깨비", "구렁이" };
            for (ulong s = 1; s <= 20; s++)
            {
                EnemyData e = EnemyContent.PickEnemy(NodeType.Elite, new SplitMix64Random(s));
                Assert.IsTrue(pool.Contains(e.Name), $"seed {s}: {e.Name}");
            }
        }

        [Test]
        public void Combat_FromNormalPool()
        {
            var pool = new HashSet<string> { "잡도깨비", "까마귀떼", "허수아비", "도깨비불", "장승", "그슨대", "멧돼지", "두꺼비", "밤송이도깨비" };
            for (ulong s = 1; s <= 40; s++)
            {
                EnemyData e = EnemyContent.PickEnemy(NodeType.Combat, new SplitMix64Random(s));
                Assert.IsTrue(pool.Contains(e.Name), $"seed {s}: unexpected enemy {e.Name}");
            }
        }

        [Test]
        public void Combat_Act2_HasScaledHp()
        {
            EnemyData a1 = EnemyContent.PickEnemy(NodeType.Combat, new SplitMix64Random(3), 1);
            EnemyData a2 = EnemyContent.PickEnemy(NodeType.Combat, new SplitMix64Random(3), 2);
            Assert.AreEqual(a1.Name, a2.Name, "같은 시드 같은 적");
            Assert.Greater(a2.MaxHpMax, a1.MaxHpMax, "act2는 HP 스케일 적용");
        }

        [Test]
        public void WillOWisp_Scald_AppliesPoison()
        {
            EnemyData wisp = StarterContent.WillOWisp();
            EnemyMoveData scald = wisp.FindMove("scald");
            Assert.IsNotNull(scald);
            Assert.AreEqual(EffectOp.ApplyStatus, scald.Effects[0].Op);
            Assert.AreEqual(StatusType.Poison, scald.Effects[0].Status);
            Assert.AreEqual(5, scald.Effects[0].Amount);
        }

        [Test]
        public void Deterministic_SameSeed()
        {
            Assert.AreEqual(
                EnemyContent.PickEnemy(NodeType.Combat, new SplitMix64Random(7)).Name,
                EnemyContent.PickEnemy(NodeType.Combat, new SplitMix64Random(7)).Name);
        }

        [Test]
        public void Gumiho_Phase2_HasDoomMove()
        {
            EnemyData g = StarterContent.Gumiho();
            EnemyMoveData doom = g.FindMove("doom");
            Assert.IsNotNull(doom);
            Assert.AreEqual(IntentType.Doom, doom.Intent);
            Assert.AreEqual(28, doom.Value);
        }

        [Test]
        public void MapGenerator_AssignsEncounterId_ToCombatNodes()
        {
            // 스테이지 고정: 맵 생성 시 모든 전투 노드에 인카운터 인덱스가 박힌다(-1 아님)
            var graph = new MapGenerator().Generate(new SplitMix64Random(42), 1);
            bool anyCombat = false;
            for (int id = 0; id < 256; id++)
            {
                MapNode n = graph.GetNode(id);
                if (n != null && n.Type == NodeType.Combat && n.OnPath)
                {
                    Assert.GreaterOrEqual(n.EncounterId, 0, $"전투 노드 {id}는 인카운터가 확정돼야 한다");
                    anyCombat = true;
                }
            }
            Assert.IsTrue(anyCombat, "맵에 전투 노드가 존재해야 한다");
        }

        [Test]
        public void BuildEncounter_IsDeterministic()
        {
            // 같은 인카운터 인덱스 → 같은 적 조합(스펙·패턴 고정). 진입마다 달라지지 않는다.
            var a = EnemyContent.BuildEncounter(NodeType.Combat, 4, 1);
            var b = EnemyContent.BuildEncounter(NodeType.Combat, 4, 1);
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].Name, b[i].Name);
            }
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Tests.Run
{
    public class MapGeneratorTests
    {
        private static MapGraph Gen(ulong seed, int act = 1)
        {
            IRandom rng = new SplitMix64Random(seed);
            return new MapGenerator().Generate(rng, act);
        }

        [Test]
        public void Deterministic_SameSeed_SameMap()
        {
            MapGraph a = Gen(12345);
            MapGraph b = Gen(12345);
            Assert.AreEqual(a.Nodes.Count, b.Nodes.Count);
            for (int i = 0; i < a.Nodes.Count; i++)
            {
                Assert.AreEqual(a.Nodes[i].Type, b.Nodes[i].Type, $"node {i} type");
                CollectionAssert.AreEqual(a.Nodes[i].NextIds, b.Nodes[i].NextIds, $"node {i} edges");
            }
        }

        [Test]
        public void AllStartNodes_ReachBoss()
        {
            for (ulong seed = 1; seed <= 30; seed++)
            {
                MapGraph g = Gen(seed);
                List<MapNode> starts = g.StartNodes();
                Assert.IsNotEmpty(starts, $"seed {seed}: no start nodes");
                foreach (MapNode s in starts)
                {
                    Assert.IsTrue(CanReachBoss(g, s), $"seed {seed}: start {s.Id} can't reach boss");
                }
            }
        }

        [Test]
        public void FixedRows_CorrectTypes()
        {
            MapGraph g = Gen(2024);
            foreach (MapNode n in g.Nodes)
            {
                if (!n.OnPath || n.Type == NodeType.Boss) continue;
                if (n.Row == 0) Assert.AreEqual(NodeType.Combat, n.Type, $"row0 node {n.Id}");
                if (n.Row == 8) Assert.AreEqual(NodeType.Treasure, n.Type, $"row8 node {n.Id}");
                if (n.Row == 14) Assert.AreEqual(NodeType.Rest, n.Type, $"row14 node {n.Id}");
            }
        }

        [Test]
        public void Constraints_LowRowsNoRestOrElite()
        {
            for (ulong seed = 1; seed <= 30; seed++)
            {
                MapGraph g = Gen(seed);
                foreach (MapNode n in g.Nodes)
                {
                    if (!n.OnPath) continue;
                    if (n.Row <= 4)
                    {
                        Assert.AreNotEqual(NodeType.Rest, n.Type, $"seed {seed} node {n.Id} (row {n.Row}) is Rest");
                        Assert.AreNotEqual(NodeType.Elite, n.Type, $"seed {seed} node {n.Id} (row {n.Row}) is Elite");
                    }
                    if (n.Row >= 13 && n.Row < 14)
                    {
                        Assert.AreNotEqual(NodeType.Rest, n.Type, $"seed {seed} node {n.Id} (row {n.Row}) is Rest");
                    }
                }
            }
        }

        private static bool CanReachBoss(MapGraph g, MapNode start)
        {
            var stack = new Stack<int>();
            var seen = new HashSet<int>();
            stack.Push(start.Id);
            while (stack.Count > 0)
            {
                int id = stack.Pop();
                if (!seen.Add(id)) continue;
                if (g.Boss != null && id == g.Boss.Id) return true;
                MapNode n = g.GetNode(id);
                if (n == null) continue;
                for (int i = 0; i < n.NextIds.Count; i++)
                {
                    stack.Push(n.NextIds[i]);
                }
            }
            return false;
        }
    }
}

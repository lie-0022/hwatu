using System.Collections.Generic;
using Hwatu.Core.Content;
using Hwatu.Core.Rng;

namespace Hwatu.Core.Run
{
    /// <summary>
    /// STS식 절차적 액트 맵 생성(01-map-generation.md). 폭×높이 그리드에 density개 경로를
    /// 바닥→꼭대기로 긋고(좌우 교차 금지=간선 단조성), 경로가 닿은 노드에 방 타입을 배정한 뒤
    /// 보스로 수렴시킨다. 시드 결정론. (조상충돌 보정은 MVP 생략.)
    /// </summary>
    public sealed class MapGenerator
    {
        public int Width { get; }
        public int Height { get; }
        public int Density { get; }

        // 타입 확률(고정 아닌 노드)
        private const float ShopProb = 0.05f;
        private const float RestProb = 0.12f;
        private const float EliteProb = 0.08f;
        private const float EventProb = 0.22f;
        private const int TreasureRow = 8;

        public MapGenerator(int width = 7, int height = 15, int density = 6)
        {
            Width = width;
            Height = height;
            Density = density;
        }

        /// <summary>맵 1장을 생성한다. act는 난이도/표시용(엘리트 빈도 보정 등에 사용 가능).</summary>
        public MapGraph Generate(IRandom rng, int act)
        {
            // 1) 그리드 노드 생성 (id = row*Width + col)
            var grid = new MapNode[Height, Width];
            var nodes = new List<MapNode>(Height * Width + 1);
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    var n = new MapNode(r * Width + c, r, c);
                    grid[r, c] = n;
                    nodes.Add(n);
                }
            }

            // 2) 경로 긋기 (첫 두 경로의 시작점은 서로 다르게)
            int firstStart = -1;
            for (int i = 0; i < Density; i++)
            {
                int startCol = rng.NextInt(Width);
                if (i == 0)
                {
                    firstStart = startCol;
                }
                else if (i == 1)
                {
                    while (startCol == firstStart)
                    {
                        startCol = rng.NextInt(Width);
                    }
                }
                WalkPath(grid, startCol, rng);
            }

            // 3) 방 타입 배정
            AssignRoomTypes(grid, nodes, rng);

            // 4) 보스 노드 + 최상단(휴식) 행에서 수렴
            var boss = new MapNode(Height * Width, Height, Width / 2) { Type = NodeType.Boss };
            nodes.Add(boss);
            for (int c = 0; c < Width; c++)
            {
                MapNode top = grid[Height - 1, c];
                if (top.OnPath)
                {
                    top.NextIds.Add(boss.Id);
                    boss.PrevIds.Add(top.Id);
                }
            }

            AssignEncounters(nodes, boss, rng);   // 맵 생성 시 각 전투/엘리트/보스 노드의 인카운터 확정(런 동안 고정)
            var graph = new MapGraph(Width, Height, act, nodes) { Boss = boss };
            return graph;
        }

        // 한 가닥 경로: 행0의 startCol에서 위로 한 칸씩, 매 스텝 좌/중/우 택1 + 교차 금지.
        private void WalkPath(MapNode[,] grid, int startCol, IRandom rng)
        {
            int curCol = startCol;
            for (int row = 0; row < Height - 1; row++)
            {
                int nextCol = ChooseNextCol(grid, row, curCol, rng);
                MapNode cur = grid[row, curCol];
                MapNode next = grid[row + 1, nextCol];
                if (!cur.NextIds.Contains(next.Id))
                {
                    cur.NextIds.Add(next.Id);
                    next.PrevIds.Add(cur.Id);
                }
                curCol = nextCol;
            }
        }

        // 좌/중/우(±1, 가장자리는 한쪽) 중 택1 후, 좌우 이웃 간선과 교차하지 않게 클램프.
        private int ChooseNextCol(MapNode[,] grid, int row, int col, IRandom rng)
        {
            int lo = col > 0 ? -1 : 0;
            int hi = col < Width - 1 ? 1 : 0;
            int nextCol = col + lo + rng.NextInt(hi - lo + 1);

            // 교차 금지: 왼쪽 이웃이 그은 최대 목적지보다 왼쪽으로 못 감(단조성 유지)
            if (col > 0)
            {
                int leftMax = MaxNextCol(grid[row, col - 1]);
                if (leftMax >= 0 && nextCol < leftMax)
                {
                    nextCol = leftMax;
                }
            }
            // 오른쪽 이웃이 그은 최소 목적지보다 오른쪽으로 못 감
            if (col < Width - 1)
            {
                int rightMin = MinNextCol(grid[row, col + 1]);
                if (rightMin >= 0 && nextCol > rightMin)
                {
                    nextCol = rightMin;
                }
            }

            if (nextCol < 0) nextCol = 0;
            if (nextCol > Width - 1) nextCol = Width - 1;
            return nextCol;
        }

        private int MaxNextCol(MapNode n)
        {
            int max = -1;
            for (int i = 0; i < n.NextIds.Count; i++)
            {
                int c = n.NextIds[i] % Width;
                if (c > max) max = c;
            }
            return max;
        }

        private int MinNextCol(MapNode n)
        {
            int min = -1;
            for (int i = 0; i < n.NextIds.Count; i++)
            {
                int c = n.NextIds[i] % Width;
                if (min < 0 || c < min) min = c;
            }
            return min;
        }

        // 고정 층(0=전투, 8=보물, 최상단=휴식) + 나머지 확률 배정(제약 준수).
        private void AssignRoomTypes(MapNode[,] grid, List<MapNode> nodes, IRandom rng)
        {
            int lastRow = Height - 1;
            for (int c = 0; c < Width; c++)
            {
                if (grid[0, c].OnPath) grid[0, c].Type = NodeType.Combat;
                if (TreasureRow < Height && grid[TreasureRow, c].OnPath) grid[TreasureRow, c].Type = NodeType.Treasure;
                if (grid[lastRow, c].OnPath) grid[lastRow, c].Type = NodeType.Rest;
            }

            // 배정 가능한(고정 아닌) 경로 노드 수집 — 행 오름차순(부모 먼저 확정되도록)
            var assignable = new List<MapNode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                MapNode n = nodes[i];
                if (!n.OnPath) continue;
                if (n.Row == 0 || n.Row == TreasureRow || n.Row == lastRow) continue;
                assignable.Add(n);
            }

            int count = assignable.Count;
            var pool = new List<NodeType>(count);
            AddType(pool, NodeType.Shop, Round(ShopProb * count));
            AddType(pool, NodeType.Rest, Round(RestProb * count));
            AddType(pool, NodeType.Elite, Round(EliteProb * count));
            AddType(pool, NodeType.Event, Round(EventProb * count));
            while (pool.Count < count) pool.Add(NodeType.Combat);
            rng.Shuffle(pool);

            for (int i = 0; i < assignable.Count; i++)
            {
                MapNode n = assignable[i];
                NodeType chosen = NodeType.Combat;
                for (int k = 0; k < pool.Count; k++)
                {
                    if (CanAssign(grid, n, pool[k]))
                    {
                        chosen = pool[k];
                        pool.RemoveAt(k);
                        break;
                    }
                }
                n.Type = chosen;
            }
        }

        // 배치 제약: 행≤4 휴식/엘리트 금지, 행≥13 휴식 금지, 부모와 같은 특수타입 금지.
        private bool CanAssign(MapNode[,] grid, MapNode n, NodeType t)
        {
            if (n.Row <= 4 && (t == NodeType.Rest || t == NodeType.Elite)) return false;
            if (n.Row >= 13 && t == NodeType.Rest) return false;

            if (t == NodeType.Rest || t == NodeType.Treasure || t == NodeType.Shop || t == NodeType.Elite)
            {
                for (int i = 0; i < n.PrevIds.Count; i++)
                {
                    MapNode parent = NodeByGridId(grid, n.PrevIds[i]);
                    if (parent != null && parent.Type == t) return false;
                }
            }
            return true;
        }

        private MapNode NodeByGridId(MapNode[,] grid, int id)
        {
            int r = id / Width;
            int c = id % Width;
            if (r >= 0 && r < Height && c >= 0 && c < Width) return grid[r, c];
            return null;
        }

        private static void AddType(List<NodeType> pool, NodeType t, int n)
        {
            for (int i = 0; i < n; i++) pool.Add(t);
        }

        private static int Round(float x)
        {
            return (int)(x + 0.5f);
        }

        // 맵 생성 시 전투/엘리트/보스 노드에 인카운터 인덱스를 확정한다(STS식 — 스테이지가 미리 정해짐, 진입 때 재계산 X).
        private void AssignEncounters(List<MapNode> nodes, MapNode boss, IRandom rng)
        {
            int normalCount = EnemyContent.NormalEncounters.Length;
            for (int i = 0; i < nodes.Count; i++)
            {
                MapNode n = nodes[i];
                if (!n.OnPath) { continue; }
                if (n.Type == NodeType.Combat) { n.EncounterId = rng.NextInt(normalCount); }
                else if (n.Type == NodeType.Elite) { n.EncounterId = rng.NextInt(3); }
            }
            boss.EncounterId = rng.NextInt(3);
        }
    }
}

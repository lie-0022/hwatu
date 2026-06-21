using System.Collections.Generic;

namespace Hwatu.Core.Run
{
    /// <summary>한 액트의 맵 그래프(노드 + 간선). 행0 시작 노드들에서 단일 보스로 수렴.</summary>
    public sealed class MapGraph
    {
        public int Width { get; }
        public int Height { get; }
        public int Act { get; }

        /// <summary>그리드 노드 + 보스. (경로 안 닿은 노드도 포함되니 <see cref="MapNode.OnPath"/>로 거른다.)</summary>
        public IReadOnlyList<MapNode> Nodes => _nodes;
        public MapNode Boss { get; internal set; }

        private readonly List<MapNode> _nodes;
        private readonly Dictionary<int, MapNode> _byId;

        public MapGraph(int width, int height, int act, List<MapNode> nodes)
        {
            Width = width;
            Height = height;
            Act = act;
            _nodes = nodes;
            _byId = new Dictionary<int, MapNode>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                _byId[nodes[i].Id] = nodes[i];
            }
        }

        public MapNode GetNode(int id)
        {
            return _byId.TryGetValue(id, out MapNode n) ? n : null;
        }

        /// <summary>행0의 진입 가능한(경로가 닿은) 시작 노드들.</summary>
        public List<MapNode> StartNodes()
        {
            var list = new List<MapNode>();
            for (int i = 0; i < _nodes.Count; i++)
            {
                MapNode n = _nodes[i];
                if (n.Row == 0 && n.OnPath)
                {
                    list.Add(n);
                }
            }
            return list;
        }
    }
}

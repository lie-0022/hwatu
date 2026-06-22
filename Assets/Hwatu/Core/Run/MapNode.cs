using System.Collections.Generic;

namespace Hwatu.Core.Run
{
    /// <summary>맵의 한 노드(방). 그리드 위치 + 타입 + 위/아래로 연결된 노드 id들.</summary>
    public sealed class MapNode
    {
        /// <summary>고유 id. 그리드 노드는 <c>row * width + col</c>로 생성(역산 가능). 보스는 그리드 밖.</summary>
        public int Id { get; }
        public int Row { get; }
        public int Col { get; }
        public NodeType Type { get; internal set; }

        /// <summary>위(다음 층)로 연결된 노드 id.</summary>
        public List<int> NextIds { get; } = new List<int>();
        /// <summary>아래(부모)로 연결된 노드 id.</summary>
        public List<int> PrevIds { get; } = new List<int>();

        public MapNode(int id, int row, int col)
        {
            Id = id;
            Row = row;
            Col = col;
        }

        /// <summary>경로가 닿아 실제로 맵에 존재하는 노드인가(간선이 하나라도 있으면 true).</summary>
        public bool OnPath => NextIds.Count > 0 || PrevIds.Count > 0;
    }
}

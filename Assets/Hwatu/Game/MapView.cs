using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hwatu.Core.Run;

namespace Hwatu.Game
{
    /// <summary>
    /// 맵 화면: RunState.Map의 노드를 그리드로 렌더하고, 진입 가능한 노드(현재 위치의 다음 층)만
    /// 활성화한다. 노드를 누르면 <see cref="GameFlow.EnterNode"/>로 진입. 간선 선은 MVP에서 생략.
    /// </summary>
    public sealed class MapView : MonoBehaviour
    {
        private const float SpacingX = 130f;
        private const float SpacingY = 62f;

        private GameFlow _flow;
        private TMP_FontAsset _font;
        private RectTransform _content;
        private readonly Dictionary<int, Button> _buttons = new Dictionary<int, Button>();

        /// <summary>부모(Canvas) 아래에 맵 컨테이너를 만든다.</summary>
        public void Init(GameFlow flow, TMP_FontAsset font, RectTransform parent)
        {
            _flow = flow;
            _font = font;

            var go = new GameObject("MapContent", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            _content = (RectTransform)go.transform;
            _content.anchorMin = Vector2.zero;
            _content.anchorMax = Vector2.one;
            _content.offsetMin = Vector2.zero;
            _content.offsetMax = Vector2.zero;
        }

        /// <summary>현재 RunState.Map을 렌더(맵 진입 시 1회).</summary>
        public void Build()
        {
            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                Destroy(_content.GetChild(i).gameObject);
            }
            _buttons.Clear();

            MapGraph map = _flow.Run.Map;
            if (map == null)
            {
                return;
            }

            float midCol = (map.Width - 1) / 2f;
            IReadOnlyList<MapNode> nodes = map.Nodes;

            // 노드 위치 계산
            var pos = new Dictionary<int, Vector2>();
            for (int i = 0; i < nodes.Count; i++)
            {
                MapNode n = nodes[i];
                if (!n.OnPath)
                {
                    continue;
                }
                pos[n.Id] = new Vector2((n.Col - midCol) * SpacingX, 50f + n.Row * SpacingY);
            }

            // 간선(노드보다 먼저 그려 뒤에 깔림)
            for (int i = 0; i < nodes.Count; i++)
            {
                MapNode n = nodes[i];
                if (!pos.ContainsKey(n.Id))
                {
                    continue;
                }
                for (int k = 0; k < n.NextIds.Count; k++)
                {
                    if (pos.TryGetValue(n.NextIds[k], out Vector2 to))
                    {
                        DrawEdge(pos[n.Id], to);
                    }
                }
            }

            // 노드 버튼
            for (int i = 0; i < nodes.Count; i++)
            {
                MapNode n = nodes[i];
                if (pos.TryGetValue(n.Id, out Vector2 p))
                {
                    CreateNodeButton(n, p.x, p.y);
                }
            }
            Refresh();
        }

        /// <summary>진입 가능한 노드만 interactable(현재 노드의 다음 층, 또는 행0 시작).</summary>
        public void Refresh()
        {
            MapGraph map = _flow.Run.Map;
            if (map == null)
            {
                return;
            }
            HashSet<int> reachable = ReachableNodeIds(map);
            foreach (KeyValuePair<int, Button> kv in _buttons)
            {
                kv.Value.interactable = reachable.Contains(kv.Key);
            }
        }

        private HashSet<int> ReachableNodeIds(MapGraph map)
        {
            var set = new HashSet<int>();
            if (_flow.Run.CurrentNodeId < 0)
            {
                List<MapNode> starts = map.StartNodes();
                for (int i = 0; i < starts.Count; i++)
                {
                    set.Add(starts[i].Id);
                }
            }
            else
            {
                MapNode cur = map.GetNode(_flow.Run.CurrentNodeId);
                if (cur != null)
                {
                    for (int i = 0; i < cur.NextIds.Count; i++)
                    {
                        set.Add(cur.NextIds[i]);
                    }
                }
            }
            return set;
        }

        private void DrawEdge(Vector2 from, Vector2 to)
        {
            var go = new GameObject("Edge", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_content, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = (from + to) * 0.5f;
            rt.sizeDelta = new Vector2(Vector2.Distance(from, to), 4f);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.40f, 0.40f, 0.45f, 0.55f);
            img.raycastTarget = false;
        }

        private void CreateNodeButton(MapNode n, float x, float y)
        {
            var go = new GameObject("Node" + n.Id, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_content, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(56f, 46f);
            go.GetComponent<Image>().color = NodeColor(n.Type);
            go.AddComponent<TooltipTrigger>().Set(GameInfo.NodeDesc(n.Type));

            var label = new GameObject("L", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(rt, false);
            var t = label.GetComponent<TextMeshProUGUI>();
            t.font = _font;
            t.fontSize = 18f;
            t.enableAutoSizing = true;   // "엘리트" 등 긴 라벨을 노드(56px) 안에 맞게 축소
            t.fontSizeMin = 11f;
            t.fontSizeMax = 18f;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            t.text = NodeLabel(n.Type);
            t.raycastTarget = false;
            var lrt = t.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            var btn = go.GetComponent<Button>();
            int id = n.Id;
            btn.onClick.AddListener(() => OnNodeClicked(id));
            _buttons[n.Id] = btn;
        }

        private void OnNodeClicked(int id)
        {
            MapNode n = _flow.Run.Map.GetNode(id);
            if (n != null)
            {
                _flow.EnterNode(n);
            }
        }

        private static Color NodeColor(NodeType t)
        {
            switch (t)
            {
                case NodeType.Combat: return new Color(0.40f, 0.40f, 0.45f);
                case NodeType.Elite: return new Color(0.70f, 0.25f, 0.22f);
                case NodeType.Rest: return new Color(0.25f, 0.50f, 0.30f);
                case NodeType.Shop: return new Color(0.30f, 0.45f, 0.70f);
                case NodeType.Treasure: return new Color(0.70f, 0.60f, 0.20f);
                case NodeType.Event: return new Color(0.45f, 0.40f, 0.60f);
                case NodeType.Boss: return new Color(0.82f, 0.15f, 0.15f);
                default: return Color.gray;
            }
        }

        private static string NodeLabel(NodeType t)
        {
            switch (t)
            {
                case NodeType.Combat: return "전투";
                case NodeType.Elite: return "정예";
                case NodeType.Rest: return "휴식";
                case NodeType.Shop: return "상점";
                case NodeType.Treasure: return "보물";
                case NodeType.Event: return "?";
                case NodeType.Boss: return "보스";
                default: return "";
            }
        }
    }
}

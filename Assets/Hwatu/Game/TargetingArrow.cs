using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.Game
{
    /// <summary>
    /// 타깃 카드를 드래그할 때 카드 상단 → 커서로 뻗는 점선 곡선 화살표(STS식). 베지어 곡선 위에
    /// 작은 점들을 배치하고 끝에 화살촉을 둔다. 화면 좌표를 받아 Canvas 로컬로 변환해 갱신한다.
    /// </summary>
    public sealed class TargetingArrow : MonoBehaviour
    {
        private const int DotCount = 14;

        private RectTransform _canvas;
        private Image[] _dots;
        private Image _head;

        /// <summary>점/화살촉을 생성하고 Canvas 전체에 스트레치한다(좌표 변환 기준).</summary>
        public void Build(RectTransform canvas)
        {
            _canvas = canvas;

            var rt = (RectTransform)transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _dots = new Image[DotCount];
            for (int i = 0; i < DotCount; i++)
            {
                float size = 9f + i * 1.3f;   // 끝으로 갈수록 굵게
                _dots[i] = CreateDot("Dot" + i, size, new Color(0.96f, 0.86f, 0.32f, 0.9f));
            }
            _head = CreateDot("Head", 30f, new Color(0.98f, 0.52f, 0.24f, 0.96f));

            SetVisible(false);
        }

        /// <summary>화살표 표시/숨김.</summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>카드 상단(시작)→커서(끝) 화면 좌표로 곡선을 갱신한다.</summary>
        public void UpdateArc(Vector2 fromScreen, Vector2 toScreen)
        {
            Vector2 from = ToLocal(fromScreen);
            Vector2 to = ToLocal(toScreen);

            // 위로 볼록한 제어점(거리에 비례)
            float lift = Vector2.Distance(from, to) * 0.3f;
            Vector2 ctrl = (from + to) * 0.5f + new Vector2(0f, lift);

            for (int i = 0; i < DotCount; i++)
            {
                float t = (i + 1f) / (DotCount + 1f);
                _dots[i].rectTransform.anchoredPosition = Bezier(from, ctrl, to, t);
            }

            _head.rectTransform.anchoredPosition = to;
            Vector2 tangent = to - Bezier(from, ctrl, to, 0.88f);
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            _head.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        private Vector2 ToLocal(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas, screen, null, out Vector2 local);
            return local;
        }

        private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private Image CreateDot(string name, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace Hwatu.Game
{
    /// <summary>
    /// 마우스 hover 시 설명을 띄우는 공통 툴팁. 최상위 Canvas(sortingOrder 200)에 단일 인스턴스.
    /// <see cref="TooltipTrigger"/>가 Show/Hide를 호출한다. STS2식 "거의 완전 정보" — 보유물·상태를 hover로 확인.
    /// </summary>
    public sealed class TooltipUI : MonoBehaviour
    {
        public static TooltipUI Instance { get; private set; }

        private GameObject _box;
        private TextMeshProUGUI _text;
        private RectTransform _boxRt;

        /// <summary>툴팁 박스를 생성한다(폰트 주입). 씬당 1회.</summary>
        public void Build(TMP_FontAsset font)
        {
            Instance = this;

            var canvasGo = new GameObject("TooltipCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;   // 모든 UI 위
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            _box.transform.SetParent(canvasGo.transform, false);
            _boxRt = (RectTransform)_box.transform;
            _boxRt.pivot = new Vector2(0f, 1f);   // 좌상단 기준 → 마우스 우하단으로 펼침
            var bg = _box.GetComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.07f, 0.96f);
            bg.raycastTarget = false;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(_box.transform, false);
            _text = textGo.GetComponent<TextMeshProUGUI>();
            _text.font = font;
            _text.fontSize = 26f;
            _text.color = Color.white;
            _text.raycastTarget = false;
            _text.textWrappingMode = TextWrappingModes.Normal;
            _text.richText = true;
            var trt = _text.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(14f, 10f);
            trt.offsetMax = new Vector2(-14f, -10f);

            _box.SetActive(false);
        }

        private void Update()
        {
            if (_box == null || !_box.activeSelf)
            {
                return;
            }
            Vector2 mp = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            // 화면 우/하단 넘침 보정
            float w = _boxRt.sizeDelta.x;
            float h = _boxRt.sizeDelta.y;
            float x = mp.x + 18f;
            float y = mp.y - 18f;
            if (x + w > Screen.width) { x = mp.x - 18f - w; }
            if (y - h < 0f) { y = h + 18f; }
            _boxRt.position = new Vector2(x, y);
        }

        /// <summary>설명 문자열을 띄운다(빈 문자열이면 무시).</summary>
        public static void Show(string text)
        {
            if (Instance == null || string.IsNullOrEmpty(text))
            {
                return;
            }
            Instance._text.text = text;
            Vector2 pref = Instance._text.GetPreferredValues(text, 380f, 0f);
            float w = Mathf.Min(pref.x + 28f, 420f);
            float h = pref.y + 22f;
            Instance._boxRt.sizeDelta = new Vector2(w, h);
            Instance._box.SetActive(true);
            Instance._box.transform.SetAsLastSibling();
        }

        /// <summary>툴팁을 숨긴다.</summary>
        public static void Hide()
        {
            if (Instance != null && Instance._box != null)
            {
                Instance._box.SetActive(false);
            }
        }
    }
}

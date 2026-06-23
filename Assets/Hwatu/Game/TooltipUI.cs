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
        private Canvas _canvas;

        /// <summary>툴팁 박스를 생성한다(폰트 주입). 씬당 1회.</summary>
        public void Build(TMP_FontAsset font)
        {
            Instance = this;

            var canvasGo = new GameObject("TooltipCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 200;   // 모든 UI 위
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
            Reposition();
        }

        // 마우스 옆에 띄우되 화면 밖으로 넘치지 않게 보정. CanvasScaler(ScaleWithScreenSize)에서
        // sizeDelta는 1920 기준 로컬 단위라, 실제 픽셀과 비교하려면 scaleFactor를 곱해야 한다(넘침 보정 정확).
        private void Reposition()
        {
            Vector2 mp = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            float sf = _canvas != null ? _canvas.scaleFactor : 1f;
            float w = _boxRt.sizeDelta.x * sf;
            float h = _boxRt.sizeDelta.y * sf;
            float x = mp.x + 18f;
            if (x + w > Screen.width) { x = mp.x - 18f - w; }   // 우측 넘침 → 마우스 왼쪽으로
            if (x < 4f) { x = 4f; }
            // 기본은 마우스 '위쪽'으로 펼친다 — 상태칩이 패널 하단(플레이어 HUD)에 있어도 가리거나 잘리지 않게.
            // pivot이 좌상단이라 position.y는 박스 상단(top), 하단은 top-h. 박스 하단을 마우스 위 18px에 둔다.
            float y = mp.y + 18f + h;
            if (y > Screen.height - 4f) { y = mp.y - 18f; }     // 위 공간 부족 → 마우스 아래로 펼침
            if (y - h < 4f) { y = h + 4f; }                     // 그래도 넘치면 화면 안에 고정
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
            Instance.Reposition();   // 첫 프레임부터 올바른 위치(깜빡임 방지)
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

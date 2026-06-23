using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Hwatu.Game
{
    /// <summary>체력 바(배경 + 채움 + 숫자). 채움 폭은 SmoothDamp로 부드럽게 변한다(숫자는 즉시).</summary>
    public sealed class HpBar : MonoBehaviour
    {
        private const float SmoothTime = 0.25f;

        private float _width;
        private float _curWidth;
        private float _targetWidth;
        private float _widthVelocity;
        private Image _fill;
        private TextMeshProUGUI _text;

        /// <summary>배경·채움·숫자를 생성한다.</summary>
        public void Build(TMP_FontAsset font, Color fillColor, float width, float height)
        {
            _width = width;
            _curWidth = width;
            _targetWidth = width;

            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(width, height);

            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);
            bg.raycastTarget = false;

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(transform, false);
            _fill = fillGo.GetComponent<Image>();
            _fill.color = fillColor;
            _fill.raycastTarget = false;
            var frt = _fill.rectTransform;
            frt.anchorMin = new Vector2(0f, 0f);
            frt.anchorMax = new Vector2(0f, 1f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.anchoredPosition = Vector2.zero;
            frt.sizeDelta = new Vector2(width, 0f);

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(transform, false);
            _text = txtGo.GetComponent<TextMeshProUGUI>();
            _text.font = font;
            _text.fontSize = height * 0.66f;
            _text.alignment = TextAlignmentOptions.Center;
            _text.color = Color.white;
            _text.raycastTarget = false;
            var trt = _text.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            // HP 아이콘(game-icons — 바 왼쪽 바깥, 바 색과 동일 틴트)
            var icoGo = new GameObject("HpIcon", typeof(RectTransform), typeof(Image));
            icoGo.transform.SetParent(transform, false);
            var icoRt = (RectTransform)icoGo.transform;
            icoRt.anchorMin = new Vector2(0f, 0.5f);
            icoRt.anchorMax = new Vector2(0f, 0.5f);
            icoRt.pivot = new Vector2(1f, 0.5f);
            icoRt.anchoredPosition = new Vector2(-6f, 0f);
            var ico = icoGo.GetComponent<Image>();
            ico.raycastTarget = false;
            IconLoader.Apply(this, ico, IconCatalog.Hp, fillColor, height + 6f);
        }

        /// <summary>현재/최대 체력을 반영한다(숫자는 즉시, 바는 Update에서 부드럽게).</summary>
        public void Set(int hp, int maxHp)
        {
            float ratio = maxHp > 0 ? Mathf.Clamp01((float)hp / maxHp) : 0f;
            _targetWidth = _width * ratio;
            _text.text = $"{hp} / {maxHp}";
        }

        private void Update()
        {
            if (Mathf.Abs(_curWidth - _targetWidth) > 0.3f)
            {
                _curWidth = Mathf.SmoothDamp(_curWidth, _targetWidth, ref _widthVelocity, SmoothTime);
                _fill.rectTransform.sizeDelta = new Vector2(_curWidth, 0f);
            }
        }
    }
}

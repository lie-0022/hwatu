using UnityEngine;
using TMPro;

namespace Hwatu.Game
{
    /// <summary>피해 숫자가 위로 떠오르며 페이드아웃한 뒤 사라지는 팝업.</summary>
    public sealed class DamagePopup : MonoBehaviour
    {
        private const float Duration = 0.9f;
        private const float RiseSpeed = 90f;

        private TextMeshProUGUI _text;
        private float _life;

        /// <summary>숫자·색을 설정한다(이후 Update가 자동 연출·소멸).</summary>
        public void Show(TMP_FontAsset font, int amount, Color color)
        {
            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(160f, 60f);

            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            _text = go.GetComponent<TextMeshProUGUI>();
            _text.font = font;
            _text.fontSize = 46f;
            _text.fontStyle = FontStyles.Bold;
            _text.alignment = TextAlignmentOptions.Center;
            _text.color = color;
            _text.text = amount.ToString();
            _text.raycastTarget = false;
            var trt = _text.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
        }

        private void Update()
        {
            _life += Time.deltaTime;
            transform.position += new Vector3(0f, RiseSpeed * Time.deltaTime, 0f);

            if (_text != null)
            {
                Color c = _text.color;
                c.a = Mathf.Clamp01(1f - _life / Duration);
                _text.color = c;
            }

            if (_life >= Duration)
            {
                Destroy(gameObject);
            }
        }
    }
}

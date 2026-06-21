using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hwatu.Core.Combat;
using Hwatu.Core.Enemies;

namespace Hwatu.Game
{
    /// <summary>
    /// 적 1마리의 영역/정보(박스 + 이름·HP·방어·다음 행동). 공격 카드의 드롭 타깃이며,
    /// 드래그로 조준되면 <see cref="SetHighlight"/>로 테두리(Outline)가 켜진다.
    /// </summary>
    public sealed class EnemyView : MonoBehaviour
    {
        private TMP_FontAsset _font;
        private Image _background;
        private Outline _outline;
        private TextMeshProUGUI _text;

        /// <summary>적 배열에서의 인덱스(PlayCard 타깃 지정용).</summary>
        public int Index { get; private set; }

        /// <summary>박스·테두리·텍스트를 생성한다.</summary>
        public void Build(TMP_FontAsset font, int index)
        {
            _font = font;
            Index = index;

            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(380f, 150f);

            _background = gameObject.AddComponent<Image>();
            _background.color = new Color(0.22f, 0.13f, 0.13f, 0.9f);

            _outline = gameObject.AddComponent<Outline>();
            _outline.effectColor = new Color(1f, 0.85f, 0.3f, 1f);
            _outline.effectDistance = new Vector2(4f, 4f);
            _outline.enabled = false;

            _text = CreateText("Info", 26f);
        }

        /// <summary>적 상태를 박스에 반영한다.</summary>
        public void Bind(EnemyState e)
        {
            string intent = e.CurrentIntent != null
                ? $"{IntentKor(e.CurrentIntent.Intent)} {e.CurrentIntent.Value}"
                : "?";
            _text.text = $"{e.Data.Name}\nHP {e.Hp}/{e.MaxHp}    방어 {e.Block}\n다음 행동: {intent}";
        }

        /// <summary>조준 테두리 on/off.</summary>
        public void SetHighlight(bool on)
        {
            _outline.enabled = on;
        }

        private TextMeshProUGUI CreateText(string name, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.raycastTarget = false;
            var rt = t.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return t;
        }

        private static string IntentKor(IntentType intent)
        {
            switch (intent)
            {
                case IntentType.Attack: return "공격";
                case IntentType.AttackMulti: return "연속공격";
                case IntentType.Block: return "방어";
                case IntentType.Buff: return "강화";
                case IntentType.Debuff: return "약화";
                default: return intent.ToString();
            }
        }
    }
}

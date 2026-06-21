using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hwatu.Core.Combat;
using Hwatu.Core.Enemies;

namespace Hwatu.Game
{
    /// <summary>
    /// 적 1마리의 영역(박스 + 이름 + HP 바 + intent 아이콘). 공격 카드의 드롭 타깃이며,
    /// 드래그로 조준되면 <see cref="SetHighlight"/>로 테두리(Outline)가 켜진다.
    /// intent는 타입별 색 박스 + 라벨 + 예상 수치로 표시한다(아이콘 에셋 도입 전 대체).
    /// </summary>
    public sealed class EnemyView : MonoBehaviour
    {
        private TMP_FontAsset _font;
        private Image _background;
        private Outline _outline;
        private TextMeshProUGUI _nameText;
        private HpBar _hpBar;
        private Image _intentBg;
        private TextMeshProUGUI _intentText;

        /// <summary>적 배열에서의 인덱스(PlayCard 타깃 지정용).</summary>
        public int Index { get; private set; }

        /// <summary>박스·테두리·이름·HP 바·intent 아이콘을 생성한다.</summary>
        public void Build(TMP_FontAsset font, int index)
        {
            _font = font;
            Index = index;

            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(400f, 170f);

            _background = gameObject.AddComponent<Image>();
            _background.color = new Color(0.22f, 0.13f, 0.13f, 0.9f);

            _outline = gameObject.AddComponent<Outline>();
            _outline.effectColor = new Color(1f, 0.85f, 0.3f, 1f);
            _outline.effectDistance = new Vector2(4f, 4f);
            _outline.enabled = false;

            _nameText = CreateText(transform, "Name", 26f, new Vector2(0.5f, 0.85f), new Vector2(360f, 34f));
            _nameText.fontStyle = FontStyles.Bold;

            var barGo = new GameObject("HpBar", typeof(RectTransform));
            barGo.transform.SetParent(transform, false);
            var brt = (RectTransform)barGo.transform;
            brt.anchorMin = new Vector2(0.5f, 0.56f);
            brt.anchorMax = new Vector2(0.5f, 0.56f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = Vector2.zero;
            _hpBar = barGo.AddComponent<HpBar>();
            _hpBar.Build(font, new Color(0.8f, 0.22f, 0.2f, 1f), 340f, 28f);

            // intent 아이콘(타입별 색 박스 + 라벨 + 수치)
            var intentGo = new GameObject("Intent", typeof(RectTransform), typeof(Image));
            intentGo.transform.SetParent(transform, false);
            var irt = (RectTransform)intentGo.transform;
            irt.anchorMin = new Vector2(0.5f, 0.18f);
            irt.anchorMax = new Vector2(0.5f, 0.18f);
            irt.pivot = new Vector2(0.5f, 0.5f);
            irt.anchoredPosition = Vector2.zero;
            irt.sizeDelta = new Vector2(220f, 48f);
            _intentBg = intentGo.GetComponent<Image>();
            _intentBg.raycastTarget = false;
            _intentText = CreateText(irt, "IntentText", 24f, new Vector2(0.5f, 0.5f), new Vector2(212f, 44f));
        }

        /// <summary>적 상태를 박스에 반영한다.</summary>
        public void Bind(EnemyState e)
        {
            _nameText.text = e.Data.Name;
            _hpBar.Set(e.Hp, e.MaxHp);

            if (e.CurrentIntent != null)
            {
                _intentBg.enabled = true;
                _intentBg.color = IntentColor(e.CurrentIntent.Intent);
                _intentText.text = $"{IntentKor(e.CurrentIntent.Intent)}  <b><size=135%>{e.CurrentIntent.Value}</size></b>";
            }
            else
            {
                _intentBg.enabled = false;
                _intentText.text = "?";
            }
        }

        /// <summary>조준 테두리 on/off.</summary>
        public void SetHighlight(bool on)
        {
            _outline.enabled = on;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, float size, Vector2 anchor, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.richText = true;
            t.raycastTarget = false;
            var rt = t.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = sizeDelta;
            return t;
        }

        private static Color IntentColor(IntentType intent)
        {
            switch (intent)
            {
                case IntentType.Attack:
                case IntentType.AttackMulti: return new Color(0.70f, 0.20f, 0.18f, 0.95f);  // 공격: 빨강
                case IntentType.Block:       return new Color(0.20f, 0.40f, 0.70f, 0.95f);  // 방어: 파랑
                case IntentType.Buff:        return new Color(0.25f, 0.55f, 0.30f, 0.95f);  // 강화: 초록
                case IntentType.Debuff:      return new Color(0.50f, 0.30f, 0.62f, 0.95f);  // 약화: 보라
                default:                     return new Color(0.35f, 0.35f, 0.35f, 0.95f);  // 기타: 회색
            }
        }

        private static string IntentKor(IntentType intent)
        {
            switch (intent)
            {
                case IntentType.Attack: return "공격";
                case IntentType.AttackMulti: return "연속";
                case IntentType.Block: return "방어";
                case IntentType.Buff: return "강화";
                case IntentType.Debuff: return "약화";
                case IntentType.Summon: return "소환";
                case IntentType.Doom: return "파멸";
                default: return "?";
            }
        }
    }
}

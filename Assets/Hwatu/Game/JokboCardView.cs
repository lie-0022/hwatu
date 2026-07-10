using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Hwatu.Core.Jokbo;

namespace Hwatu.Game
{
    /// <summary>
    /// 족보 전투의 손패 카드 1장(코드 생성 uGUI) — 클릭으로 선택 토글, 선택 시 위로 떠오름.
    /// 배경색=등급(광 금·열끗 녹·띠 색·피 회), 상단 월/등급, 중앙 공/방 수치.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class JokboCardView : MonoBehaviour, IPointerClickHandler
    {
        public const float Width = 128f;
        public const float Height = 176f;

        private TMP_FontAsset _font;
        private Image _background;
        private Outline _outline;
        private TextMeshProUGUI _topText;
        private TextMeshProUGUI _valueText;
        private Vector2 _homePos;
        private bool _selected;

        /// <summary>이 뷰가 든 카드 인스턴스.</summary>
        public JokboCardInstance Card { get; private set; }
        public bool Selected => _selected;

        /// <summary>클릭 시 콜백(자신 전달) — 뷰가 선택 상태를 컨트롤러/뷰에 알린다.</summary>
        public event Action<JokboCardView> OnClicked;

        public void Build(TMP_FontAsset font)
        {
            _font = font;
            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(Width, Height);

            _background = gameObject.AddComponent<Image>();
            _outline = gameObject.AddComponent<Outline>();
            _outline.effectColor = new Color(1f, 0.85f, 0.3f, 1f);
            _outline.effectDistance = new Vector2(3f, 3f);
            _outline.enabled = false;

            _topText = CreateText("Top", new Vector2(0.5f, 0.86f), 20f, new Vector2(Width - 12f, 30f));
            _topText.fontStyle = FontStyles.Bold;
            _valueText = CreateText("Value", new Vector2(0.5f, 0.34f), 22f, new Vector2(Width - 12f, 60f));
        }

        public void Bind(JokboCardInstance card)
        {
            Card = card;
            HwatuCardData d = card.Data;
            _background.color = KindColor(d);
            _topText.text = $"{d.Month}월";
            _topText.color = TextColorFor(d);

            int atk = JokboRules.AttackOf(d);
            int def = JokboRules.DefenseOf(d);
            string body = KindKor(d);
            if (atk > 0) { body += $"\n<size=120%><b>{atk}</b></size> 공격"; }
            if (def > 0) { body += $"\n<size=120%><b>{def}</b></size> 방어"; }
            _valueText.text = body;
            _valueText.color = TextColorFor(d);
        }

        /// <summary>선택 상태 지정(위로 떠오름 + 금색 테두리).</summary>
        public void SetSelected(bool on)
        {
            _selected = on;
            _outline.enabled = on;
            var rt = (RectTransform)transform;
            rt.anchoredPosition = on ? _homePos + new Vector2(0f, 40f) : _homePos;
        }

        /// <summary>손패 기준 위치 지정(선택 해제 상태로 복귀 기준).</summary>
        public void SetHome(Vector2 pos)
        {
            _homePos = pos;
            var rt = (RectTransform)transform;
            rt.anchoredPosition = _selected ? _homePos + new Vector2(0f, 40f) : _homePos;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke(this);
        }

        // ── 색·라벨 ──

        private static Color KindColor(HwatuCardData d)
        {
            switch (d.Kind)
            {
                case HwatuCardKind.Bright: return new Color(0.86f, 0.74f, 0.30f);   // 금
                case HwatuCardKind.Animal: return new Color(0.30f, 0.55f, 0.44f);   // 녹
                case HwatuCardKind.Ribbon:
                    switch (d.Ribbon)
                    {
                        case RibbonColor.Red:  return new Color(0.70f, 0.28f, 0.30f);
                        case RibbonColor.Blue: return new Color(0.28f, 0.42f, 0.68f);
                        default:               return new Color(0.42f, 0.55f, 0.32f);
                    }
                default: return new Color(0.34f, 0.34f, 0.38f);   // 피 회
            }
        }

        private static Color TextColorFor(HwatuCardData d)
        {
            return d.Kind == HwatuCardKind.Bright ? new Color(0.15f, 0.10f, 0.02f) : new Color(0.96f, 0.96f, 0.92f);
        }

        private static string KindKor(HwatuCardData d)
        {
            switch (d.Kind)
            {
                case HwatuCardKind.Bright: return d.IsRainBright ? "비광" : "광";
                case HwatuCardKind.Animal: return "열끗";
                case HwatuCardKind.Ribbon:
                    switch (d.Ribbon)
                    {
                        case RibbonColor.Red:   return "홍단";
                        case RibbonColor.Blue:  return "청단";
                        case RibbonColor.Plain: return "초단";
                        default:                return "비띠";
                    }
                default: return d.IsDouble ? "쌍피" : "피";
            }
        }

        private TextMeshProUGUI CreateText(string name, Vector2 anchor, float size, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.richText = true;
            t.raycastTarget = false;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = sizeDelta;
            return t;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.Game
{
    /// <summary>
    /// HUD 미니칩(반투명 배경 + game-icons 아이콘 + 우측 숫자) 공용 생성기 — 디자인 일관성 SSOT.
    /// 전투/맵/상점 등 어느 화면에서도 같은 크기·정렬·아이콘 규격으로 칩을 만든다(들쑥날쑥 방지).
    /// </summary>
    public static class UiChips
    {
        /// <summary>미니칩 생성 후 숫자 라벨(TextMeshProUGUI)을 반환한다(.text 갱신용).</summary>
        public static TextMeshProUGUI MakeHudChip(MonoBehaviour host, Transform parent, TMP_FontAsset font,
            string iconName, Color color, Vector2 anchoredPos, float width = 96f, float height = 34f)
        {
            var go = new GameObject("HudChip", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(width, height);
            go.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.22f);

            var icoGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icoGo.transform.SetParent(rt, false);
            var icoRt = (RectTransform)icoGo.transform;
            icoRt.anchorMin = new Vector2(0f, 0.5f);
            icoRt.anchorMax = new Vector2(0f, 0.5f);
            icoRt.pivot = new Vector2(0f, 0.5f);
            icoRt.anchoredPosition = new Vector2(5f, 0f);
            var ico = icoGo.GetComponent<Image>();
            ico.raycastTarget = false;
            IconLoader.Apply(host, ico, iconName, color, DesignTokens.IconSm);

            var lblGo = new GameObject("L", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(rt, false);
            var lbl = lblGo.GetComponent<TextMeshProUGUI>();
            lbl.font = font;
            lbl.fontSize = 20f;
            lbl.enableAutoSizing = true;
            lbl.fontSizeMin = 12f;
            lbl.fontSizeMax = 20f;
            lbl.alignment = TextAlignmentOptions.Right;
            lbl.color = Color.white;
            lbl.raycastTarget = false;
            lbl.margin = new Vector4(0f, 0f, 8f, 0f);
            var lrt = lbl.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            return lbl;
        }
    }
}

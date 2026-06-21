using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;
using Hwatu.Core.Enemies;

namespace Hwatu.Game
{
    /// <summary>
    /// 최소 정식 전투 UI(uGUI, 코드 생성). STS 레이아웃(SPEC 부록 C): 적 상단 / 플레이어 좌하단 /
    /// 손패 하단 / 턴버튼 우하단 / 결과 오버레이. <see cref="CombatController"/> 상태를 읽어 갱신한다.
    /// </summary>
    [RequireComponent(typeof(CombatController))]
    public sealed class CombatView : MonoBehaviour
    {
        private CombatController _controller;
        private TMP_FontAsset _font;

        private TextMeshProUGUI _enemyText;
        private TextMeshProUGUI _playerText;
        private RectTransform _handArea;
        private GameObject _resultPanel;
        private TextMeshProUGUI _resultText;

        private void Start()
        {
            _controller = GetComponent<CombatController>();
            _font = LoadKoreanFont();
            BuildUI();
            Refresh();
        }

        private static TMP_FontAsset LoadKoreanFont()
        {
            Font f = Resources.Load<Font>("Fonts/malgun");
            if (f == null)
            {
                f = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Gulim", "Arial" }, 24);
            }
            return TMP_FontAsset.CreateFontAsset(f);
        }

        private void BuildUI()
        {
            EnsureEventSystem();

            var canvasGo = new GameObject("CombatCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            RectTransform root = canvasGo.GetComponent<RectTransform>();

            // 적(상단 중앙)
            _enemyText = CreateText(root, "EnemyText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -90), 30, TextAlignmentOptions.Top);
            _enemyText.rectTransform.sizeDelta = new Vector2(1300, 160);

            // 플레이어(좌하단)
            _playerText = CreateText(root, "PlayerText", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(380, 250), 26, TextAlignmentOptions.BottomLeft);
            _playerText.rectTransform.sizeDelta = new Vector2(720, 150);

            // 손패(하단 중앙, 가로 배치)
            var handGo = new GameObject("HandArea", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            handGo.transform.SetParent(root, false);
            _handArea = handGo.GetComponent<RectTransform>();
            _handArea.anchorMin = new Vector2(0.5f, 0f);
            _handArea.anchorMax = new Vector2(0.5f, 0f);
            _handArea.pivot = new Vector2(0.5f, 0f);
            _handArea.anchoredPosition = new Vector2(0, 30);
            _handArea.sizeDelta = new Vector2(1500, 190);
            var hlg = handGo.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.LowerCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // 턴 종료(우하단)
            CreateButton(root, "턴 종료", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-130, 250), new Vector2(170, 60),
                () => { _controller.EndTurn(); Refresh(); });

            // 결과 오버레이
            _resultPanel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
            _resultPanel.transform.SetParent(root, false);
            var rp = _resultPanel.GetComponent<RectTransform>();
            rp.anchorMin = Vector2.zero;
            rp.anchorMax = Vector2.one;
            rp.offsetMin = Vector2.zero;
            rp.offsetMax = Vector2.zero;
            _resultPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            _resultText = CreateText(rp, "ResultText", new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero, 56, TextAlignmentOptions.Center);
            _resultText.rectTransform.sizeDelta = new Vector2(700, 120);
            CreateButton(rp, "다시 시작", new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(220, 66),
                () => { _controller.NewCombat(); Refresh(); });
            _resultPanel.SetActive(false);
        }

        private void Refresh()
        {
            CombatState s = _controller.State;
            if (s == null)
            {
                return;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < s.Enemies.Count; i++)
            {
                EnemyState e = s.Enemies[i];
                string intent = e.CurrentIntent != null
                    ? $"{IntentKor(e.CurrentIntent.Intent)} {e.CurrentIntent.Value}"
                    : "?";
                sb.AppendLine($"{e.Data.Name}    HP {e.Hp}/{e.MaxHp}    방어 {e.Block}    다음 행동: {intent}");
            }
            _enemyText.text = sb.ToString();

            PlayerState p = s.Player;
            _playerText.text =
                $"나    HP {p.Hp}/{p.MaxHp}    방어 {p.Block}\n" +
                $"에너지 {p.Energy}/{p.BaseEnergy}    광 {p.GetStatus(StatusType.Radiance)}\n" +
                $"턴 {s.Turn}    덱 {s.DrawPile.Count}    버린 더미 {s.DiscardPile.Count}";

            RebuildHand(s);

            if (_controller.Result != CombatResult.InProgress)
            {
                _resultPanel.SetActive(true);
                _resultPanel.transform.SetAsLastSibling();
                _resultText.text = _controller.Result == CombatResult.Win ? "승리!" : "패배...";
            }
            else
            {
                _resultPanel.SetActive(false);
            }
        }

        private void RebuildHand(CombatState s)
        {
            for (int i = _handArea.childCount - 1; i >= 0; i--)
            {
                Destroy(_handArea.GetChild(i).gameObject);
            }
            for (int i = 0; i < s.Hand.Count; i++)
            {
                CardInstance c = s.Hand[i];
                int idx = i;
                string label = $"[{c.Data.Cost}]\n{c.Data.Name}\n\n{Describe(c)}";
                Button btn = CreateCardButton(_handArea, label);
                btn.interactable = s.Player.Energy >= c.Data.Cost;
                btn.onClick.AddListener(() => { if (_controller.PlayCard(idx)) { Refresh(); } });
            }
        }

        // uGUI 입력 처리에 필요한 EventSystem이 없으면 만든다(New Input System 모듈).
        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        // ── UI 생성 헬퍼 ──

        private TextMeshProUGUI CreateText(RectTransform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, float size, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = align;
            t.color = Color.white;
            t.enableWordWrapping = false;
            var rt = t.rectTransform;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(400, 100);
            return t;
        }

        private Button CreateButton(RectTransform parent, string label, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Action onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.24f, 0.28f, 0.36f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var txt = CreateText(rt, "Label", Vector2.zero, Vector2.one, Vector2.zero, 22, TextAlignmentOptions.Center);
            txt.rectTransform.sizeDelta = Vector2.zero;
            txt.text = label;
            return go.GetComponent<Button>();
        }

        private Button CreateCardButton(RectTransform parent, string label)
        {
            var go = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 160;
            le.preferredHeight = 180;
            go.GetComponent<Image>().color = new Color(0.30f, 0.34f, 0.42f);

            var txt = CreateText(go.GetComponent<RectTransform>(), "Label", Vector2.zero, Vector2.one, Vector2.zero, 20, TextAlignmentOptions.Center);
            txt.rectTransform.sizeDelta = Vector2.zero;
            txt.enableWordWrapping = true;
            txt.text = label;
            return go.GetComponent<Button>();
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

        private static string Describe(CardInstance c)
        {
            if (c.Data.Effects.Count == 0)
            {
                return "-";
            }
            EffectData e = c.Data.Effects[0];
            switch (e.Op)
            {
                case EffectOp.DealDamage: return $"{e.Amount} 피해";
                case EffectOp.GainBlock: return $"{e.Amount} 방어";
                case EffectOp.GainResource: return $"광 +{e.Amount}";
                default: return e.Op;
            }
        }
    }
}

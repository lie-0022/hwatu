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

        private EnemyView _enemyView;
        private TextMeshProUGUI _playerText;
        private Outline _playerOutline;
        private HpBar _playerHpBar;
        private RectTransform _root;
        private int _prevEnemyHp;
        private int _prevPlayerHp;
        private RectTransform _handArea;
        private GameObject _resultPanel;
        private TextMeshProUGUI _resultText;
        private TargetingArrow _arrow;

        /// <summary>타깃 카드 드래그 시 쓰는 조준 화살표.</summary>
        public TargetingArrow Arrow => _arrow;

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
            _root = root;

            // 적 영역(상단 중앙) — 박스(드롭 타깃 + 조준 테두리)
            var enemyGo = new GameObject("EnemyView", typeof(RectTransform));
            enemyGo.transform.SetParent(root, false);
            var ert = (RectTransform)enemyGo.transform;
            ert.anchorMin = new Vector2(0.5f, 1f);
            ert.anchorMax = new Vector2(0.5f, 1f);
            ert.pivot = new Vector2(0.5f, 1f);
            ert.anchoredPosition = new Vector2(0f, -70f);
            _enemyView = enemyGo.AddComponent<EnemyView>();
            _enemyView.Build(_font, 0);

            // 플레이어 영역(좌하단) — 박스 + 테두리(방어 카드 드래그 시 강조)
            var playerGo = new GameObject("PlayerView", typeof(RectTransform), typeof(Image));
            playerGo.transform.SetParent(root, false);
            var prt = (RectTransform)playerGo.transform;
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.zero;
            prt.pivot = Vector2.zero;
            prt.anchoredPosition = new Vector2(24f, 24f);
            prt.sizeDelta = new Vector2(440f, 150f);
            playerGo.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.20f, 0.85f);
            _playerOutline = playerGo.AddComponent<Outline>();
            _playerOutline.effectColor = new Color(0.4f, 0.8f, 1f, 1f);
            _playerOutline.effectDistance = new Vector2(4f, 4f);
            _playerOutline.enabled = false;
            var pbarGo = new GameObject("PlayerHpBar", typeof(RectTransform));
            pbarGo.transform.SetParent(prt, false);
            var pbrt = (RectTransform)pbarGo.transform;
            pbrt.anchorMin = new Vector2(0.5f, 1f);
            pbrt.anchorMax = new Vector2(0.5f, 1f);
            pbrt.pivot = new Vector2(0.5f, 1f);
            pbrt.anchoredPosition = new Vector2(0f, -12f);
            _playerHpBar = pbarGo.AddComponent<HpBar>();
            _playerHpBar.Build(_font, new Color(0.3f, 0.75f, 0.35f, 1f), 400f, 28f);

            _playerText = CreateText(prt, "PlayerText", Vector2.zero, Vector2.one, Vector2.zero, 22, TextAlignmentOptions.Center);
            _playerText.rectTransform.offsetMin = new Vector2(16f, 12f);
            _playerText.rectTransform.offsetMax = new Vector2(-16f, -52f);

            // 손패(하단 중앙, 가로 배치)
            var handGo = new GameObject("HandArea", typeof(RectTransform));
            handGo.transform.SetParent(root, false);
            _handArea = handGo.GetComponent<RectTransform>();
            _handArea.anchorMin = new Vector2(0.5f, 0f);
            _handArea.anchorMax = new Vector2(0.5f, 0f);
            _handArea.pivot = new Vector2(0.5f, 0f);
            _handArea.anchoredPosition = new Vector2(0, 30);
            _handArea.sizeDelta = new Vector2(1500, 190);

            // 턴 종료(우하단)
            CreateButton(root, "턴 종료", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-130, 250), new Vector2(170, 60),
                () => { _controller.EndTurn(); Refresh(); });

            // 타깃팅 화살표(손패 위, 결과 오버레이 아래)
            var arrowGo = new GameObject("TargetingArrow", typeof(RectTransform));
            arrowGo.transform.SetParent(root, false);
            _arrow = arrowGo.AddComponent<TargetingArrow>();
            _arrow.Build(root);

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

            // HP 감소량만큼 피해 팝업(엔진 수정 없이 UI에서 감지)
            int curEnemyHp = s.Enemies.Count > 0 ? s.Enemies[0].Hp : 0;
            int curPlayerHp = s.Player.Hp;
            int enemyDamage = _prevEnemyHp - curEnemyHp;
            int playerDamage = _prevPlayerHp - curPlayerHp;
            if (enemyDamage > 0 && _enemyView != null)
            {
                ShowDamagePopup(_enemyView.transform.position, enemyDamage, new Color(1f, 0.95f, 0.45f));
            }
            if (playerDamage > 0 && _playerOutline != null)
            {
                ShowDamagePopup(_playerOutline.transform.position + new Vector3(0f, 80f, 0f), playerDamage, new Color(1f, 0.45f, 0.4f));
            }
            _prevEnemyHp = curEnemyHp;
            _prevPlayerHp = curPlayerHp;

            if (s.Enemies.Count > 0)
            {
                _enemyView.Bind(s.Enemies[0]);
            }

            PlayerState p = s.Player;
            _playerHpBar.Set(p.Hp, p.MaxHp);
            _playerText.text =
                $"에너지 {p.Energy}/{p.BaseEnergy}    광 {p.GetStatus(StatusType.Radiance)}\n" +
                $"방어 {p.Block}    턴 {s.Turn}    덱 {s.DrawPile.Count}    버린 {s.DiscardPile.Count}";

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

        /// <summary>드래그로 놓은 카드를 사용 시도한다(enemyIndex&lt;0 = 비타깃). 성공 시 손패 갱신.</summary>
        public bool TryPlayCard(CardView card, int enemyIndex)
        {
            CombatState s = _controller.State;
            if (s == null || card == null)
            {
                return false;
            }
            int idx = -1;
            for (int i = 0; i < s.Hand.Count; i++)
            {
                if (s.Hand[i] == card.Card)
                {
                    idx = i;
                    break;
                }
            }
            if (idx < 0)
            {
                return false;
            }
            bool ok = _controller.PlayCard(idx, enemyIndex < 0 ? 0 : enemyIndex);
            if (ok)
            {
                Refresh();
            }
            return ok;
        }

        /// <summary>적 박스 조준 테두리(단일 적: 인자가 그 적이면 켜고, null이면 끈다).</summary>
        public void SetEnemyHighlight(EnemyView enemy)
        {
            if (_enemyView != null)
            {
                _enemyView.SetHighlight(enemy == _enemyView);
            }
        }

        /// <summary>플레이어 영역 테두리 on/off(방어 카드 드래그 시).</summary>
        public void SetPlayerHighlight(bool on)
        {
            if (_playerOutline != null)
            {
                _playerOutline.enabled = on;
            }
        }

        // 피해 숫자 팝업을 대상 위치에 띄운다.
        private void ShowDamagePopup(Vector3 worldPos, int amount, Color color)
        {
            var go = new GameObject("DamagePopup", typeof(RectTransform));
            go.transform.SetParent(_root, false);
            go.transform.position = worldPos;
            go.AddComponent<DamagePopup>().Show(_font, amount, color);
        }

        private void RebuildHand(CombatState s)
        {
            for (int i = _handArea.childCount - 1; i >= 0; i--)
            {
                Transform child = _handArea.GetChild(i);
                child.SetParent(null, false);   // 즉시 분리 → 아래 LayoutHand의 childCount가 새 카드만 세도록
                Destroy(child.gameObject);
            }
            for (int i = 0; i < s.Hand.Count; i++)
            {
                CardInstance c = s.Hand[i];
                int idx = i;
                bool playable = s.Player.Energy >= c.Data.Cost;

                var go = new GameObject("Card", typeof(RectTransform));
                go.transform.SetParent(_handArea, false);
                var card = go.AddComponent<CardView>();
                card.Build(_font);
                card.SetCombat(this);
                card.Bind(c, playable);

                var btn = go.AddComponent<Button>();
                btn.targetGraphic = go.GetComponent<Image>();
                btn.interactable = playable;
                btn.onClick.AddListener(() => { if (_controller.PlayCard(idx)) { Refresh(); } });
            }

            LayoutHand();
        }

        // 손패 카드를 부채꼴(아래 손잡이를 중심으로 한 원호)로 배치하고 각 카드의 기준 위치·회전을 지정한다.
        private void LayoutHand()
        {
            int n = _handArea.childCount;
            const float anglePerCard = 7f;   // 카드 사이 벌어지는 각도(도)
            const float radius = 1400f;       // 원호 반경(클수록 평평)
            float mid = (n - 1) / 2f;
            for (int i = 0; i < n; i++)
            {
                float offset = i - mid;
                float angle = offset * anglePerCard;
                float rad = angle * Mathf.Deg2Rad;
                float x = Mathf.Sin(rad) * radius;
                float y = (Mathf.Cos(rad) - 1f) * radius;   // 중앙 0, 양끝 아래로(위로 볼록한 아치)
                var card = _handArea.GetChild(i).GetComponent<CardView>();
                if (card != null)
                {
                    card.SetHome(new Vector2(x, y), -angle);
                }
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
            t.textWrappingMode = TextWrappingModes.NoWrap;
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

    }
}

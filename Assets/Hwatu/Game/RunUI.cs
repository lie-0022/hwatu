using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;
using Hwatu.Core.Run;

namespace Hwatu.Game
{
    /// <summary>
    /// 한 판 루프 화면 조율(06-screens-ui). GameFlow 페이즈에 따라 메인/캐릭터/맵/보상/결과 패널과
    /// 전투 화면을 토글한다. 전투는 CombatController/CombatView GO를 동적 생성해 코루틴으로 시작하고,
    /// 종료 콜백에서 RunState.Hp를 갱신한 뒤 GameFlow로 되돌린다.
    /// </summary>
    [RequireComponent(typeof(GameFlow))]
    public sealed class RunUI : MonoBehaviour
    {
        private GameFlow _flow;
        private TMP_FontAsset _font;

        private GameObject _menuPanel;
        private GameObject _charPanel;
        private GameObject _mapPanel;
        private GameObject _rewardPanel;
        private GameObject _resultPanel;
        private TextMeshProUGUI _resultText;

        private MapView _mapView;
        private GameObject _combatGo;
        private CombatController _combatCtrl;
        private CombatView _combatView;

        private void Start()
        {
            _flow = GetComponent<GameFlow>();
            _font = CombatView.LoadKoreanFont();
            EnsureEventSystem();
            BuildUI();
            _flow.OnPhaseChanged += OnPhaseChanged;
            OnPhaseChanged(_flow.Phase);
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("RunCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            var root = canvasGo.GetComponent<RectTransform>();

            // 메인 메뉴
            _menuPanel = CreatePanel(root, "MenuPanel", new Color(0.08f, 0.06f, 0.10f, 1f));
            CreateText(_menuPanel, "화투", 88f, new Vector2(0, 140));
            CreateButton(_menuPanel, "시작", new Vector2(0, -40), () => _flow.GoToCharacterSelect());

            // 캐릭터 선택
            _charPanel = CreatePanel(root, "CharPanel", new Color(0.08f, 0.08f, 0.12f, 1f));
            CreateText(_charPanel, "캐릭터 선택", 56f, new Vector2(0, 170));
            CreateButton(_charPanel, "광객 (光客)", new Vector2(0, 0), () => _flow.StartNewRun(CharacterData.Luminary()));

            // 맵
            _mapPanel = CreatePanel(root, "MapPanel", new Color(0.06f, 0.07f, 0.10f, 1f));
            _mapView = _mapPanel.AddComponent<MapView>();
            _mapView.Init(_flow, _font, _mapPanel.GetComponent<RectTransform>());

            // 보상(MVP stub)
            _rewardPanel = CreatePanel(root, "RewardPanel", new Color(0.09f, 0.09f, 0.06f, 0.97f));
            CreateText(_rewardPanel, "전투 승리!  (카드 보상은 다음 단계)", 40f, new Vector2(0, 80));
            CreateButton(_rewardPanel, "계속", new Vector2(0, -50), () => _flow.OnRewardDone());

            // 결과
            _resultPanel = CreatePanel(root, "ResultPanel", new Color(0f, 0f, 0f, 0.9f));
            _resultText = CreateText(_resultPanel, "", 72f, new Vector2(0, 0));

            // 전투 GO(자체 Canvas, 초기 비활성)
            _combatGo = new GameObject("RunCombat", typeof(CombatController), typeof(CombatView));
            _combatCtrl = _combatGo.GetComponent<CombatController>();
            _combatView = _combatGo.GetComponent<CombatView>();
            _combatCtrl.OnCombatEnded += HandleCombatEnded;
            _combatGo.SetActive(false);
        }

        private void OnPhaseChanged(RunPhase p)
        {
            _menuPanel.SetActive(p == RunPhase.MainMenu);
            _charPanel.SetActive(p == RunPhase.CharacterSelect);
            _mapPanel.SetActive(p == RunPhase.Map);
            _rewardPanel.SetActive(p == RunPhase.Reward);
            _resultPanel.SetActive(p == RunPhase.GameOver || p == RunPhase.Victory);
            _combatGo.SetActive(p == RunPhase.Combat);

            if (p == RunPhase.Map)
            {
                _mapView.Build();
            }
            else if (p == RunPhase.Combat)
            {
                StartCoroutine(StartCombatNextFrame());
            }
            else if (p == RunPhase.GameOver)
            {
                _resultText.text = "패배...";
            }
            else if (p == RunPhase.Victory)
            {
                _resultText.text = "승리!";
            }
        }

        // CombatView.Start(BuildUI)가 끝난 다음 프레임에 런 덱으로 전투 시작.
        private IEnumerator StartCombatNextFrame()
        {
            yield return null;
            EnemyData enemy = StarterContent.DokkaebiMinion();   // MVP: 모든 전투 잡도깨비(보스 적은 후속)
            ulong combatSeed = _flow.Run.Seed + (ulong)(_flow.Run.CurrentNodeId + 1);
            _combatCtrl.StartCombat(_flow.Run.Deck, enemy, combatSeed, _flow.Run.MaxHp, _flow.Run.Hp);
            _combatView.SetVisible(true);
            _combatView.Refresh();
        }

        private void HandleCombatEnded(bool won)
        {
            CombatState s = _combatCtrl.State;
            if (s != null)
            {
                _flow.Run.Hp = s.Player.Hp;
            }
            _combatView.SetVisible(false);
            _flow.OnCombatEnded(won);
        }

        // ── UI 헬퍼 ──

        private GameObject CreatePanel(RectTransform parent, string name, Color bg)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = bg;
            return go;
        }

        private TextMeshProUGUI CreateText(GameObject panel, string text, float size, Vector2 pos)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(panel.transform, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            t.text = text;
            t.raycastTarget = false;
            var rt = t.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(1200f, size * 1.6f);
            return t;
        }

        private Button CreateButton(GameObject panel, string label, Vector2 pos, Action onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(320f, 84f);
            go.GetComponent<Image>().color = new Color(0.25f, 0.30f, 0.42f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var t = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            t.transform.SetParent(rt, false);
            var tmp = t.GetComponent<TextMeshProUGUI>();
            tmp.font = _font;
            tmp.fontSize = 30f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.text = label;
            tmp.raycastTarget = false;
            var lrt = tmp.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            return go.GetComponent<Button>();
        }

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
    }
}

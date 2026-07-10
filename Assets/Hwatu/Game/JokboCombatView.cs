using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hwatu.Core.Combat;
using Hwatu.Core.Enemies;
using Hwatu.Core.Jokbo;

namespace Hwatu.Game
{
    /// <summary>
    /// 족보 전투 UI(코드 생성 uGUI) — STS식 좌우 대치 + 손패 다중선택 + 족보 미리보기 + 내기/버리기.
    /// <see cref="JokboCombatController"/>를 읽어 그리고 입력을 전달한다. 기존 전투 UI(CombatView)와 병행.
    /// </summary>
    [RequireComponent(typeof(JokboCombatController))]
    public sealed class JokboCombatView : MonoBehaviour
    {
        private const int MaxEnemies = 3;

        private JokboCombatController _ctrl;
        private TMP_FontAsset _font;
        private RectTransform _root;

        private readonly List<EnemyView> _enemyViews = new List<EnemyView>();
        private HpBar _playerHpBar;
        private TextMeshProUGUI _playerText;
        private RectTransform _handArea;
        private readonly List<JokboCardView> _cardViews = new List<JokboCardView>();
        private readonly List<int> _selected = new List<int>();   // 선택된 손패 인덱스(순서 = 선택 순)

        private TextMeshProUGUI _preview;
        private Button _playBtn;
        private TextMeshProUGUI _playLabel;
        private Button _discardBtn;
        private TextMeshProUGUI _discardLabel;
        private GameObject _resultPanel;
        private TextMeshProUGUI _resultText;

        private int _targetEnemy;

        private void Start()
        {
            _ctrl = GetComponent<JokboCombatController>();
            _font = CombatView.LoadKoreanFont();
            BuildUI();
            _ctrl.OnCombatEnded += OnEnded;
            if (_ctrl.State == null)
            {
                _ctrl.StartCombat(HwatuDeckContent.CommonStarterDeck(),
                    Hwatu.Core.Content.StarterContent.DokkaebiMinion(), 12345UL, 80, 80);
            }
            Refresh();
        }

        // ── 빌드 ──

        private void BuildUI()
        {
            var canvasGo = new GameObject("JokboCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            _root = canvasGo.GetComponent<RectTransform>();

            EnsureEventSystem();

            // 적 슬롯(우측 중앙, 최대 3)
            for (int i = 0; i < MaxEnemies; i++)
            {
                var go = new GameObject($"EnemyView_{i}", typeof(RectTransform));
                go.transform.SetParent(_root, false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(-540f, 40f);
                var ev = go.AddComponent<EnemyView>();
                ev.Build(_font, i);
                go.SetActive(false);
                _enemyViews.Add(ev);
            }

            // 플레이어(좌측 중앙)
            var pgo = new GameObject("PlayerView", typeof(RectTransform), typeof(Image));
            pgo.transform.SetParent(_root, false);
            var prt = (RectTransform)pgo.transform;
            prt.anchorMin = prt.anchorMax = new Vector2(0f, 0.5f);
            prt.pivot = new Vector2(0f, 0.5f);
            prt.anchoredPosition = new Vector2(60f, 20f);
            prt.sizeDelta = new Vector2(460f, 210f);
            pgo.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.20f, 0.85f);

            var barGo = new GameObject("PlayerHpBar", typeof(RectTransform));
            barGo.transform.SetParent(prt, false);
            var brt = (RectTransform)barGo.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 1f);
            brt.pivot = new Vector2(0.5f, 1f);
            brt.anchoredPosition = new Vector2(0f, -12f);
            _playerHpBar = barGo.AddComponent<HpBar>();
            _playerHpBar.Build(_font, new Color(0.3f, 0.75f, 0.35f, 1f), 400f, 28f);

            _playerText = MakeText(prt, "PlayerText", new Vector2(0.5f, 0.4f), 22f, new Vector2(430f, 120f));

            // 손패(하단 중앙)
            var handGo = new GameObject("HandArea", typeof(RectTransform));
            handGo.transform.SetParent(_root, false);
            _handArea = (RectTransform)handGo.transform;
            _handArea.anchorMin = _handArea.anchorMax = new Vector2(0.5f, 0f);
            _handArea.pivot = new Vector2(0.5f, 0f);
            _handArea.anchoredPosition = new Vector2(0f, 40f);
            _handArea.sizeDelta = new Vector2(1500f, 200f);

            // 족보 미리보기(상단 중앙)
            _preview = MakeText(_root, "Preview", new Vector2(0.5f, 1f), 30f, new Vector2(900f, 60f));
            _preview.rectTransform.anchoredPosition = new Vector2(0f, -60f);

            // 버튼: 내기 / 버리기 / 턴 종료
            _playBtn = MakeButton("내기", new Vector2(0.5f, 0f), new Vector2(-140f, 270f), new Vector2(200f, 64f), OnPlay, out _playLabel);
            _discardBtn = MakeButton("버리기", new Vector2(0.5f, 0f), new Vector2(90f, 270f), new Vector2(200f, 64f), OnDiscard, out _discardLabel);
            MakeButton("턴 종료", new Vector2(1f, 0f), new Vector2(-130f, 250f), new Vector2(170f, 60f), OnEndTurn, out _);

            // 결과 오버레이
            _resultPanel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
            _resultPanel.transform.SetParent(_root, false);
            var rrt = (RectTransform)_resultPanel.transform;
            rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
            rrt.offsetMin = Vector2.zero; rrt.offsetMax = Vector2.zero;
            _resultPanel.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.9f);
            _resultText = MakeText((RectTransform)_resultPanel.transform, "ResultText", new Vector2(0.5f, 0.6f), 60f, new Vector2(1000f, 200f));
            MakeButton("다시", new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(240f, 70f), Restart, out _)
                .transform.SetParent(_resultPanel.transform, false);
            _resultPanel.SetActive(false);
        }

        // ── 갱신 ──

        private void Refresh()
        {
            if (_ctrl.State == null) { return; }
            CombatState s = _ctrl.State;

            // 적 배치(우측 대치, 수에 따라 축소)
            int active = 0;
            for (int i = 0; i < s.Enemies.Count && i < MaxEnemies; i++)
            {
                if (!s.Enemies[i].IsDead) { active++; }
            }
            int slot = 0;
            for (int i = 0; i < MaxEnemies; i++)
            {
                bool alive = i < s.Enemies.Count && !s.Enemies[i].IsDead;
                _enemyViews[i].gameObject.SetActive(alive);
                if (!alive) { continue; }
                float scale = active <= 1 ? 0.92f : active == 2 ? 0.84f : 0.74f;
                float spacing = 400f * scale + 64f;
                float x = -560f + (slot - (active - 1) / 2f) * spacing;
                var rt = (RectTransform)_enemyViews[i].transform;
                rt.localScale = new Vector3(scale, scale, 1f);
                rt.anchoredPosition = new Vector2(x, 20f);
                _enemyViews[i].Bind(s.Enemies[i], s.Player);
                slot++;
            }
            if (_targetEnemy >= s.Enemies.Count || (_targetEnemy < s.Enemies.Count && s.Enemies[_targetEnemy].IsDead))
            {
                _targetEnemy = FirstAlive(s);
            }

            // 플레이어 HUD
            PlayerState p = s.Player;
            _playerHpBar.Set(p.Hp, p.MaxHp);
            string block = p.Block > 0 ? $"    <color=#5B9BD5><b>방어 {p.Block}</b></color>" : "";
            _playerText.text =
                $"<size=130%><color=#E0B84A><b>내기 {_ctrl.Engine.PlaysLeft}/{JokboRules.PlaysPerTurn}   버리기 {_ctrl.Engine.DiscardsLeft}/{JokboRules.DiscardsPerTurn}</b></color></size>{block}\n" +
                $"<size=78%>턴 {s.Turn}   ·   덱 {_ctrl.Engine.DrawPile.Count} / 버린 {_ctrl.Engine.DiscardPile.Count}</size>";

            RebuildHand();
            UpdateSelectionUi();
        }

        private void RebuildHand()
        {
            foreach (JokboCardView cv in _cardViews)
            {
                if (cv != null) { Destroy(cv.gameObject); }
            }
            _cardViews.Clear();
            _selected.Clear();

            List<JokboCardInstance> hand = _ctrl.Engine.Hand;
            int n = hand.Count;
            float spacing = 138f;
            float startX = -(n - 1) / 2f * spacing;
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject("Card", typeof(RectTransform));
                go.transform.SetParent(_handArea, false);
                var cv = go.AddComponent<JokboCardView>();
                cv.Build(_font);
                cv.Bind(hand[i]);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                cv.SetHome(new Vector2(startX + i * spacing, 10f));
                int idx = i;
                cv.OnClicked += _ => ToggleSelect(idx, cv);
                _cardViews.Add(cv);
            }
        }

        private void ToggleSelect(int handIndex, JokboCardView cv)
        {
            if (_selected.Contains(handIndex))
            {
                _selected.Remove(handIndex);
                cv.SetSelected(false);
            }
            else
            {
                if (_selected.Count >= JokboRules.MaxSubmitCards) { return; }
                _selected.Add(handIndex);
                cv.SetSelected(true);
            }
            UpdateSelectionUi();
        }

        // 선택 상태 → 족보 미리보기 + 버튼 활성.
        private void UpdateSelectionUi()
        {
            var cards = new List<HwatuCardData>(_selected.Count);
            foreach (int i in _selected)
            {
                if (i >= 0 && i < _ctrl.Engine.Hand.Count) { cards.Add(_ctrl.Engine.Hand[i].Data); }
            }
            JokboType jokbo = JokboDetector.Detect(cards);
            bool playable = jokbo != JokboType.None && _ctrl.Engine.PlaysLeft > 0;

            if (cards.Count == 0)
            {
                _preview.text = "<color=#888888>카드를 골라 족보를 맞추세요</color>";
            }
            else if (jokbo == JokboType.None)
            {
                _preview.text = "<color=#B06060>족보 없음</color>";
            }
            else
            {
                int num = JokboRules.MultNum(jokbo), den = JokboRules.MultDen(jokbo);
                int atk = JokboRules.AttackSum(cards) * num / den;
                int def = JokboRules.DefenseSum(cards) * num / den;
                string aoe = JokboRules.IsAoe(jokbo) ? " <size=70%>(전체)</size>" : "";
                _preview.text = $"<color=#E0B84A><b>{JokboRules.DisplayName(jokbo)}</b></color>  —  피해 {atk} · 방어 {def}{aoe}";
            }

            _playBtn.interactable = playable;
            _playLabel.text = $"내기 ({cards.Count})";
            _discardBtn.interactable = _selected.Count > 0 && _ctrl.Engine.DiscardsLeft > 0;
            _discardLabel.text = $"버리기 ({_selected.Count})";
        }

        // ── 입력 ──

        private void OnPlay()
        {
            if (_selected.Count == 0) { return; }
            var indices = new List<int>(_selected);
            if (_ctrl.PlayJokbo(indices, _targetEnemy)) { Refresh(); }
        }

        private void OnDiscard()
        {
            if (_selected.Count == 0) { return; }
            var indices = new List<int>(_selected);
            if (_ctrl.Discard(indices)) { Refresh(); }
        }

        private void OnEndTurn()
        {
            _ctrl.EndTurn();
            Refresh();
        }

        private void OnEnded(bool win)
        {
            _resultText.text = win ? "<color=#E0B84A>승리!</color>" : "<color=#C05050>패배...</color>";
            _resultPanel.SetActive(true);
            _resultPanel.transform.SetAsLastSibling();
        }

        private void Restart()
        {
            _resultPanel.SetActive(false);
            _ctrl.StartCombat(HwatuDeckContent.CommonStarterDeck(),
                Hwatu.Core.Content.StarterContent.DokkaebiMinion(), 12345UL, 80, 80);
            Refresh();
        }

        // ── 헬퍼 ──

        private static int FirstAlive(CombatState s)
        {
            for (int i = 0; i < s.Enemies.Count; i++)
            {
                if (!s.Enemies[i].IsDead) { return i; }
            }
            return 0;
        }

        private TextMeshProUGUI MakeText(RectTransform parent, string name, Vector2 anchor, float size, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            t.richText = true;
            t.raycastTarget = false;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = sizeDelta;
            return t;
        }

        private Button MakeButton(string label, Vector2 anchor, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick, out TextMeshProUGUI labelText)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_root, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.20f, 0.22f, 0.30f, 0.95f);
            go.GetComponent<Button>().onClick.AddListener(onClick);
            labelText = MakeText(rt, "L", new Vector2(0.5f, 0.5f), 24f, size);
            return go.GetComponent<Button>();
        }

        private void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
                es.transform.SetParent(transform, false);
            }
        }
    }
}

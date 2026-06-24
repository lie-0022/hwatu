using System;
using System.Text;
using System.Collections.Generic;
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
        private Canvas _canvas;

        /// <summary>최대 3마리 적 뷰(미리 생성, 슬롯 단위 재사용).</summary>
        private readonly List<EnemyView> _enemyViews = new List<EnemyView>();
        private int[] _prevEnemyHps = System.Array.Empty<int>();
        private const int MaxEnemies = 3;
        /// <summary>적 박스 폭(400) + 여백(40) = 슬롯 간격.</summary>
        private const float EnemySpacing = 440f;
        private TextMeshProUGUI _playerText;
        private Outline _playerOutline;
        private HpBar _playerHpBar;
        private RectTransform _root;
        private int _prevPlayerHp;
        private readonly Dictionary<int, CardView> _cardViews = new Dictionary<int, CardView>();
        private RectTransform _handArea;
        private RectTransform _playerStatusArea;
        private static readonly StatusType[] s_statusOrder = { StatusType.Radiance, StatusType.Majesty, StatusType.Dexterity, StatusType.Regen, StatusType.Thorns, StatusType.Weak, StatusType.Vulnerable, StatusType.Poison };
        private GameObject _pilePanel;
        private TextMeshProUGUI _pileText;
        private RectTransform _pileGridArea;
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

        public static TMP_FontAsset LoadKoreanFont()
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
            _canvas = canvas;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            RectTransform root = canvasGo.GetComponent<RectTransform>();
            _root = root;

            // 적 영역(상단, 최대 3슬롯 미리 생성) — 실제 위치는 Refresh 시 적 수에 따라 가운데 정렬
            for (int i = 0; i < MaxEnemies; i++)
            {
                var enemyGo = new GameObject($"EnemyView_{i}", typeof(RectTransform));
                enemyGo.transform.SetParent(root, false);
                var ert = (RectTransform)enemyGo.transform;
                ert.anchorMin = new Vector2(1f, 0.5f);
                ert.anchorMax = new Vector2(1f, 0.5f);
                ert.pivot = new Vector2(0.5f, 0.5f);
                ert.anchoredPosition = new Vector2(-540f, 40f);   // 우측 중앙(STS식 대치). 실제 위치/스케일은 Refresh에서 적 수에 따라
                var ev = enemyGo.AddComponent<EnemyView>();
                ev.Build(_font, i);
                enemyGo.SetActive(false);
                _enemyViews.Add(ev);
            }

            // 플레이어 영역(좌하단) — 박스 + 테두리(방어 카드 드래그 시 강조)
            var playerGo = new GameObject("PlayerView", typeof(RectTransform), typeof(Image));
            playerGo.transform.SetParent(root, false);
            var prt = (RectTransform)playerGo.transform;
            prt.anchorMin = new Vector2(0f, 0.5f);
            prt.anchorMax = new Vector2(0f, 0.5f);
            prt.pivot = new Vector2(0f, 0.5f);
            prt.anchoredPosition = new Vector2(60f, 20f);   // 좌측 중앙(STS식 대치 — 캐릭터 자리)
            prt.sizeDelta = new Vector2(460f, 220f);
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

            // 플레이어 status 칩 영역(PlayerView 위, hover 설명 — STS2식)
            var pstatGo = new GameObject("PlayerStatus", typeof(RectTransform));
            pstatGo.transform.SetParent(root, false);
            _playerStatusArea = (RectTransform)pstatGo.transform;
            _playerStatusArea.anchorMin = new Vector2(0f, 0.5f);
            _playerStatusArea.anchorMax = new Vector2(0f, 0.5f);
            _playerStatusArea.pivot = new Vector2(0f, 0f);
            _playerStatusArea.anchoredPosition = new Vector2(60f, 138f);   // 좌측 플레이어 박스 위
            _playerStatusArea.sizeDelta = new Vector2(460f, 40f);

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

            // 더미 보기 버튼 — 좌하단 모서리 세로(STS식: 뽑을/버린/소멸). 적(우측)·손패(중앙)와 안 겹침.
            CreateButton(root, "뽑을 카드", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(112f, 190f), new Vector2(156f, 50f),
                () => ShowPile(_controller.State.DrawPile, "뽑을 카드 (남은 덱)"));
            CreateButton(root, "버린 카드", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(112f, 132f), new Vector2(156f, 50f),
                () => ShowPile(_controller.State.DiscardPile, "버린 카드"));
            CreateButton(root, "소멸 카드", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(112f, 74f), new Vector2(156f, 50f),
                () => ShowPile(_controller.State.ExhaustPile, "소멸한 카드"));

            // 더미 내용 모달
            _pilePanel = new GameObject("PilePanel", typeof(RectTransform), typeof(Image));
            _pilePanel.transform.SetParent(root, false);
            var pp = _pilePanel.GetComponent<RectTransform>();
            pp.anchorMin = Vector2.zero;
            pp.anchorMax = Vector2.one;
            pp.offsetMin = Vector2.zero;
            pp.offsetMax = Vector2.zero;
            _pilePanel.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.04f, 0.9f);
            _pileText = CreateText(pp, "PileText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -64f), 34f, TextAlignmentOptions.Center);
            _pileText.rectTransform.sizeDelta = new Vector2(900f, 56f);
            var pileGridGo = new GameObject("PileGrid", typeof(RectTransform));
            pileGridGo.transform.SetParent(pp, false);
            _pileGridArea = (RectTransform)pileGridGo.transform;
            _pileGridArea.anchorMin = _pileGridArea.anchorMax = new Vector2(0.5f, 0.5f);
            _pileGridArea.pivot = new Vector2(0.5f, 0.5f);
            _pileGridArea.anchoredPosition = new Vector2(0f, -10f);
            _pileGridArea.sizeDelta = new Vector2(1180f, 780f);
            CreateButton(pp, "닫기", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 130f), new Vector2(220f, 62f),
                () => _pilePanel.SetActive(false));
            _pilePanel.SetActive(false);
        }

        /// <summary>전투 화면(Canvas) 표시 토글(한 판 루프에서 GameFlow가 제어).</summary>
        public void SetVisible(bool visible)
        {
            if (_canvas != null)
            {
                _canvas.enabled = visible;
            }
        }

        public void Refresh()
        {
            CombatState s = _controller.State;
            if (s == null)
            {
                return;
            }

            // 적별 HP 감소량만큼 피해 팝업(엔진 수정 없이 UI에서 감지)
            int curPlayerHp = s.Player.Hp;
            int playerDamage = _prevPlayerHp - curPlayerHp;
            if (playerDamage > 0 && _playerOutline != null)
            {
                ShowDamagePopup(_playerOutline.transform.position + new Vector3(0f, 80f, 0f), playerDamage, new Color(1f, 0.45f, 0.4f));
            }
            _prevPlayerHp = curPlayerHp;

            // 살아있는 적만 active + 위치 가운데 정렬
            int activeCount = 0;
            for (int i = 0; i < s.Enemies.Count && i < MaxEnemies; i++)
            {
                if (!s.Enemies[i].IsDead) { activeCount++; }
            }

            int slotIdx = 0;
            for (int i = 0; i < MaxEnemies; i++)
            {
                bool inRange = i < s.Enemies.Count;
                bool alive   = inRange && !s.Enemies[i].IsDead;
                _enemyViews[i].gameObject.SetActive(alive);

                if (alive)
                {
                    // STS식 우측 대치: 적은 화면 오른쪽에 가로 나열. 많을수록 박스 축소(scale)+간격 좁힘.
                    float escale = activeCount <= 1 ? 0.92f : activeCount == 2 ? 0.84f : 0.74f;
                    float espacing = 400f * escale + 64f;   // 박스폭(400)×scale + 여백64 → 인접 박스가 절대 안 겹침
                    float x = -560f + (slotIdx - (activeCount - 1) / 2f) * espacing;
                    var ert = (RectTransform)_enemyViews[i].transform;
                    ert.localScale = new Vector3(escale, escale, 1f);
                    ert.anchoredPosition = new Vector2(x, 20f);

                    _enemyViews[i].Bind(s.Enemies[i], s.Player);

                    // 적별 데미지 팝업
                    if (i < _prevEnemyHps.Length)
                    {
                        int dmg = _prevEnemyHps[i] - s.Enemies[i].Hp;
                        if (dmg > 0)
                        {
                            ShowDamagePopup(_enemyViews[i].transform.position, dmg, new Color(1f, 0.95f, 0.45f));
                        }
                    }

                    slotIdx++;
                }
            }

            // prevHp 배열 갱신(살아있는 적만 아니라 전체 슬롯 기준으로 저장해 인덱스 일치 유지)
            if (_prevEnemyHps.Length != s.Enemies.Count)
            {
                _prevEnemyHps = new int[s.Enemies.Count];
            }
            for (int i = 0; i < s.Enemies.Count; i++)
            {
                _prevEnemyHps[i] = s.Enemies[i].Hp;
            }

            PlayerState p = s.Player;
            _playerHpBar.Set(p.Hp, p.MaxHp);
            string blockStr = p.Block > 0 ? $"      <color=#5B9BD5><b>방어 {p.Block}</b></color>" : "";   // 방어>0만 표시(STS식), 청색
            _playerText.text =
                $"<size=150%><color=#9FD8F0><b>에너지 {p.Energy}/{p.BaseEnergy}</b></color></size>{blockStr}\n" +
                $"<size=78%>턴 {s.Turn}   ·   덱 {s.DrawPile.Count} / 버린 {s.DiscardPile.Count} / 소멸 {s.ExhaustPile.Count}</size>";
            RebuildStatus(p);

            RebuildHand(s);

            // 런 모드에선 승패 후 RunUI가 보상/게임오버 화면을 띄운다 → CombatView 결과 패널("다시 시작")은
            // 쓰지 않는다(보상 직전 0.7초 동안 잠깐 깜빡이던 문제 제거).
            _resultPanel.SetActive(false);
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
                // 사용한 카드는 추적에서 빼고 사용 연출(스스로 소멸)을 재생한 뒤 손패를 갱신
                _cardViews.Remove(card.Card.InstanceId);
                card.PlayUseAnimation();
                Refresh();
            }
            return ok;
        }

        /// <summary>적 박스 조준 테두리. 인자와 일치하는 뷰만 on, 나머지는 off(null이면 전부 off).</summary>
        public void SetEnemyHighlight(EnemyView enemy)
        {
            foreach (EnemyView ev in _enemyViews)
            {
                ev.SetHighlight(enemy != null && ev == enemy);
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

        /// <summary>새 전투 시작 시 호출: 이전 전투의 데미지 팝업 잔존을 제거하고 HP 기준점을 초기화한다.</summary>
        public void ResetForNewCombat()
        {
            var kill = new System.Collections.Generic.List<GameObject>();
            foreach (Transform c in _root)
            {
                if (c.name == "DamagePopup") { kill.Add(c.gameObject); }
            }
            foreach (GameObject g in kill) { Destroy(g); }

            CombatState s = _controller.State;
            _prevPlayerHp = s != null ? s.Player.Hp : 0;

            // 적별 prevHp 초기화
            int count = (s != null) ? s.Enemies.Count : 0;
            _prevEnemyHps = new int[count];
            for (int i = 0; i < count; i++)
            {
                _prevEnemyHps[i] = s.Enemies[i].Hp;
            }
        }

        // 손패를 InstanceId로 재사용해 갱신한다(유지 카드는 위치만 트윈, 빠진 카드만 제거, 새 카드만 생성).
        /// <summary>플레이어 status를 칩으로 다시 그린다(active만, hover 설명 — STS2식).</summary>
        private void RebuildStatus(PlayerState p)
        {
            var kill = new System.Collections.Generic.List<GameObject>();
            foreach (Transform c in _playerStatusArea) { kill.Add(c.gameObject); }
            foreach (var g in kill) { Destroy(g); }
            int idx = 0;
            foreach (StatusType st in s_statusOrder)
            {
                int amt = p.GetStatus(st);
                if (amt <= 0) { continue; }
                var chip = new GameObject($"St_{st}", typeof(RectTransform), typeof(Image), typeof(TooltipTrigger));
                chip.transform.SetParent(_playerStatusArea, false);
                var crt = (RectTransform)chip.transform;
                crt.anchorMin = new Vector2(0f, 0.5f);
                crt.anchorMax = new Vector2(0f, 0.5f);
                crt.pivot = new Vector2(0f, 0.5f);
                crt.anchoredPosition = new Vector2(idx * 96f, 0f);
                crt.sizeDelta = new Vector2(90f, 40f);
                var sc = DesignTokens.StatusColor(st);
                chip.GetComponent<Image>().color = new Color(sc.r * 0.5f, sc.g * 0.5f, sc.b * 0.5f, 0.95f);   // 의미색을 어둡게 → 흰 글씨 대비 확보
                chip.GetComponent<TooltipTrigger>().Set(GameInfo.StatusDesc(st, amt));
                var lbl = CreateText(crt, "L", Vector2.zero, Vector2.one, Vector2.zero, 20f, TextAlignmentOptions.Center);
                lbl.enableAutoSizing = true;   // 큰 수치(취약 12 등)도 칩 안에 맞춤
                lbl.fontSizeMin = 12f;
                lbl.fontSizeMax = 20f;
                lbl.text = $"{GameInfo.StatusName(st)} {amt}";
                lbl.raycastTarget = false;
                idx++;
            }
        }

        /// <summary>더미(뽑을/버린/소멸) 내용을 카드명 ×수량으로 모달에 표시 — STS2식.</summary>
        private void ShowPile(System.Collections.Generic.List<CardInstance> pile, string title)
        {
            _pileText.text = pile.Count == 0 ? $"<b>{title}</b>  (비어 있음)" : $"<b>{title}</b>  ({pile.Count}장)";
            PopulatePileGrid(pile);
            _pilePanel.SetActive(true);
            _pilePanel.transform.SetAsLastSibling();
        }

        /// <summary>더미 모달을 카드(CardView) 그리드로 채운다 — 한 줄 5장, 정적 배치(트윈 끔).</summary>
        private void PopulatePileGrid(System.Collections.Generic.List<CardInstance> pile)
        {
            var kill = new System.Collections.Generic.List<GameObject>();
            foreach (Transform c in _pileGridArea) { kill.Add(c.gameObject); }
            foreach (GameObject g in kill) { Destroy(g); }

            int cols = 5;
            float scale = pile.Count > 15 ? 0.54f : 0.64f;
            float cw = (200f * scale) + 30f;
            float ch = (280f * scale) + 22f;
            float x0 = -(cols - 1) / 2f * cw;
            float y0 = (_pileGridArea.sizeDelta.y / 2f) - (ch / 2f) - 6f;
            for (int i = 0; i < pile.Count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                var go = new GameObject("PileCard", typeof(RectTransform));
                go.transform.SetParent(_pileGridArea, false);
                var cv = go.AddComponent<CardView>();
                cv.Build(_font);
                cv.Bind(pile[i], true);
                cv.enabled = false;   // 손패 트윈 비활성 — 모달은 정적 배치
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localScale = new Vector3(scale, scale, 1f);
                rt.anchoredPosition = new Vector2(x0 + col * cw, y0 - row * ch);
            }
        }

        private void RebuildHand(CombatState s)
        {
            var hand = s.Hand;
            var present = new HashSet<int>();
            var ordered = new List<CardView>(hand.Count);

            for (int i = 0; i < hand.Count; i++)
            {
                CardInstance c = hand[i];
                present.Add(c.InstanceId);
                bool playable = s.Player.Energy >= c.Data.Cost;

                if (!_cardViews.TryGetValue(c.InstanceId, out CardView card))
                {
                    var go = new GameObject("Card", typeof(RectTransform));
                    go.transform.SetParent(_handArea, false);
                    card = go.AddComponent<CardView>();
                    card.Build(_font);
                    card.SetCombat(this);
                    _cardViews[c.InstanceId] = card;
                }
                int atkBonus = s.Player.GetStatus(StatusType.Majesty) + s.Player.GetStatus(StatusType.Radiance);
                bool weak = s.Player.GetStatus(StatusType.Weak) > 0;
                card.Bind(c, playable, atkBonus, weak);
                ordered.Add(card);
            }

            // 손패에서 빠진 카드 제거
            var stale = new List<int>();
            foreach (var kv in _cardViews)
            {
                if (!present.Contains(kv.Key))
                {
                    stale.Add(kv.Key);
                }
            }
            for (int i = 0; i < stale.Count; i++)
            {
                Destroy(_cardViews[stale[i]].gameObject);
                _cardViews.Remove(stale[i]);
            }

            LayoutHand(ordered);
        }

        // 손패 카드를 부채꼴(아래 손잡이를 중심으로 한 원호)로 배치하고 각 카드의 기준 위치·회전을 지정한다.
        private void LayoutHand(List<CardView> cards)
        {
            int n = cards.Count;
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
                cards[i].transform.SetSiblingIndex(i);
                cards[i].SetHome(new Vector2(x, y), -angle);
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
            txt.enableAutoSizing = true;   // 긴 버튼 라벨도 버튼 안에 맞춤
            txt.fontSizeMin = 13f;
            txt.fontSizeMax = 22f;
            txt.text = label;
            return go.GetComponent<Button>();
        }

    }
}

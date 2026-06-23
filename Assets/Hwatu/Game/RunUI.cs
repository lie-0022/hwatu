using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Core.Enemies;
using Hwatu.Core.Rng;
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
        private RectTransform _rewardCardArea;
        private TextMeshProUGUI _rewardTitle;
        private GameObject _resultPanel;
        private TextMeshProUGUI _resultText;
        private TextMeshProUGUI _mapHud;
        private TextMeshProUGUI _mapHpChip;
        private TextMeshProUGUI _mapGoldChip;
        private GameObject _eventPanel;
        private TextMeshProUGUI _eventText;
        private RectTransform _eventChoiceArea;
        private GameObject _restPanel;
        private GameObject _shopPanel;
        private RectTransform _shopItemsRoot;
        private TextMeshProUGUI _shopText;

        private MapView _mapView;
        private GameObject _combatGo;
        private CombatController _combatCtrl;
        private CombatView _combatView;
        private GameObject _potionBarGo;
        private GameObject _deckPanel;
        private TextMeshProUGUI _deckText;
        private RectTransform _deckGridArea;
        private GameObject _upgradePanel;
        private GameObject _upgradeCardArea;
        private bool _isEnchant;
        private TextMeshProUGUI _upgradeTitle;
        private GameObject _removePanel;
        private GameObject _removeCardArea;
        private TextMeshProUGUI _removeTitle;
        private GameObject _relicBarGo;
        private GameObject _neowPanel;

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
            CreateButton(_charPanel, "광객 (光客) — 빛/공격", new Vector2(0, 40), () => _flow.StartNewRun(CharacterData.Luminary()))
                .gameObject.AddComponent<TooltipTrigger>().Set(GameInfo.CharacterDesc(CharacterData.Luminary()));
            CreateButton(_charPanel, "묵귀 (墨鬼) — 독/약화", new Vector2(0, -40), () => _flow.StartNewRun(CharacterData.InkSpirit()))
                .gameObject.AddComponent<TooltipTrigger>().Set(GameInfo.CharacterDesc(CharacterData.InkSpirit()));

            // 맵
            _mapPanel = CreatePanel(root, "MapPanel", new Color(0.06f, 0.07f, 0.10f, 1f));
            _mapView = _mapPanel.AddComponent<MapView>();
            _mapView.Init(_flow, _font, _mapPanel.GetComponent<RectTransform>());
            // HP·골드 미니칩(아이콘+숫자) + 액트/유물/포션 텍스트
            _mapHpChip = UiChips.MakeHudChip(this, _mapPanel.transform, _font, IconCatalog.Hp, DesignTokens.Danger, new Vector2(40f, -12f), 124f);
            _mapGoldChip = UiChips.MakeHudChip(this, _mapPanel.transform, _font, IconCatalog.Gold, DesignTokens.Gold, new Vector2(180f, -12f), 110f);
            _mapHud = CreateText(_mapPanel, "", 26f, new Vector2(0, 320));
            _mapHud.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            _mapHud.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            _mapHud.rectTransform.anchoredPosition = new Vector2(150f, -28f);
            _mapHud.rectTransform.sizeDelta = new Vector2(800f, 48f);
            CreateButton(_mapPanel, "덱 보기", new Vector2(720, 320), () => OpenDeckView());

            // 덱 보기 모달(맵 위 오버레이 — 버튼으로 열고 닫음)
            _deckPanel = CreatePanel(root, "DeckPanel", new Color(0.05f, 0.05f, 0.08f, 0.98f));
            _deckText = CreateText(_deckPanel, "보유 덱", 44f, new Vector2(0, 470));
            var deckGrid = new GameObject("DeckGrid", typeof(RectTransform));
            deckGrid.transform.SetParent(_deckPanel.transform, false);
            _deckGridArea = (RectTransform)deckGrid.transform;
            _deckGridArea.anchorMin = _deckGridArea.anchorMax = new Vector2(0.5f, 0.5f);
            _deckGridArea.pivot = new Vector2(0.5f, 0.5f);
            _deckGridArea.anchoredPosition = new Vector2(0, -10);
            _deckGridArea.sizeDelta = new Vector2(1040f, 770f);
            CreateButton(_deckPanel, "닫기", new Vector2(0, -470), () => CloseDeckView());
            _deckPanel.SetActive(false);

            // 시작 축복(Neow) — 캐릭터 선택 직후 보너스 택1
            _neowPanel = CreatePanel(root, "NeowPanel", new Color(0.06f, 0.05f, 0.12f, 1f));
            CreateText(_neowPanel, "시작 축복 — 하나 선택", 50f, new Vector2(0, 180));
            CreateButton(_neowPanel, "최대 체력 +8", new Vector2(0, 60), () => _flow.ApplyNeowBoon(0));
            CreateButton(_neowPanel, "골드 +100", new Vector2(0, -20), () => _flow.ApplyNeowBoon(1));
            CreateButton(_neowPanel, "유물 1개 획득", new Vector2(0, -100), () => _flow.ApplyNeowBoon(2));
            CreateButton(_neowPanel, "첫 카드 강화 (+3)", new Vector2(0, -180), () => _flow.ApplyNeowBoon(3));
            _neowPanel.SetActive(false);

            // 보상(카드 3택1 + 스킵)
            _rewardPanel = CreatePanel(root, "RewardPanel", new Color(0.09f, 0.09f, 0.06f, 0.97f));
            _rewardTitle = CreateText(_rewardPanel, "카드 보상 — 1장 선택 (또는 건너뛰기)", 46f, new Vector2(0, 300));
            CreateButton(_rewardPanel, "건너뛰기 (덱 압축)", new Vector2(0, -320), () => _flow.OnRewardDone());
            var areaGo = new GameObject("RewardCards", typeof(RectTransform));
            areaGo.transform.SetParent(_rewardPanel.transform, false);
            _rewardCardArea = (RectTransform)areaGo.transform;
            _rewardCardArea.anchorMin = new Vector2(0.5f, 0.5f);
            _rewardCardArea.anchorMax = new Vector2(0.5f, 0.5f);
            _rewardCardArea.pivot = new Vector2(0.5f, 0.5f);
            _rewardCardArea.anchoredPosition = new Vector2(0, 20);
            _rewardCardArea.sizeDelta = new Vector2(900, 320);
            CreateButton(_rewardPanel, "스킵", new Vector2(0, -340), () => _flow.OnRewardDone());

            // 결과
            _resultPanel = CreatePanel(root, "ResultPanel", new Color(0f, 0f, 0f, 0.9f));
            _resultText = CreateText(_resultPanel, "", 72f, new Vector2(0, 70));
            CreateButton(_resultPanel, "메인으로", new Vector2(0, -90), () => _flow.BackToMenu());

            // 이벤트(제목/설명 + 선택지 버튼)
            _eventPanel = CreatePanel(root, "EventPanel", new Color(0.07f, 0.05f, 0.10f, 0.97f));
            _eventText = CreateText(_eventPanel, "", 34f, new Vector2(0, 200));
            var evAreaGo = new GameObject("EventChoices", typeof(RectTransform));
            evAreaGo.transform.SetParent(_eventPanel.transform, false);
            _eventChoiceArea = (RectTransform)evAreaGo.transform;
            _eventChoiceArea.anchoredPosition = new Vector2(0, -40);

            // 휴식처(회복/강화 선택)
            _restPanel = CreatePanel(root, "RestPanel", new Color(0.05f, 0.09f, 0.07f, 0.97f));
            CreateText(_restPanel, "휴식처", 50f, new Vector2(0, 160));
            CreateButton(_restPanel, "회복 (HP 30%)", new Vector2(0, 20), () => _flow.OnRest(0));
            CreateButton(_restPanel, "강화 (카드 선택)", new Vector2(0, -60), () => OpenUpgradeView(false));
            CreateButton(_restPanel, "벼리기 (예리 인챈트)", new Vector2(0, -140), () => OpenUpgradeView(true));

            // 강화 카드 선택 모달(휴식 위 오버레이)
            _upgradePanel = CreatePanel(root, "UpgradePanel", new Color(0.06f, 0.05f, 0.09f, 0.98f));
            _upgradeTitle = CreateText(_upgradePanel, "강화할 카드 선택", 50f, new Vector2(0, 460));
            var upArea = new GameObject("UpgradeCardArea", typeof(RectTransform));
            upArea.transform.SetParent(_upgradePanel.transform, false);
            var upRt = (RectTransform)upArea.transform;
            upRt.anchorMin = upRt.anchorMax = new Vector2(0.5f, 0.5f);
            upRt.pivot = new Vector2(0.5f, 0.5f);
            upRt.anchoredPosition = new Vector2(0, -20);
            upRt.sizeDelta = new Vector2(1040f, 770f);
            _upgradeCardArea = upArea;
            CreateButton(_upgradePanel, "취소", new Vector2(0, -470), () => CloseUpgradeView());
            _upgradePanel.SetActive(false);

            // 카드 제거 선택 모달(상점 위 오버레이) — 덱을 카드 형태로 띄워 1장 골라 삭제
            _removePanel = CreatePanel(root, "RemovePanel", new Color(0.09f, 0.05f, 0.05f, 0.98f));
            _removeTitle = CreateText(_removePanel, "제거할 카드 선택", 46f, new Vector2(0, 470));
            var rmArea = new GameObject("RemoveCardArea", typeof(RectTransform));
            rmArea.transform.SetParent(_removePanel.transform, false);
            var rmRt = (RectTransform)rmArea.transform;
            rmRt.anchorMin = rmRt.anchorMax = new Vector2(0.5f, 0.5f);
            rmRt.pivot = new Vector2(0.5f, 0.5f);
            rmRt.anchoredPosition = new Vector2(0, -10);
            rmRt.sizeDelta = new Vector2(1040f, 770f);
            _removeCardArea = rmArea;
            CreateButton(_removePanel, "취소", new Vector2(0, -470), () => CloseRemoveView());
            _removePanel.SetActive(false);

            // 상점(STS2식 다중 매물: 카드·유물·포션 동시 진열 + 개별 구매)
            _shopPanel = CreatePanel(root, "ShopPanel", new Color(0.10f, 0.08f, 0.04f, 0.97f));
            CreateText(_shopPanel, "상점", 50f, new Vector2(0, 440));
            _shopText = CreateText(_shopPanel, "", 30f, new Vector2(0, 380));
            var itemsRootGo = new GameObject("ShopItems", typeof(RectTransform));
            itemsRootGo.transform.SetParent(_shopPanel.transform, false);
            _shopItemsRoot = (RectTransform)itemsRootGo.transform;
            _shopItemsRoot.anchorMin = new Vector2(0.5f, 0.5f);
            _shopItemsRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _shopItemsRoot.pivot = new Vector2(0.5f, 0.5f);
            _shopItemsRoot.anchoredPosition = Vector2.zero;
            _shopItemsRoot.sizeDelta = new Vector2(1240f, 720f);
            CreateButton(_shopPanel, "매물 새로고침 (15골드)", new Vector2(-220, -430), () => { _flow.ShopReroll(); BuildShop(); });
            CreateButton(_shopPanel, "카드 제거 (75골드)", new Vector2(220, -430), () => OpenRemoveView());
            CreateButton(_shopPanel, "나가기", new Vector2(0, -510), () => _flow.OnShopLeave());

            // 전투 GO(자체 Canvas, 초기 비활성)
            _combatGo = new GameObject("RunCombat", typeof(CombatController), typeof(CombatView));
            _combatCtrl = _combatGo.GetComponent<CombatController>();
            _combatView = _combatGo.GetComponent<CombatView>();
            _combatCtrl.OnCombatEnded += HandleCombatEnded;
            _combatGo.SetActive(false);

            // 포션 바(전투 위 오버레이 — 별도 캔버스 sortingOrder=50, 전투 중에만 표시)
            var potionCanvasGo = new GameObject("PotionBar", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            potionCanvasGo.transform.SetParent(transform, false);
            var pcanvas = potionCanvasGo.GetComponent<Canvas>();
            pcanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            pcanvas.sortingOrder = 50;
            var pscaler = potionCanvasGo.GetComponent<CanvasScaler>();
            pscaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            pscaler.referenceResolution = new Vector2(1920, 1080);
            _potionBarGo = potionCanvasGo;
            _potionBarGo.SetActive(false);

            // 공통 툴팁(최상위 sortingOrder 200)
            var tooltipGo = new GameObject("Tooltip", typeof(TooltipUI));
            tooltipGo.transform.SetParent(transform, false);
            tooltipGo.GetComponent<TooltipUI>().Build(_font);

            // 유물 보유 바(맵·전투 상단, hover 효과 설명)
            var relicCanvasGo = new GameObject("RelicBar", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            relicCanvasGo.transform.SetParent(transform, false);
            var rcanvas = relicCanvasGo.GetComponent<Canvas>();
            rcanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rcanvas.sortingOrder = 55;
            var rscaler = relicCanvasGo.GetComponent<CanvasScaler>();
            rscaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            rscaler.referenceResolution = new Vector2(1920, 1080);
            _relicBarGo = relicCanvasGo;
            _relicBarGo.SetActive(false);
        }

        private void OnPhaseChanged(RunPhase p)
        {
            _menuPanel.SetActive(p == RunPhase.MainMenu);
            _neowPanel.SetActive(p == RunPhase.NeowBoon);
            _charPanel.SetActive(p == RunPhase.CharacterSelect);
            _mapPanel.SetActive(p == RunPhase.Map);
            _deckPanel.SetActive(false);
            _rewardPanel.SetActive(p == RunPhase.Reward);
            _resultPanel.SetActive(p == RunPhase.GameOver || p == RunPhase.Victory);
            _eventPanel.SetActive(p == RunPhase.Event);
            _restPanel.SetActive(p == RunPhase.Rest);
            _upgradePanel.SetActive(false);
            _removePanel.SetActive(false);
            _shopPanel.SetActive(p == RunPhase.Shop);
            _combatGo.SetActive(p == RunPhase.Combat);
            _potionBarGo.SetActive(p == RunPhase.Combat);
            if (p == RunPhase.Combat)
            {
                BuildPotions();
            }
            _relicBarGo.SetActive(p == RunPhase.Map || p == RunPhase.Combat);
            if (p == RunPhase.Map || p == RunPhase.Combat)
            {
                BuildRelicBar();
            }

            if (p == RunPhase.Map)
            {
                _mapView.Build();
                _mapHpChip.text = $"{_flow.Run.Hp}/{_flow.Run.MaxHp}";
                _mapGoldChip.text = _flow.Run.Gold.ToString();
                _mapHud.text = $"액트 {_flow.Run.Act}    유물 {_flow.Run.Relics.Count}    포션 {_flow.Run.Potions.Count}/{RunState.MaxPotions}";
            }
            else if (p == RunPhase.Event)
            {
                BuildEvent();
            }
            else if (p == RunPhase.Shop)
            {
                BuildShop();
            }
            else if (p == RunPhase.Reward)
            {
                BuildReward();
            }
            else if (p == RunPhase.Combat)
            {
                StartCoroutine(StartCombatNextFrame());
            }
            else if (p == RunPhase.GameOver)
            {
                _resultText.text = $"패배...\n<size=45%>{_flow.Run.Character.Name} · 액트 {_flow.Run.Act} · 골드 {_flow.Run.Gold} · 유물 {_flow.Run.Relics.Count} · 덱 {_flow.Run.Deck.Count}장</size>";
            }
            else if (p == RunPhase.Victory)
            {
                _resultText.text = $"승리!\n<size=45%>{_flow.Run.Character.Name} · 액트 {_flow.Run.Act} 클리어 · 유물 {_flow.Run.Relics.Count} · 덱 {_flow.Run.Deck.Count}장</size>";
            }
        }

        /// <summary>이벤트 화면을 현재 이벤트로 채운다(제목/설명 + 선택지 버튼).</summary>
        private void BuildEvent()
        {
            EventData ev = _flow.CurrentEvent;
            if (ev == null)
            {
                return;
            }
            _eventText.text = $"<b>{ev.Title}</b>\n<size=65%>{ev.Description}</size>";
            for (int i = _eventChoiceArea.childCount - 1; i >= 0; i--)
            {
                Destroy(_eventChoiceArea.GetChild(i).gameObject);
            }
            for (int i = 0; i < ev.Choices.Count; i++)
            {
                int idx = i;
                CreateButton(_eventChoiceArea.gameObject, ev.Choices[i].Label, new Vector2(0, -i * 70), () => _flow.OnEventChoice(idx))
                    .gameObject.AddComponent<TooltipTrigger>().Set(ev.Choices[idx].Result);
            }
        }

        /// <summary>상점 화면을 현재 매물(카드·유물·포션)로 채운다 — 개별 구매 버튼 그리드.</summary>
        private void BuildShop()
        {
            _shopText.text = $"골드 {_flow.Run.Gold}    <size=64%>(매물에 마우스를 올리면 효과)</size>";

            var kill = new List<GameObject>();
            foreach (Transform c in _shopItemsRoot) { kill.Add(c.gameObject); }
            foreach (GameObject g in kill) { Destroy(g); }

            ShopStock shop = _flow.CurrentShop;
            if (shop == null) { return; }
            for (int i = 0; i < shop.Items.Count; i++)
            {
                ShopItem it = shop.Items[i];
                int idx = i;
                bool left = i < 5;            // 좌열 카드5 / 우열 유물·포션
                int row = left ? i : i - 5;
                float x = left ? -300f : 300f;
                float y = 280f - row * 92f;
                CreateShopButton(it, new Vector2(x, y), () => { if (_flow.BuyShopItem(idx)) { BuildShop(); } });
            }
        }

        /// <summary>상점 매물 1칸 버튼(종류·이름·가격 + hover 설명, 품절/골드부족 시 비활성).</summary>
        private void CreateShopButton(ShopItem it, Vector2 pos, Action onClick)
        {
            var go = new GameObject("Shop_" + it.DisplayName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TooltipTrigger));
            go.transform.SetParent(_shopItemsRoot, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(560f, 80f);
            string kindTag = it.Kind == ShopItemKind.Card ? "카드" : (it.Kind == ShopItemKind.Relic ? "유물" : "포션");
            bool afford = _flow.Run.Gold >= it.Price && !it.Sold;
            go.GetComponent<Image>().color = it.Sold
                ? new Color(0.18f, 0.16f, 0.14f)
                : (afford ? new Color(0.30f, 0.25f, 0.13f) : new Color(0.24f, 0.15f, 0.13f));
            var btn = go.GetComponent<Button>();
            btn.interactable = !it.Sold;
            btn.onClick.AddListener(() => onClick());
            go.GetComponent<TooltipTrigger>().Set(ShopItemDesc(it));

            var t = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            t.transform.SetParent(rt, false);
            var tmp = t.GetComponent<TextMeshProUGUI>();
            tmp.font = _font;
            tmp.fontSize = 26f;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 14f;
            tmp.fontSizeMax = 26f;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.text = it.Sold
                ? $"[{kindTag}] {it.DisplayName} — 품절"
                : $"[{kindTag}] {it.DisplayName} — {it.Price}골드";
            tmp.raycastTarget = false;
            var lrt = tmp.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(12f, 0f);
            lrt.offsetMax = new Vector2(-12f, 0f);
        }

        /// <summary>매물 종류별 hover 설명(카드/유물/포션).</summary>
        private string ShopItemDesc(ShopItem it)
        {
            switch (it.Kind)
            {
                case ShopItemKind.Card: return GameInfo.CardDesc(it.Card);
                case ShopItemKind.Relic: return GameInfo.RelicDesc(it.Relic);
                default: return GameInfo.PotionDesc(it.Potion);
            }
        }

        /// <summary>덱 보기 모달을 현재 덱 카드(카드 형태 그리드)로 채워 연다.</summary>
        private void OpenDeckView()
        {
            if (_flow.Run == null)
            {
                return;
            }
            _deckText.text = $"보유 덱 — 총 {_flow.Run.Deck.Count}장";
            PopulateCardGrid(_deckGridArea, _flow.Run.Deck, null, null);
            _deckPanel.SetActive(true);
            _deckPanel.transform.SetAsLastSibling();
        }

        /// <summary>부모에 덱 카드들을 CardView로 그리드 배치한다(축소 스케일, 클릭·hover 옵션). 카드에 이름·효과가 그려진다.</summary>
        private void PopulateCardGrid(RectTransform parent, List<CardData> cards, Action<int> onClick, Func<CardData, string> hover)
        {
            var kill = new List<GameObject>();
            foreach (Transform c in parent) { kill.Add(c.gameObject); }
            foreach (GameObject g in kill) { Destroy(g); }

            int cols = 5;                                            // 한 줄에 5장
            float scale = cards.Count > 15 ? 0.54f : 0.66f;
            float cw = (200f * scale) + 30f;
            float ch = (280f * scale) + 22f;
            float x0 = -(cols - 1) / 2f * cw;
            float y0 = (parent.sizeDelta.y / 2f) - (ch / 2f) - 6f;
            for (int i = 0; i < cards.Count; i++)
            {
                int idx = i;
                int col = i % cols;
                int row = i / cols;
                var go = new GameObject("DeckCard", typeof(RectTransform));
                go.transform.SetParent(parent, false);
                var cv = go.AddComponent<CardView>();
                cv.Build(_font);
                cv.Bind(new CardInstance(cards[i], i), true);
                cv.enabled = false;   // 손패 트윈 Update 비활성 — 모달은 정적 배치(안 끄면 전부 중앙으로 모이고 스케일이 덮어써짐)
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localScale = new Vector3(scale, scale, 1f);
                rt.anchoredPosition = new Vector2(x0 + col * cw, y0 - row * ch);
                if (hover != null)
                {
                    go.AddComponent<TooltipTrigger>().Set(hover(cards[idx]));
                }
                if (onClick != null)
                {
                    var btn = go.AddComponent<Button>();
                    btn.onClick.AddListener(() => onClick(idx));
                }
            }
        }

        /// <summary>덱 보기 모달을 닫는다.</summary>
        private void CloseDeckView()
        {
            _deckPanel.SetActive(false);
        }

        /// <summary>카드 제거 모달을 연다 — 덱을 카드 형태로 띄워 1장 골라 삭제(상점, 골드 소모).</summary>
        private void OpenRemoveView()
        {
            if (_flow.Run == null) { return; }
            int cost = _flow.CurrentShop != null ? _flow.CurrentShop.RemoveCost : 75;
            bool used = _flow.CurrentShop != null && _flow.CurrentShop.RemoveUsed;
            _removeTitle.text = used ? "이미 이 상점에서 카드를 제거했습니다" : $"제거할 카드 선택 — {cost}골드";
            PopulateCardGrid((RectTransform)_removeCardArea.transform, _flow.Run.Deck, idx => RemoveAndClose(idx), c => GameInfo.CardDesc(c));
            _removePanel.SetActive(true);
            _removePanel.transform.SetAsLastSibling();
        }

        /// <summary>선택 카드를 제거하고(성공 시) 모달을 닫고 상점을 갱신한다.</summary>
        private void RemoveAndClose(int index)
        {
            if (_flow.ShopRemoveCardAt(index))
            {
                _removePanel.SetActive(false);
                BuildShop();
            }
        }

        /// <summary>카드 제거 모달을 닫는다.</summary>
        private void CloseRemoveView()
        {
            _removePanel.SetActive(false);
        }

        /// <summary>강화할 카드를 덱에서 고르는 모달을 연다(카드별 버튼 6열 그리드).</summary>
        private void OpenUpgradeView(bool enchant)
        {
            _isEnchant = enchant;
            _upgradeTitle.text = enchant
                ? "벼릴 카드 선택 (예리) — 카드에 마우스를 올리면 강화 후 효과"
                : "강화할 카드 선택 — 카드에 마우스를 올리면 강화 후 효과";
            if (_flow.Run == null)
            {
                return;
            }
            PopulateCardGrid((RectTransform)_upgradeCardArea.transform, _flow.Run.Deck, idx => UpgradeAndClose(idx), EnchantPreviewDesc);
            _upgradePanel.SetActive(true);
            _upgradePanel.transform.SetAsLastSibling();
        }

        /// <summary>강화/벼리기 시 카드가 어떻게 바뀌는지 — 지금 효과와 강화 후 효과를 함께 보여준다.</summary>
        private string EnchantPreviewDesc(CardData card)
        {
            CardData after = _isEnchant ? card.WithEnchant("sharp") : card.Upgrade();
            string label = _isEnchant ? "벼린 후 (예리)" : "강화 후";
            return $"<b>지금</b>\n{GameInfo.CardDesc(card)}\n\n<b>→ {label}</b>\n{GameInfo.CardDesc(after)}";
        }

        /// <summary>선택 카드를 강화하고 모달을 닫는다(휴식 종료 → 맵).</summary>
        private void UpgradeAndClose(int index)
        {
            _upgradePanel.SetActive(false);
            if (_isEnchant) { _flow.RestEnchantCard(index); }
            else { _flow.RestUpgradeCard(index); }
        }

        /// <summary>강화 모달을 닫는다(강화 없이 휴식 화면 유지).</summary>
        private void CloseUpgradeView()
        {
            _upgradePanel.SetActive(false);
        }

        /// <summary>보유 유물을 상단 가로 바에 칩으로 그린다(hover 시 효과 설명 — STS2식).</summary>
        private void BuildRelicBar()
        {
            if (_flow.Run == null)
            {
                return;
            }
            var existing = new List<GameObject>();
            foreach (Transform child in _relicBarGo.transform)
            {
                existing.Add(child.gameObject);
            }
            foreach (GameObject go in existing)
            {
                Destroy(go);
            }
            var relics = _flow.Run.Relics;
            for (int i = 0; i < relics.Count; i++)
            {
                var chip = new GameObject($"Relic{i}", typeof(RectTransform), typeof(Image), typeof(TooltipTrigger));
                chip.transform.SetParent(_relicBarGo.transform, false);
                var crt = (RectTransform)chip.transform;
                crt.anchorMin = new Vector2(0f, 1f);
                crt.anchorMax = new Vector2(0f, 1f);
                crt.pivot = new Vector2(0f, 1f);
                crt.anchoredPosition = new Vector2(24f + i * 132f, -78f);
                crt.sizeDelta = new Vector2(124f, 48f);
                chip.GetComponent<Image>().color = new Color(0.28f, 0.22f, 0.12f, 0.96f);
                chip.GetComponent<TooltipTrigger>().Set(GameInfo.RelicDesc(relics[i]));

                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelGo.transform.SetParent(chip.transform, false);
                var lbl = labelGo.GetComponent<TextMeshProUGUI>();
                lbl.font = _font;
                lbl.fontSize = 22f;
                lbl.alignment = TextAlignmentOptions.Center;
                lbl.color = Color.white;
                lbl.raycastTarget = false;
                lbl.textWrappingMode = TextWrappingModes.NoWrap;
                lbl.text = relics[i].Name;
                var lrt = lbl.rectTransform;
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;
            }
        }

        /// <summary>전투 중 포션 슬롯 버튼을 현재 보유 포션으로 다시 그린다(좌측 세로).</summary>
        private void BuildPotions()
        {
            if (_flow.Run == null)
            {
                return;
            }
            var existing = new List<GameObject>();
            foreach (Transform child in _potionBarGo.transform)
            {
                existing.Add(child.gameObject);
            }
            foreach (GameObject go in existing)
            {
                Destroy(go);
            }
            var potions = _flow.Run.Potions;
            for (int i = 0; i < potions.Count; i++)
            {
                PotionData potion = potions[i];
                var pos = new Vector2(-840f, 380f - i * 90f);
                var pbtn = CreateButton(_potionBarGo, potion.Name, pos, () => UsePotionInCombat(potion));
                pbtn.gameObject.AddComponent<TooltipTrigger>().Set(GameInfo.PotionDesc(potion));
            }
        }

        /// <summary>포션을 전투에 사용하고(즉시 효과) 슬롯에서 제거한 뒤 전투 화면을 갱신한다.</summary>
        private void UsePotionInCombat(PotionData potion)
        {
            if (_combatCtrl.UsePotion(potion))
            {
                _flow.Run.Potions.Remove(potion);
                _combatView.Refresh();
                BuildPotions();
            }
        }

        // CombatView.Start(BuildUI)가 끝난 다음 프레임에 런 덱으로 전투 시작.
        private IEnumerator StartCombatNextFrame()
        {
            yield return null;
            MapNode node = _flow.Run.Map.GetNode(_flow.Run.CurrentNodeId);
            NodeType nodeType = node != null ? node.Type : NodeType.Combat;
            IRandom enemyRng = new RngStreams(_flow.Run.Seed).ForStream("enemy_" + _flow.Run.CurrentNodeId);
            var enemies = EnemyContent.PickEnemies(nodeType, enemyRng, _flow.Run.Act);
            ulong combatSeed = _flow.Run.Seed + (ulong)(_flow.Run.CurrentNodeId + 1);
            _combatCtrl.StartCombat(_flow.Run.Deck, enemies, combatSeed, _flow.Run.MaxHp, _flow.Run.Hp, _flow.Run.Relics);
            _combatView.SetVisible(true);
            _combatView.ResetForNewCombat();
            _combatView.Refresh();
        }

        private void HandleCombatEnded(bool won)
        {
            CombatState s = _combatCtrl.State;
            if (s != null)
            {
                _flow.Run.Hp = s.Player.Hp;
            }
            StartCoroutine(EndCombatAfterDelay(won));
        }

        /// <summary>처치 데미지 팝업·승패 연출을 잠깐 보여준 뒤 화면을 전환한다.</summary>
        private IEnumerator EndCombatAfterDelay(bool won)
        {
            yield return new WaitForSeconds(0.7f);
            _combatView.SetVisible(false);
            _flow.OnCombatEnded(won);
        }

        // 전투 종류에 맞는 카드 보상 3장을 뽑아 버튼으로 표시(선택 → 덱 추가).
        private void BuildReward()
        {
            for (int i = _rewardCardArea.childCount - 1; i >= 0; i--)
            {
                Destroy(_rewardCardArea.GetChild(i).gameObject);
            }

            EncounterType enc = NodeEncounterType();
            IRandom rng = new RngStreams(_flow.Run.Seed).ForStream("reward_" + _flow.Run.CurrentNodeId);
            int offset = _flow.Run.RareOffset;
            List<CardData> reward = RewardSystem.RollCardReward(rng, enc, CharacterPools.RewardPool(_flow.Run.Character.Id), ref offset, 3);

            // 포션 보상 피티(전투 후). 획득 시 슬롯 보유(슬롯 UI는 후속).
            int potionChance = _flow.Run.PotionChance;
            var potion = RewardSystem.RollPotion(rng, PotionContent.All(), ref potionChance);
            _flow.Run.PotionChance = potionChance;
            if (potion != null)
            {
                _flow.Run.AddPotion(potion);
            }
            _flow.Run.RareOffset = offset;
            int goldReward = 10 + rng.NextInt(11);   // 전투 골드 보상(STS식 10~20)
            _flow.Run.Gold += goldReward;
            _rewardTitle.text = $"카드 보상 — 1장 선택 (또는 건너뛰기)   <size=68%>골드 +{goldReward}{(potion != null ? $" · 포션 +{potion.Name}" : "")}</size>";

            const float spacing = 280f;
            float startX = -(reward.Count - 1) * spacing / 2f;
            for (int i = 0; i < reward.Count; i++)
            {
                CreateRewardCard(reward[i], new Vector2(startX + i * spacing, 0f));
            }
        }

        private void CreateRewardCard(CardData card, Vector2 pos)
        {
            var go = new GameObject("Reward_" + card.Id, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_rewardCardArea, false);
            go.AddComponent<TooltipTrigger>().Set(GameInfo.CardDesc(card));
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(230f, 290f);
            go.GetComponent<Image>().color = RarityColor(card.Rarity);

            var t = new GameObject("L", typeof(RectTransform), typeof(TextMeshProUGUI));
            t.transform.SetParent(rt, false);
            var tmp = t.GetComponent<TextMeshProUGUI>();
            tmp.font = _font;
            tmp.fontSize = 24f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            tmp.text = $"[{card.Cost}] {card.Name}\n\n<size=70%>{RarityKor(card.Rarity)}</size>";
            var lrt = tmp.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(8, 8);
            lrt.offsetMax = new Vector2(-8, -8);

            CardData picked = card;
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                _flow.Run.AddCard(picked);
                _flow.OnRewardDone();
            });
        }

        private EncounterType NodeEncounterType()
        {
            MapNode node = _flow.Run.Map.GetNode(_flow.Run.CurrentNodeId);
            if (node == null) return EncounterType.Normal;
            if (node.Type == NodeType.Boss) return EncounterType.Boss;
            if (node.Type == NodeType.Elite) return EncounterType.Elite;
            return EncounterType.Normal;
        }

        private static Color RarityColor(CardRarity r)
        {
            switch (r)
            {
                case CardRarity.Rare: return new Color(0.60f, 0.50f, 0.15f);
                case CardRarity.Uncommon: return new Color(0.20f, 0.40f, 0.55f);
                default: return new Color(0.35f, 0.35f, 0.42f);
            }
        }

        private static string RarityKor(CardRarity r)
        {
            switch (r)
            {
                case CardRarity.Rare: return "희귀";
                case CardRarity.Uncommon: return "고급";
                default: return "일반";
            }
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
            tmp.enableAutoSizing = true;   // 긴 라벨(캐릭터 선택·휴식 옵션 등)을 버튼 안에 1줄로 맞춤
            tmp.fontSizeMin = 16f;
            tmp.fontSizeMax = 30f;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
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

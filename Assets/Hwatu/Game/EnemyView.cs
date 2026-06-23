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
        private Image _intentIcon;
        private TextMeshProUGUI _intentText;
        private TextMeshProUGUI _blockText;
        private RectTransform _enemyStatusArea;
        private static readonly StatusType[] s_statusOrder = { StatusType.Weak, StatusType.Vulnerable, StatusType.Poison, StatusType.Radiance, StatusType.Dexterity };

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
            irt.sizeDelta = new Vector2(264f, 48f);
            _intentBg = intentGo.GetComponent<Image>();
            _intentBg.raycastTarget = false;

            // 인텐트 아이콘(game-icons — 박스 왼쪽, IconSm 규격으로 크기 통일)
            var iconGo = new GameObject("IntentIcon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(irt, false);
            var iconRt = (RectTransform)iconGo.transform;
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = new Vector2(8f, 0f);
            _intentIcon = iconGo.GetComponent<Image>();
            _intentIcon.raycastTarget = false;
            _intentIcon.enabled = false;

            _intentText = CreateText(irt, "IntentText", 24f, new Vector2(0.5f, 0.5f), new Vector2(256f, 44f));
            _intentText.enableAutoSizing = true;   // 긴 인텐트(연속 히트·Doom 타이머)는 자동 축소해 박스 안에 맞춘다
            _intentText.fontSizeMin = 13f;
            _intentText.fontSizeMax = 24f;

            // 적 status 칩 영역(박스 아래, hover 설명 — STS2식)
            var estatGo = new GameObject("EnemyStatus", typeof(RectTransform));
            estatGo.transform.SetParent(transform, false);
            _enemyStatusArea = (RectTransform)estatGo.transform;
            _enemyStatusArea.anchorMin = new Vector2(0f, 0f);
            _enemyStatusArea.anchorMax = new Vector2(0f, 0f);
            _enemyStatusArea.pivot = new Vector2(0f, 1f);
            _enemyStatusArea.anchoredPosition = new Vector2(30f, -6f);
            _enemyStatusArea.sizeDelta = new Vector2(400f, 36f);

            // 방어도(Block) — HP바 위 좌측. 0이면 숨김(STS2식 방패 표기)
            _blockText = CreateText(transform, "Block", 22f, new Vector2(0.5f, 0.66f), new Vector2(160f, 30f));
            _blockText.rectTransform.anchoredPosition = new Vector2(-110f, 0f);
            _blockText.color = new Color(0.55f, 0.78f, 1f);
            _blockText.fontStyle = FontStyles.Bold;
            _blockText.alignment = TextAlignmentOptions.Left;
        }

        /// <summary>적 상태를 박스에 반영한다.</summary>
        public void Bind(EnemyState e, PlayerState player)
        {
            _nameText.text = e.Data.Name;
            _hpBar.Set(e.Hp, e.MaxHp);
            _blockText.text = e.Block > 0 ? $"방어 {e.Block}" : "";

            if (e.CurrentIntent != null)
            {
                _intentBg.enabled = true;
                _intentBg.color = IntentColor(e.CurrentIntent.Intent);
                // 공격류는 약화·취약·광을 반영한 "실제로 들어올" 데미지로 표시(EffectDispatcher와 같은 DamageMath 공유)
                IntentType it = e.CurrentIntent.Intent;
                bool isAttack = it == IntentType.Attack || it == IntentType.AttackMulti || it == IntentType.Doom;
                int shown = (isAttack && player != null)
                    ? DamageMath.RawDamage(e, player, e.CurrentIntent.Value)
                    : e.CurrentIntent.Value;
                string num = e.CurrentIntent.Hits > 1
                    ? $"{shown}×{e.CurrentIntent.Hits}"
                    : shown.ToString();
                string doomTag = (e.CurrentIntent.Intent == IntentType.Doom && e.DoomTimer > 0)
                    ? $"  <size=80%>({e.DoomTimer})</size>" : "";
                _intentText.text = $"{IntentKor(e.CurrentIntent.Intent)}  <b><size=135%>{num}</size></b>{doomTag}";
                IconLoader.Apply(this, _intentIcon, IconCatalog.ForIntent(it), Color.white, DesignTokens.IconSm);
            }
            else
            {
                _intentBg.enabled = false;
                _intentIcon.enabled = false;
                _intentText.text = "?";
            }
            RebuildEnemyStatus(e);
        }

        /// <summary>적 status를 박스 아래 칩으로 다시 그린다(active만, hover 설명).</summary>
        private void RebuildEnemyStatus(EnemyState e)
        {
            var kill = new System.Collections.Generic.List<GameObject>();
            foreach (Transform c in _enemyStatusArea) { kill.Add(c.gameObject); }
            foreach (var g in kill) { Destroy(g); }
            int idx = 0;
            foreach (StatusType st in s_statusOrder)
            {
                int amt = e.GetStatus(st);
                if (amt <= 0) { continue; }
                var chip = new GameObject($"St_{st}", typeof(RectTransform), typeof(Image), typeof(TooltipTrigger));
                chip.transform.SetParent(_enemyStatusArea, false);
                var crt = (RectTransform)chip.transform;
                crt.anchorMin = new Vector2(0f, 1f);
                crt.anchorMax = new Vector2(0f, 1f);
                crt.pivot = new Vector2(0f, 1f);
                crt.anchoredPosition = new Vector2(idx * 96f, 0f);
                crt.sizeDelta = new Vector2(90f, 34f);
                chip.GetComponent<Image>().color = DesignTokens.StatusColor(st);
                chip.GetComponent<TooltipTrigger>().Set(GameInfo.StatusDesc(st, amt));
                var lbl = CreateText((RectTransform)chip.transform, "L", 19f, new Vector2(0.5f, 0.5f), new Vector2(86f, 32f));
                lbl.enableAutoSizing = true;   // 큰 수치(취약 12 등)도 칩 안에 맞춤
                lbl.fontSizeMin = 12f;
                lbl.fontSizeMax = 19f;
                lbl.text = $"{GameInfo.StatusName(st)} {amt}";
                idx++;
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

        // 인텐트 박스 색 — 디자인 토큰(의미 색)으로 통일. Doom만 고유 흑보라.
        private static Color IntentColor(IntentType intent)
        {
            switch (intent)
            {
                case IntentType.Attack:
                case IntentType.AttackMulti: return DesignTokens.Danger;
                case IntentType.Block:       return DesignTokens.Defense;
                case IntentType.Buff:        return DesignTokens.Heal;
                case IntentType.Debuff:      return DesignTokens.Weak;
                case IntentType.Doom:        return new Color(0.12f, 0.02f, 0.16f, 0.98f);
                default:                     return DesignTokens.PanelHi;
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

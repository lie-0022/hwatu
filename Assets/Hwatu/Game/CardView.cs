using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using Hwatu.Core.Cards;
using Hwatu.Core.Effects;
using Hwatu.Core.Combat;

namespace Hwatu.Game
{
    /// <summary>
    /// 손패 카드 1장의 비주얼(코드 생성 uGUI). 배경색=카드 타입, 좌상단 코스트·우상단 타입 라벨·
    /// 중앙 아트 placeholder·중하단 이름·하단 효과. 값 채움은 <see cref="Bind"/>.
    /// 드래그/호버(서브시스템 2~3) 대비로 pivot=(0.5,0) + <see cref="Group"/>(CanvasGroup)을 가진다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>카드 기본 크기(px).</summary>
        public const float Width = 200f;
        public const float Height = 280f;

        private TMP_FontAsset _font;
        private Image _background;
        private TextMeshProUGUI _costText;
        private TextMeshProUGUI _typeText;
        private TextMeshProUGUI _nameText;
        private TextMeshProUGUI _descText;
        private Vector2 _homePos;
        private float _homeRot;
        private Vector2 _targetPos;
        private Vector2 _posVelocity;
        private Vector3 _targetScale = Vector3.one;
        private float _targetRot;
        private bool _hovered;
        private bool _dragging;
        private bool _used;
        private static bool s_anyDragging;
        private static readonly List<RaycastResult> s_raycastResults = new List<RaycastResult>();
        private bool _targeting;
        private TargetType _target;
        private CombatView _combat;

        private const float HoverScale = 1.18f;
        private const float HoverLift = 52f;
        private const float TweenSpeed = 12f;
        private const float SmoothTime = 0.14f;   // 위치 보간 시간(클수록 더 느리고 완만)
        private const float PlayThreshold = 80f;   // 비타깃 카드: 이 높이 이상으로 올려 놓아야 사용(아래는 취소)

        /// <summary>이 뷰가 표시 중인 카드 인스턴스.</summary>
        public CardInstance Card { get; private set; }

        /// <summary>드래그/호버 제어용 CanvasGroup(레이캐스트 차단 등).</summary>
        public CanvasGroup Group { get; private set; }

        /// <summary>비주얼 트리를 1회 생성한다(폰트 주입). <see cref="Bind"/> 전에 호출.</summary>
        public void Build(TMP_FontAsset font)
        {
            _font = font;

            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(Width, Height);

            Group = gameObject.AddComponent<CanvasGroup>();

            _background = gameObject.AddComponent<Image>();
            _background.color = Color.white;

            // 중앙 아트 placeholder
            Image art = CreateImage("Art", new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(172, 120));
            art.color = new Color(0.16f, 0.16f, 0.19f);

            // 좌상단 코스트 배지
            Image cost = CreateImage("CostBadge", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28, -28), new Vector2(46, 46));
            cost.color = new Color(0.10f, 0.32f, 0.58f);
            _costText = CreateText(cost.rectTransform, "CostText", Vector2.zero, Vector2.one, Vector2.zero, 26, TextAlignmentOptions.Center);

            // 우상단 타입 라벨
            _typeText = CreateText(rt, "TypeText", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48, -28), 18, TextAlignmentOptions.Center);
            _typeText.rectTransform.sizeDelta = new Vector2(82, 30);

            // 이름(아트 아래)
            _nameText = CreateText(rt, "NameText", new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), Vector2.zero, 24, TextAlignmentOptions.Center);
            _nameText.rectTransform.sizeDelta = new Vector2(180, 38);
            _nameText.enableAutoSizing = true;   // 강화(+)·인챈트 마크로 길어진 이름을 카드 폭에 맞게 축소
            _nameText.fontSizeMin = 15f;
            _nameText.fontSizeMax = 24f;

            // 효과(하단)
            _descText = CreateText(rt, "DescText", new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, 20, TextAlignmentOptions.Center);
            _descText.rectTransform.sizeDelta = new Vector2(176, 64);
            _descText.textWrappingMode = TextWrappingModes.Normal;
            _descText.enableAutoSizing = true;   // 긴 효과 설명도 카드 안에 맞게 축소
            _descText.fontSizeMin = 13f;
            _descText.fontSizeMax = 20f;
        }

        /// <summary>카드 데이터를 비주얼에 반영한다. playable=false면 어둡게 표시.</summary>
        public void Bind(CardInstance card, bool playable)
        {
            Card = card;
            CardData d = card.Data;
            _target = d.Target;

            _background.color = TypeColor(d.Type, playable);
            _costText.text = d.Cost.ToString();
            _typeText.text = TypeKor(d.Type);
            _nameText.text = d.Name;
            _descText.text = Describe(d);
            // 카드 자체에 이름·효과가 적혀 있어 hover 툴팁은 두지 않는다(손패에서 카드를 가리는 문제 회피).

            float a = playable ? 1f : 0.6f;
            _nameText.color = new Color(1f, 1f, 1f, a);
            _descText.color = new Color(0.92f, 0.92f, 0.92f, a);
            _typeText.color = new Color(1f, 1f, 1f, a * 0.85f);
        }

        // ── 손패 배치 / 호버 ──

        /// <summary>화살표 제어 등을 위해 소속 CombatView를 주입한다.</summary>
        public void SetCombat(CombatView combat)
        {
            _combat = combat;
        }

        /// <summary>손패에서의 기준 위치·회전을 지정한다(부채꼴 배치). 호버 해제 시 이 값으로 복귀.</summary>
        public void SetHome(Vector2 anchoredPosition, float rotationZ)
        {
            _homePos = anchoredPosition;
            _homeRot = rotationZ;

            // 즉시 이동하지 않고 목표만 갱신 → Update에서 스르륵 보간(손패 재배치 부드럽게)
            if (!_dragging && !_hovered)
            {
                _targetPos = anchoredPosition;
                _targetRot = rotationZ;
                _targetScale = Vector3.one;
            }
        }

        /// <summary>마우스 진입: 똑바로 세워 확대 + 위로 + 맨 앞으로(목표만 설정, 보간은 Update).</summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (s_anyDragging)
            {
                return;
            }
            _hovered = true;
            _targetPos = _homePos + new Vector2(0f, HoverLift);
            _targetRot = 0f;
            _targetScale = new Vector3(HoverScale, HoverScale, 1f);
            transform.SetAsLastSibling();
        }

        /// <summary>마우스 이탈: 기준 위치·회전·크기로 복귀(목표만 설정).</summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (s_anyDragging)
            {
                return;
            }
            _hovered = false;
            _targetPos = _homePos;
            _targetRot = _homeRot;
            _targetScale = Vector3.one;
        }

        // ── 드래그(서브3: 집기 / 따라오기 / 복귀) ──

        /// <summary>드래그 시작: 손패에서 들어 올려 똑바로, 레이캐스트 통과(드롭 대상 판정용).</summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
            s_anyDragging = true;
            _hovered = false;
            _targeting = _target == TargetType.Enemy;
            _targetScale = new Vector3(HoverScale, HoverScale, 1f);
            _targetRot = 0f;
            transform.SetAsLastSibling();
            if (Group != null)
            {
                Group.blocksRaycasts = false;
            }
            if (_targeting)
            {
                _targetPos = new Vector2(0f, 120f);   // STS2: 공격 카드는 손패 가운데로, 손 가까이 내려옴
                if (_combat != null)
                {
                    _combat.Arrow.SetVisible(true);
                }
            }
            else if (_combat != null)
            {
                _combat.SetPlayerHighlight(true);   // 방어/비타깃: 플레이어 강조
            }
        }

        /// <summary>드래그 중: 카드를 커서 위치로(부모 로컬 좌표로 변환).</summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
            {
                return;   // 우클릭으로 취소된 뒤에는 카드를 더 움직이지 않음
            }
            if (_targeting)
            {
                if (_combat != null)
                {
                    _combat.Arrow.UpdateArc(CardTopScreen(), eventData.position);
                    _combat.SetEnemyHighlight(FindEnemyUnderPointer(eventData));
                }
                return;
            }

            var parent = (RectTransform)transform.parent;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, eventData.position, eventData.pressEventCamera, out Vector2 local))
            {
                ((RectTransform)transform).anchoredPosition = local;
            }
        }

        // 커서 아래의 적 박스를 찾는다(없으면 null).
        private static EnemyView FindEnemyUnderPointer(PointerEventData eventData)
        {
            if (EventSystem.current == null)
            {
                return null;
            }
            s_raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, s_raycastResults);
            for (int i = 0; i < s_raycastResults.Count; i++)
            {
                EnemyView enemy = s_raycastResults[i].gameObject.GetComponentInParent<EnemyView>();
                if (enemy != null)
                {
                    return enemy;
                }
            }
            return null;
        }

        // 카드 상단 중앙의 화면 좌표(화살표 시작점).
        private Vector2 CardTopScreen()
        {
            Vector3 p = transform.position;
            p.y += Height * 0.5f;
            return p;
        }

        /// <summary>드래그 종료: (서브4/5에서 타깃 판정 예정) 지금은 손패로 복귀.</summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
            {
                return;   // 우클릭으로 이미 취소됨
            }
            _dragging = false;
            s_anyDragging = false;
            _targeting = false;
            if (Group != null)
            {
                Group.blocksRaycasts = true;
            }
            if (_combat != null)
            {
                _combat.Arrow.SetVisible(false);
                _combat.SetEnemyHighlight(null);
                _combat.SetPlayerHighlight(false);
            }

            // 비타깃 카드(방어/스킬): 손패보다 충분히 위(플레이 영역)에서 놓으면 사용, 손패 근처면 취소.
            // 타깃 카드는 서브5에서 적 위 판정 예정.
            if (_target != TargetType.Enemy)
            {
                float releaseY = ((RectTransform)transform).anchoredPosition.y;
                if (releaseY > PlayThreshold && _combat != null && _combat.TryPlayCard(this, -1))
                {
                    return;
                }
            }
            else
            {
                EnemyView enemy = FindEnemyUnderPointer(eventData);
                if (enemy != null && _combat != null && _combat.TryPlayCard(this, enemy.Index))
                {
                    return;
                }
            }

            _targetPos = _homePos;
            _targetRot = _homeRot;
            _targetScale = Vector3.one;
        }

        // 드래그를 취소하고 손패 제자리로 되돌린다(우클릭).
        private void CancelDrag()
        {
            _dragging = false;
            s_anyDragging = false;
            _targeting = false;
            if (Group != null)
            {
                Group.blocksRaycasts = true;
            }
            if (_combat != null)
            {
                _combat.Arrow.SetVisible(false);
                _combat.SetEnemyHighlight(null);
                _combat.SetPlayerHighlight(false);
            }
            _targetPos = _homePos;
            _targetRot = _homeRot;
            _targetScale = Vector3.one;
        }

        /// <summary>카드 사용 연출: 위로 날아오르며 페이드아웃한 뒤 소멸한다.</summary>
        public void PlayUseAnimation()
        {
            _used = true;
            _hovered = false;
            _dragging = false;
            if (Group != null)
            {
                Group.blocksRaycasts = false;
            }
            transform.SetAsLastSibling();
        }

        // 매 프레임 목표(위치·크기·회전)로 부드럽게 보간(스르륵).
        private void Update()
        {
            if (_used)
            {
                var urt = (RectTransform)transform;
                urt.anchoredPosition += new Vector2(0f, 620f * Time.deltaTime);
                urt.localScale *= 1f + Time.deltaTime * 1.2f;
                if (Group != null)
                {
                    Group.alpha -= Time.deltaTime * 2.6f;
                    if (Group.alpha <= 0f)
                    {
                        Destroy(gameObject);
                    }
                }
                return;
            }

            if (_dragging && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelDrag();
            }

            if (s_anyDragging && !_dragging && _hovered)
            {
                // 다른 카드를 드래그하는 동안에는 이 카드의 호버를 해제(잔상 방지).
                _hovered = false;
                _targetPos = _homePos;
                _targetRot = _homeRot;
                _targetScale = Vector3.one;
            }

            var rt = (RectTransform)transform;
            float t = Time.deltaTime * TweenSpeed;
            bool followCursor = _dragging && !_targeting;   // 비타깃 드래그만 커서를 직접 따라감
            if (!followCursor)
            {
                rt.anchoredPosition = Vector2.SmoothDamp(rt.anchoredPosition, _targetPos, ref _posVelocity, SmoothTime);
            }
            rt.localScale = Vector3.Lerp(rt.localScale, _targetScale, t);
            float z = Mathf.LerpAngle(rt.localEulerAngles.z, _targetRot, t);
            rt.localRotation = Quaternion.Euler(0f, 0f, z);
        }

        // ── 타입 → 색 / 라벨 ──

        private static Color TypeColor(CardType type, bool playable)
        {
            Color c;
            switch (type)
            {
                case CardType.Attack: c = new Color(0.62f, 0.22f, 0.20f); break; // 주홍
                case CardType.Skill:  c = new Color(0.16f, 0.40f, 0.44f); break; // 청록
                case CardType.Power:  c = new Color(0.60f, 0.46f, 0.16f); break; // 황금
                default:              c = new Color(0.30f, 0.30f, 0.32f); break; // 상태/저주
            }
            if (!playable)
            {
                c = new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, 1f);
            }
            return c;
        }

        private static string TypeKor(CardType type)
        {
            switch (type)
            {
                case CardType.Attack: return "공격";
                case CardType.Skill:  return "스킬";
                case CardType.Power:  return "파워";
                case CardType.Status: return "상태";
                case CardType.Curse:  return "저주";
                default: return "";
            }
        }

        private static string Describe(CardData d)
        {
            var parts = new System.Collections.Generic.List<string>();
            foreach (EffectData e in d.Effects)
            {
                switch (e.Op)
                {
                    case EffectOp.DealDamage:   parts.Add($"{e.Amount} 피해"); break;
                    case EffectOp.GainBlock:    parts.Add($"{e.Amount} 방어"); break;
                    case EffectOp.GainResource: parts.Add($"광 +{e.Amount}"); break;
                    case EffectOp.Draw:         parts.Add($"{e.Amount}장 뽑기"); break;
                    case EffectOp.ApplyStatus:  parts.Add($"{StatusKor(e.Status)} {e.Amount}"); break;
                    case EffectOp.ClearStatus:  parts.Add($"{StatusKor(e.Status)} 제거"); break;
                    default:                    parts.Add(e.Op.ToString()); break;
                }
            }
            var kw = new System.Collections.Generic.List<string>();
            if (d.Innate) { kw.Add("선제"); }
            if (d.Retain) { kw.Add("유지"); }
            if (d.Exhaust) { kw.Add("소멸"); }
            if (d.Ethereal) { kw.Add("휘발"); }

            string body = parts.Count > 0 ? string.Join(", ", parts) : "-";
            if (kw.Count > 0)
            {
                body += $"\n<size=78%><color=#C9B98C>[{string.Join("·", kw)}]</color></size>";
            }
            return body;
        }

        /// <summary>StatusType을 카드 설명용 한글로.</summary>
        private static string StatusKor(StatusType s)
        {
            switch (s)
            {
                case StatusType.Poison:     return "중독";
                case StatusType.Weak:       return "약화";
                case StatusType.Vulnerable: return "취약";
                case StatusType.Radiance:   return "광";
                case StatusType.Dexterity:  return "민첩";
                default:                    return s.ToString();
            }
        }

        // ── UI 생성 헬퍼 ──

        private Image CreateImage(string name, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go.GetComponent<Image>();
        }

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
            rt.sizeDelta = new Vector2(100, 40);
            return t;
        }
    }
}

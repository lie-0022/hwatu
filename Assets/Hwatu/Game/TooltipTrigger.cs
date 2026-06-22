using UnityEngine;
using UnityEngine.EventSystems;

namespace Hwatu.Game
{
    /// <summary>
    /// UI 요소(Image/Button 등 raycastTarget)에 붙여 hover 시 <see cref="TooltipUI"/>에 설명을 띄운다.
    /// 유물 칩·status 칩·포션 버튼·카드 키워드 등 "정보가 필요한 모든 것"에 부착.
    /// </summary>
    public sealed class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private string _description;
        private bool _showing;

        /// <summary>hover 시 표시할 설명을 설정한다(런타임 갱신 가능).</summary>
        public void Set(string description)
        {
            _description = description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _showing = true;
            TooltipUI.Show(_description);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _showing = false;
            TooltipUI.Hide();
        }

        // 클릭으로 화면이 전환돼 이 GameObject가 비활성/파괴되면 Unity는 OnPointerExit를 호출하지 않는다.
        // → hover 중이던 툴팁이 화면에 남는 문제를, 자기가 띄운 경우(_showing)에 한해 OnDisable에서 직접 숨겨 막는다.
        private void OnDisable()
        {
            if (_showing)
            {
                _showing = false;
                TooltipUI.Hide();
            }
        }
    }
}

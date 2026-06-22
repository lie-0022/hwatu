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

        /// <summary>hover 시 표시할 설명을 설정한다(런타임 갱신 가능).</summary>
        public void Set(string description)
        {
            _description = description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipUI.Show(_description);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipUI.Hide();
        }
    }
}

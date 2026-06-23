using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Content;
using Hwatu.Game;

namespace Hwatu.Tests.GameUi
{
    /// <summary>효과·status 표시 전수 — 신규 status/op이 영문 fallback으로 새지 않는지(미표시 회귀 방지).</summary>
    public class GameInfoStatusTests
    {
        [Test]
        public void StatusName_AllStatuses_HaveKorean()
        {
            foreach (StatusType s in System.Enum.GetValues(typeof(StatusType)))
            {
                Assert.AreNotEqual(s.ToString(), GameInfo.StatusName(s), $"{s} 한글명 누락(영문 fallback)");
            }
        }

        [Test]
        public void StatusDesc_AllStatuses_HaveBoldDesc()
        {
            foreach (StatusType s in System.Enum.GetValues(typeof(StatusType)))
            {
                Assert.IsTrue(GameInfo.StatusDesc(s, 3).Contains("<b>"), $"{s} 설명 누락");
            }
        }

        [Test]
        public void CardDesc_ConsumeAndMultiply_NoRawOpName()
        {
            Assert.IsFalse(GameInfo.CardDesc(LuminaryCards.RadiantNova()).Contains("consume_radiance"), "광폭발 효과가 영문 op로 노출");
            Assert.IsFalse(GameInfo.CardDesc(InkCards.Catalyst()).Contains("multiply_poison"), "촉매 효과가 영문 op로 노출");
        }
    }
}

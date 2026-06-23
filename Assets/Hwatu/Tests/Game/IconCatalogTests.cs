using NUnit.Framework;
using Hwatu.Core.Combat;
using Hwatu.Core.Enemies;
using Hwatu.Game;

namespace Hwatu.Tests.GameUi
{
    /// <summary>아이콘 매핑(IconCatalog) + 디자인 토큰 색 — 누락 없음·시각 구분 검증.</summary>
    public class IconCatalogTests
    {
        [Test]
        public void EveryStatus_HasIconName()
        {
            foreach (StatusType s in System.Enum.GetValues(typeof(StatusType)))
            {
                Assert.IsFalse(string.IsNullOrEmpty(IconCatalog.ForStatus(s)), $"{s} 아이콘 매핑 누락");
            }
        }

        [Test]
        public void EveryIntent_HasIconName()
        {
            foreach (IntentType it in System.Enum.GetValues(typeof(IntentType)))
            {
                Assert.IsFalse(string.IsNullOrEmpty(IconCatalog.ForIntent(it)), $"{it} 아이콘 매핑 누락");
            }
        }

        [Test]
        public void StatusColor_DiffersFromTextMain_ForKeyStatuses()
        {
            Assert.AreNotEqual(DesignTokens.TextMain, DesignTokens.StatusColor(StatusType.Poison));
            Assert.AreNotEqual(DesignTokens.TextMain, DesignTokens.StatusColor(StatusType.Radiance));
            Assert.AreNotEqual(DesignTokens.TextMain, DesignTokens.StatusColor(StatusType.Majesty));
        }
    }
}

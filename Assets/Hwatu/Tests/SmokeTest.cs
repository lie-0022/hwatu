using NUnit.Framework;
using Hwatu.Core;

namespace Hwatu.Tests
{
    /// <summary>
    /// 1단계 scaffold 검증: Hwatu.Core + Hwatu.Tests 어셈블리가 컴파일되고,
    /// Tests가 Core를 참조하며, EditMode 테스트 러너가 동작한다.
    /// </summary>
    public class SmokeTest
    {
        [Test]
        public void Scaffold_CompilesAndCoreIsReferenced()
        {
            Assert.AreEqual("Hwatu.Core", CoreAssemblyMarker.Name);
        }
    }
}

using System.Collections.Generic;

namespace Hwatu.Core.Rng
{
    /// <summary>
    /// 결정론 난수 소스. 시드가 같으면 항상 같은 수열과 셔플 결과를 낸다.
    /// 전투 셔플·적 HP 롤 등 재현이 필요한 모든 무작위에 주입해서 사용한다.
    /// </summary>
    public interface IRandom
    {
        /// <summary>[0, maxExclusive) 범위의 정수. maxExclusive가 0 이하면 0.</summary>
        int NextInt(int maxExclusive);

        /// <summary>Fisher-Yates 제자리 셔플(결정론).</summary>
        void Shuffle<T>(IList<T> list);
    }
}

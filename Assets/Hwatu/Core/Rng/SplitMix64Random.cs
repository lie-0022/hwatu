using System.Collections.Generic;

namespace Hwatu.Core.Rng
{
    /// <summary>
    /// SplitMix64(Vigna) 기반 결정론 난수. <see cref="System.Random"/>과 달리
    /// .NET 런타임·플랫폼이 달라도 동일 시드 → 동일 수열을 보장한다(버그 재현·시드 공유 목적).
    /// </summary>
    public sealed class SplitMix64Random : IRandom
    {
        private ulong _state;

        public SplitMix64Random(ulong seed)
        {
            _state = seed;
        }

        /// <summary>다음 64비트 난수.</summary>
        public ulong NextULong()
        {
            _state += 0x9E3779B97F4A7C15UL;
            ulong z = _state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                return 0;
            }

            return (int)(NextULong() % (ulong)maxExclusive);
        }

        public void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = NextInt(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}

namespace Hwatu.Core.Rng
{
    /// <summary>
    /// 마스터 시드 하나에서 이름별 독립 난수 스트림을 파생한다(예: "combatShuffle", "enemyHp").
    /// 같은 (마스터시드, 이름)은 항상 같은 스트림을 만들고, 서로 다른 이름의 스트림은 독립적이다.
    /// 목적: 한 런(run) 안에서 영역별 무작위를 분리해 재현성과 디버깅을 보장(SPEC §3.4).
    /// </summary>
    public sealed class RngStreams
    {
        private readonly ulong _masterSeed;

        public RngStreams(ulong masterSeed)
        {
            _masterSeed = masterSeed;
        }

        /// <summary>이름으로 결정론 하위 스트림을 만든다.</summary>
        public IRandom ForStream(string name)
        {
            ulong seed = Mix(_masterSeed, Hash(name));
            return new SplitMix64Random(seed);
        }

        // FNV-1a 64bit — 결정론, 플랫폼 무관 문자열 해시.
        private static ulong Hash(string s)
        {
            ulong hash = 1469598103934665603UL;
            for (int i = 0; i < s.Length; i++)
            {
                hash ^= s[i];
                hash *= 1099511628211UL;
            }
            return hash;
        }

        // 마스터시드와 이름 해시를 섞어 스트림 시드를 만든다(SplitMix64 finalizer).
        private static ulong Mix(ulong a, ulong b)
        {
            ulong z = a ^ (b + 0x9E3779B97F4A7C15UL + (a << 6) + (a >> 2));
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }
}

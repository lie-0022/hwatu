using System.Collections.Generic;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Core.Content
{
    /// <summary>이벤트 노드 정의 + 시드 선택. 효과는 RunState를 직접 변경한다(UI 연결은 RunUI 후속).</summary>
    public static class EventContent
    {
        // 달빛 옹달샘: 회복 vs 골드
        public static EventData Spring() => new EventData(
            "spring", "달빛 옹달샘", "고요한 샘이 달빛을 머금고 있다.",
            new[]
            {
                new EventChoice("마신다 (HP +12)", "샘물이 상처를 달랜다.", run => run.Heal(12)),
                new EventChoice("바닥을 뒤진다 (골드 +25)", "엽전 한 줌을 줍는다.", run => run.Gold += 25),
            });

        // 도깨비의 거래: 골드로 최대 HP
        public static EventData Bargain() => new EventData(
            "bargain", "도깨비의 거래", "도깨비가 이를 드러내며 흥정을 건다.",
            new[]
            {
                new EventChoice("받아들인다 (골드 -30, 최대 HP +8)", "기운이 깃든다.",
                    run => { if (run.TrySpend(30)) { run.MaxHp += 8; run.Heal(8); } }),
                new EventChoice("거절한다", "도깨비가 연기처럼 사라진다.", run => { }),
            });

        public static List<EventData> All()
        {
            return new List<EventData> { Spring(), Bargain() };
        }

        /// <summary>시드로 이벤트 1개 선택.</summary>
        public static EventData Pick(IRandom rng)
        {
            List<EventData> all = All();
            return all[rng.NextInt(all.Count)];
        }
    }
}

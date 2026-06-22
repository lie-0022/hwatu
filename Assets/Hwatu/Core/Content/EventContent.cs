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

        public static EventData Forge() => new EventData(
            "forge", "오래된 대장간", "식지 않은 화로가 손에 든 카드를 벼릴 수 있다.",
            new[]
            {
                new EventChoice("첫 카드를 벼린다 (공격/방어 +2 영구)", "쇳소리와 함께 날이 선다.",
                    run => run.EnchantCard(0, "sharp")),
                new EventChoice("강하게 벼린다 (공격/방어 +4, 단 1회 쓰면 소멸)", "강철빛이 돌지만 한 번 쓰면 부서진다.",
                    run => run.EnchantCard(0, "brittle")),
                new EventChoice("그냥 지나간다", "화로가 식어간다.", run => { }),
            });

        public static EventData Curse() => new EventData(
            "curse", "그믐의 속삭임", "어둠이 힘을 빌려주겠다 속삭인다. 대가가 따른다.",
            new[]
            {
                new EventChoice("힘을 받는다 (골드 +80, 최대 HP -6)", "살점을 내주고 힘을 얻는다.",
                    run => { run.Gold += 80; run.MaxHp -= 6; if (run.Hp > run.MaxHp) { run.Hp = run.MaxHp; } }),
                new EventChoice("거절한다", "속삭임이 잦아든다.", run => { }),
            });

        public static List<EventData> All()
        {
            return new List<EventData> { Spring(), Bargain(), Forge(), Curse() };
        }

        /// <summary>시드로 이벤트 1개 선택.</summary>
        public static EventData Pick(IRandom rng)
        {
            List<EventData> all = All();
            return all[rng.NextInt(all.Count)];
        }
    }
}

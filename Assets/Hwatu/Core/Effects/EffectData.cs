using Hwatu.Core.Cards;
using Hwatu.Core.Combat;

namespace Hwatu.Core.Effects
{
    /// <summary>
    /// 효과 원자(불변 POCO). 카드와 적 move가 같은 타입을 공유한다.
    /// <see cref="EffectDispatcher"/>가 <see cref="Op"/>로 분기해 실행한다.
    /// </summary>
    public sealed class EffectData
    {
        public string Op { get; }
        public int Amount { get; }
        public TargetType Target { get; }
        public StatusType Status { get; }
        public ResourceType Resource { get; }

        public EffectData(
            string op,
            int amount = 0,
            TargetType target = TargetType.Enemy,
            StatusType status = StatusType.Radiance,
            ResourceType resource = ResourceType.Radiance)
        {
            Op = op;
            Amount = amount;
            Target = target;
            Status = status;
            Resource = resource;
        }
    }
}

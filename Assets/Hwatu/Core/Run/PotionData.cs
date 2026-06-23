using System;
using Hwatu.Core.Combat;

namespace Hwatu.Core.Run
{
    /// <summary>포션 사용 대상 구분. Self=플레이어 버프/회복, Enemy=적 대상 전술.</summary>
    public enum PotionTarget { Self, Enemy }

    /// <summary>포션(일회용 소모품). 전투 중 사용 시 즉시 효과(SPEC 보상 포션 피티).</summary>
    public sealed class PotionData
    {
        private readonly Action<CombatState, int> _apply;
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        /// <summary>Self=플레이어 대상, Enemy=적 대상.</summary>
        public PotionTarget Target { get; }

        public PotionData(string id, string name, string description, PotionTarget target, Action<CombatState, int> apply)
        {
            Id = id;
            Name = name;
            Description = description;
            Target = target;
            _apply = apply;
        }

        /// <summary>전투 중 즉시 효과 적용.
        /// <param name="state">현재 전투 상태.</param>
        /// <param name="enemyTargetIndex">Enemy 포션 대상 적 인덱스. 범위 밖이거나 해당 적이 이미 죽었으면 첫 생존 적으로 폴백.</param>
        /// </summary>
        public void Apply(CombatState state, int enemyTargetIndex = 0)
        {
            _apply?.Invoke(state, enemyTargetIndex);
        }
    }
}

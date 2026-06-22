using System;
using Hwatu.Core.Combat;

namespace Hwatu.Core.Run
{
    /// <summary>포션(일회용 소모품). 전투 중 사용 시 PlayerState에 즉시 효과(SPEC 보상 포션 피티).</summary>
    public sealed class PotionData
    {
        private readonly Action<CombatState> _apply;
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }

        public PotionData(string id, string name, string description, Action<CombatState> apply)
        {
            Id = id;
            Name = name;
            Description = description;
            _apply = apply;
        }

        /// <summary>전투 중 즉시 효과 적용(플레이어·적 모두 접근 가능).</summary>
        public void Apply(CombatState state)
        {
            _apply?.Invoke(state);
        }
    }
}

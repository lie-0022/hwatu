using System;
using Hwatu.Core.Combat;

namespace Hwatu.Core.Run
{
    /// <summary>포션(일회용 소모품). 전투 중 사용 시 PlayerState에 즉시 효과(SPEC 보상 포션 피티).</summary>
    public sealed class PotionData
    {
        private readonly Action<PlayerState> _apply;
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }

        public PotionData(string id, string name, string description, Action<PlayerState> apply)
        {
            Id = id;
            Name = name;
            Description = description;
            _apply = apply;
        }

        /// <summary>전투 중 즉시 효과 적용.</summary>
        public void Apply(PlayerState p)
        {
            _apply?.Invoke(p);
        }
    }
}

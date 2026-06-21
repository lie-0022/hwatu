using System.Collections.Generic;

namespace Hwatu.Core.Combat
{
    /// <summary>플레이어의 런타임 상태(<see cref="ICombatant"/>). HP/Block/Energy/상태/자원.</summary>
    public sealed class PlayerState : ICombatant
    {
        private readonly Dictionary<StatusType, int> _statuses = new Dictionary<StatusType, int>();
        private readonly Dictionary<ResourceType, int> _resources = new Dictionary<ResourceType, int>();

        public PlayerState(int maxHp, int baseEnergy = 3, int handSize = 5)
        {
            MaxHp = maxHp;
            Hp = maxHp;
            BaseEnergy = baseEnergy;
            HandSize = handSize;
        }

        public int MaxHp { get; }
        public int Hp { get; private set; }
        public int Block { get; private set; }
        public int Energy { get; internal set; }
        public int BaseEnergy { get; }
        public int HandSize { get; }

        public void SetHp(int value) => Hp = value;
        public void SetBlock(int value) => Block = value;
        public int GetStatus(StatusType status) => _statuses.TryGetValue(status, out var v) ? v : 0;
        public void AddStatus(StatusType status, int amount) => _statuses[status] = GetStatus(status) + amount;
        public int GetResource(ResourceType resource) => _resources.TryGetValue(resource, out var v) ? v : 0;
    }
}

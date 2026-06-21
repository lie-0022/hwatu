using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Effects;
using Hwatu.Core.Rng;

namespace Hwatu.Tests.Combat
{
    /// <summary>테스트용 최소 ICombatant 구현.</summary>
    internal sealed class TestCombatant : ICombatant
    {
        private readonly Dictionary<StatusType, int> _statuses = new Dictionary<StatusType, int>();

        public TestCombatant(int hp = 50, int block = 0)
        {
            Hp = hp;
            Block = block;
        }

        public int Hp { get; private set; }
        public int Block { get; private set; }
        public void SetHp(int value) => Hp = value;
        public void SetBlock(int value) => Block = value;
        public int GetStatus(StatusType status) => _statuses.TryGetValue(status, out var v) ? v : 0;
        public void AddStatus(StatusType status, int amount) => _statuses[status] = GetStatus(status) + amount;
    }

    /// <summary>테스트용 IEffectContext 구현.</summary>
    internal sealed class TestEffectContext : IEffectContext
    {
        public ICombatant Source { get; set; }
        public ICombatant Target { get; set; }
        public List<CardInstance> Hand { get; set; } = new List<CardInstance>();
        public List<CardInstance> DrawPile { get; set; } = new List<CardInstance>();
        public List<CardInstance> DiscardPile { get; set; } = new List<CardInstance>();
        public IRandom ShuffleRng { get; set; } = new SplitMix64Random(1UL);
    }
}

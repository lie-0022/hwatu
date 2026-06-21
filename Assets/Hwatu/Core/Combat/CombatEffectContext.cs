using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Effects;
using Hwatu.Core.Rng;

namespace Hwatu.Core.Combat
{
    /// <summary>CombatState를 효과 실행용 <see cref="IEffectContext"/>로 감싼다. Source/Target은 시전자/대상.</summary>
    internal sealed class CombatEffectContext : IEffectContext
    {
        private readonly CombatState _state;

        public CombatEffectContext(CombatState state, ICombatant source, ICombatant target)
        {
            _state = state;
            Source = source;
            Target = target;
        }

        public ICombatant Source { get; }
        public ICombatant Target { get; }
        public List<CardInstance> Hand => _state.Hand;
        public List<CardInstance> DrawPile => _state.DrawPile;
        public List<CardInstance> DiscardPile => _state.DiscardPile;
        public IRandom ShuffleRng => _state.ShuffleRng;
    }
}

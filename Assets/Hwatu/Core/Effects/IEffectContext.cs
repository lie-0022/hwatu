using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Combat;
using Hwatu.Core.Rng;

namespace Hwatu.Core.Effects
{
    /// <summary>
    /// 효과 실행 컨텍스트. Source=시전자(공격자, radiance 제공), Target=대상(vulnerable 보유).
    /// Self 효과는 Source로 리졸브한다. draw op은 Hand/DrawPile/DiscardPile/ShuffleRng를 사용.
    /// </summary>
    public interface IEffectContext
    {
        ICombatant Source { get; }
        ICombatant Target { get; }
        List<CardInstance> Hand { get; }
        List<CardInstance> DrawPile { get; }
        List<CardInstance> DiscardPile { get; }
        IRandom ShuffleRng { get; }
    }
}

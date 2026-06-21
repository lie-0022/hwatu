using System.Collections.Generic;
using Hwatu.Core.Effects;

namespace Hwatu.Core.Enemies
{
    /// <summary>적의 한 행동(불변). intent + 표시값 + 실행 효과 리스트.</summary>
    public sealed class EnemyMoveData
    {
        public string Id { get; }
        public IntentType Intent { get; }
        public int Value { get; }   // 완전정보 UI 표시용(예: 공격 7)
        public IReadOnlyList<EffectData> Effects { get; }

        public EnemyMoveData(string id, IntentType intent, int value, IReadOnlyList<EffectData> effects)
        {
            Id = id;
            Intent = intent;
            Value = value;
            Effects = effects;
        }
    }
}

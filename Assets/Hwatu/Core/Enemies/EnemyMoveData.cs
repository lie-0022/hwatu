using System.Collections.Generic;
using Hwatu.Core.Effects;

namespace Hwatu.Core.Enemies
{
    /// <summary>적의 한 행동(불변). intent + 표시값 + 실행 효과 리스트.</summary>
    public sealed class EnemyMoveData
    {
        public string Id { get; }
        public IntentType Intent { get; }
        public int Value { get; }   // 완전정보 UI 표시용(예: 공격 7; 다회면 1회 피해)
        public int Hits { get; }    // 공격 횟수(다회공격 N×M 표시; 기본 1)
        public IReadOnlyList<EffectData> Effects { get; }

        public EnemyMoveData(string id, IntentType intent, int value, IReadOnlyList<EffectData> effects, int hits = 1)
        {
            Id = id;
            Intent = intent;
            Value = value;
            Effects = effects;
            Hits = hits;
        }
    }
}

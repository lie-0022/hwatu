using System.Collections.Generic;

namespace Hwatu.Core.Enemies
{
    /// <summary>적 정의(불변). HP 범위 롤 + move 목록 + AI 순서.</summary>
    public sealed class EnemyData
    {
        public string Id { get; }
        public string Name { get; }
        public int MaxHpMin { get; }
        public int MaxHpMax { get; }
        public IReadOnlyList<EnemyMoveData> Moves { get; }
        public EnemyAiKind AiKind { get; }
        public IReadOnlyList<string> AiOrder { get; }
        public IReadOnlyList<string> SecondPhaseOrder { get; }   // Phase AI 2페이즈 순서(없으면 null)

        public EnemyData(string id, string name, int maxHpMin, int maxHpMax,
            IReadOnlyList<EnemyMoveData> moves, EnemyAiKind aiKind, IReadOnlyList<string> aiOrder,
            IReadOnlyList<string> secondPhaseOrder = null)
        {
            Id = id;
            Name = name;
            MaxHpMin = maxHpMin;
            MaxHpMax = maxHpMax;
            Moves = moves;
            AiKind = aiKind;
            AiOrder = aiOrder;
            SecondPhaseOrder = secondPhaseOrder;
        }

        /// <summary>HP 범위에 배율을 적용한 복제본(2막+ 난이도 스케일). 다른 필드는 공유.</summary>
        public EnemyData WithHpScale(double mult)
        {
            return new EnemyData(Id, Name,
                (int)System.Math.Round(MaxHpMin * mult),
                (int)System.Math.Round(MaxHpMax * mult),
                Moves, AiKind, AiOrder, SecondPhaseOrder);
        }

        /// <summary>id로 move를 찾는다. 없으면 null.</summary>
        public EnemyMoveData FindMove(string id)
        {
            for (int i = 0; i < Moves.Count; i++)
            {
                if (Moves[i].Id == id)
                {
                    return Moves[i];
                }
            }
            return null;
        }
    }
}

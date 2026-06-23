using System;
using Hwatu.Core.Combat;

namespace Hwatu.Core.Run
{
    /// <summary>
    /// 유물 정의. MVP는 전투 시작 훅(플레이어에 효과)만 지원한다.
    /// 후속: 턴 시작·처치·피격 등 다양한 훅으로 확장.
    /// </summary>
    public sealed class RelicData
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }

        private readonly Action<PlayerState> _onCombatStart;
        private readonly Action<PlayerState> _onTurnStart;
        private readonly Action<PlayerState> _onCardPlay;

        public RelicData(string id, string name, string description,
            Action<PlayerState> onCombatStart = null, Action<PlayerState> onTurnStart = null,
            Action<PlayerState> onCardPlay = null)
        {
            Id = id;
            Name = name;
            Description = description;
            _onCombatStart = onCombatStart;
            _onTurnStart = onTurnStart;
            _onCardPlay = onCardPlay;
        }

        /// <summary>전투 시작 시 플레이어에 효과 적용(없으면 무동작).</summary>
        public void ApplyCombatStart(PlayerState player)
        {
            _onCombatStart?.Invoke(player);
        }

        /// <summary>매 플레이어 턴 시작 시 효과 적용(없으면 무동작). 트리거 다양성(연구 §3.2).</summary>
        public void ApplyTurnStart(PlayerState player)
        {
            _onTurnStart?.Invoke(player);
        }

        /// <summary>카드 1장 플레이할 때마다 효과 적용(없으면 무동작). 트리거 다양성(연구 §3.2).</summary>
        public void ApplyCardPlay(PlayerState player)
        {
            _onCardPlay?.Invoke(player);
        }
    }
}

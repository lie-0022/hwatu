using System.Collections.Generic;
using Hwatu.Core.Enemies;

namespace Hwatu.Core.Combat
{
    /// <summary>
    /// 적의 런타임 상태(<see cref="ICombatant"/>). HP/Block/상태 + AI 진행 + 현재 intent(완전정보 노출).
    /// </summary>
    public sealed class EnemyState : ICombatant
    {
        private readonly Dictionary<StatusType, int> _statuses = new Dictionary<StatusType, int>();

        public EnemyState(EnemyData data, int maxHp, IEnemyAi ai)
        {
            Data = data;
            MaxHp = maxHp;
            Hp = maxHp;
            Ai = ai;
        }

        public EnemyData Data { get; }
        public int MaxHp { get; }
        public int Hp { get; private set; }
        public int Block { get; private set; }
        public IEnemyAi Ai { get; }
        public EnemyMoveData CurrentIntent { get; private set; }
        public bool IsDead => Hp <= 0;

        /// <summary>파멸(Doom) 카운트다운. -1이면 비활성, 0이면 이번 적 턴에 발동.</summary>
        public int DoomTimer { get; private set; } = -1;
        public void SetDoomTimer(int value) => DoomTimer = value;

        public void SetHp(int value) => Hp = value;
        public void SetBlock(int value) => Block = value;
        public int GetStatus(StatusType status) => _statuses.TryGetValue(status, out var v) ? v : 0;
        public void AddStatus(StatusType status, int amount) => _statuses[status] = GetStatus(status) + amount;

        /// <summary>AI에서 다음 move를 계산해 intent로 노출한다(완전정보).</summary>
        public void RefreshIntent(PlayerState player = null)
        {
            CurrentIntent = Ai.PeekNext(this, player);
        }
    }
}

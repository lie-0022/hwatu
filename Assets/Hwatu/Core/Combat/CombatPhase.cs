namespace Hwatu.Core.Combat
{
    /// <summary>전투 턴 상태머신의 단계(SPEC §3.2와 1:1).</summary>
    public enum CombatPhase
    {
        CombatStart,
        PlayerTurnStart,
        PlayerAction,
        PlayerTurnEnd,
        EnemyTurn,
        CheckDeath,
        Win,
        Lose
    }
}

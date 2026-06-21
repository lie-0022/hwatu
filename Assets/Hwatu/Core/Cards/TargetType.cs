namespace Hwatu.Core.Cards
{
    /// <summary>
    /// 효과/카드의 대상. 디스패처는 Self를 시전자(Source)로, 그 외(Enemy/AllEnemies/None)를
    /// 전투 컨텍스트가 정한 Target으로 리졸브한다.
    /// </summary>
    public enum TargetType
    {
        Enemy,
        AllEnemies,
        Self,
        None
    }
}

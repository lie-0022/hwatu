namespace Hwatu.Core.Cards
{
    /// <summary>
    /// 런타임 카드 인스턴스. 같은 정의(예: 빛타격 5장)를 더미 이동·셔플에서 개별 식별한다.
    /// </summary>
    public sealed class CardInstance
    {
        public CardData Data { get; }
        public int InstanceId { get; }

        public CardInstance(CardData data, int instanceId)
        {
            Data = data;
            InstanceId = instanceId;
        }
    }
}

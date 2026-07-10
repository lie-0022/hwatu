namespace Hwatu.Core.Jokbo
{
    /// <summary>전투 중 화투 카드 1장의 런타임 인스턴스(UI 추적용 고유 id).</summary>
    public sealed class JokboCardInstance
    {
        public HwatuCardData Data { get; }
        public int InstanceId { get; }

        public JokboCardInstance(HwatuCardData data, int instanceId)
        {
            Data = data;
            InstanceId = instanceId;
        }
    }
}

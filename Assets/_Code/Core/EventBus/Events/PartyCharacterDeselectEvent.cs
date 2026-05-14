namespace Code.Core.Events.Bus
{
    public struct PartyCharacterDeselectEvent : IEvent
    {
        public UnitSO Unit { get; }

        public PartyCharacterDeselectEvent(UnitSO unit)
        {
            Unit = unit;
        }
    }
}
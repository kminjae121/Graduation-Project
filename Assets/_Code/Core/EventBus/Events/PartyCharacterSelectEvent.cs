namespace Code.Core.Events.Bus
{
    public struct PartyCharacterSelectEvent : IEvent
    {
        public UnitSO Unit { get; }

        public PartyCharacterSelectEvent(UnitSO unit)
        {
            Unit = unit;
        }
    }
}
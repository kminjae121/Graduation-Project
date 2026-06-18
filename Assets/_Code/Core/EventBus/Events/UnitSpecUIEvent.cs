namespace Code.Core.Events.Bus
{
    public struct UnitSpecUIEvent : IEvent
    {
        public UnitType UnitType;
        
        public UnitSpecUIEvent(UnitType unitType)
        {
            UnitType = unitType;
        }
    }
}
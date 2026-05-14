namespace Code.Core.Events.Bus
{
    public struct UnitGimicUIEvent : IEvent
    {
        public UnitType UnitType;
        
        public UnitGimicUIEvent(UnitType unitType)
        {
            UnitType = unitType;
        }
    }
}
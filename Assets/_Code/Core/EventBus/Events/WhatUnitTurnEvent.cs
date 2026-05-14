namespace Code.Core.Events.Bus
{
    public struct WhatUnitTurnEvent : IEvent
    {
        public UnitType UnitType;
        
        public WhatUnitTurnEvent(UnitType unitType)
        {
            this.UnitType = unitType;
        }
    }
}
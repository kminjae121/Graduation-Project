using Code.Core.Interfaces;

namespace Code.Core.Events.Bus
{
    public struct UnitTurnEndEvent : IEvent
    {
        public ITurnable Unit;

        public UnitTurnEndEvent(ITurnable unit)
        {
            Unit = unit;
        }
    }
}
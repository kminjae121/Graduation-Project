using Code.UnitSystem;

namespace Code.Core.Events.Bus
{
    public struct UnitDeadEvent : IEvent
    {
        public Unit Unit { get; }

        public UnitDeadEvent(Unit unit)
        {
            Unit = unit;
        }
    }
}
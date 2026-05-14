using Code.UnitSystem;

namespace Code.Core.Events.Bus
{
    public struct UnitSpawnEvent : IEvent
    {
        public Unit Unit { get; }

        public UnitSpawnEvent(Unit unit)
        {
            Unit = unit;
        }
    }
}


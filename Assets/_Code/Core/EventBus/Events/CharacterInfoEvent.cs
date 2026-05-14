using Code.UnitSystem;

namespace Code.Core.Events.Bus
{
    public struct CharacterInfoEvent : IEvent
    {
        public UnitState Unit { get; }

        public CharacterInfoEvent(UnitState unit)
        {
            Unit = unit;
        }
    }
}
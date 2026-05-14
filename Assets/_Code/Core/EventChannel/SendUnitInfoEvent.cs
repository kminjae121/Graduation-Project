using Code.Core.Events.Bus;
using Code.UnitSystem;

namespace GameEventChannel
{
    public struct SendUnitInfoEvent : IEvent
    {
        public UnitState unitState;

        public SendUnitInfoEvent(UnitState unit)
        {
            unitState = unit;
        }
    }
}
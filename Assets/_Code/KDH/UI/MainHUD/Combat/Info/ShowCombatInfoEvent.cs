using Code.UnitSystem;

namespace Code.Core.Events.Bus
{
    public abstract class ShowCombatInfoEvent : IEvent
    {
        public UnitState TargetUnit { get; }
        public bool IsShow { get; }

        public ShowCombatInfoEvent(UnitState targetUnit, bool isShow)
        {
            TargetUnit = targetUnit;
            IsShow = isShow;
        }
    }
}
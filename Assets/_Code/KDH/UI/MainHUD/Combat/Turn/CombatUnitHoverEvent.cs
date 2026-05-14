using Code.Core.Interfaces;
using Code.UnitSystem;

namespace Code.Core.Events.Bus
{
    public struct CombatUnitHoverEvent : IEvent
    {
        public ITurnable HoveredUnit { get; }
        public bool IsHoverEnter { get; }

        public CombatUnitHoverEvent(ITurnable hoveredUnit, bool isHoverEnter)
        {
            HoveredUnit = hoveredUnit;
            IsHoverEnter = isHoverEnter;
        }
    }
}
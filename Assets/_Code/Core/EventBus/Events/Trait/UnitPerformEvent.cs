using Code.UnitSystem.UnitAttributes;

namespace Code.Core.Events.Bus.Trait
{
    public struct UnitPerformEvent : IEvent
    {
        public UnitTrait TraitCompo;
        
        public UnitPerformEvent(UnitType unitType, UnitTrait traitCompo)
        {
            TraitCompo = traitCompo;
        }
    }
}
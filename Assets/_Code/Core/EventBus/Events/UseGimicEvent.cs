using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct UseGimicEvent : IEvent
    {
        public UnitType unitType;

        public GameObject target;
        
        public UseGimicEvent(UnitType unitType, GameObject target)
        {
            this.unitType = unitType;
            this.target = target;
        }
    }
}
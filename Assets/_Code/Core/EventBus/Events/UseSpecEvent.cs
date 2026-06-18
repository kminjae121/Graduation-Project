using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct UseSpecEvent : IEvent
    {
        public UnitType unitType;

        public GameObject target;
        
        public UseSpecEvent(UnitType unitType, GameObject target)
        {
            this.unitType = unitType;
            this.target = target;
        }
    }
}
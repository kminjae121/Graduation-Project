using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct CamMovingEvent : IEvent
    {
        public GameObject target;

        public CamMovingEvent(GameObject target)
        {
            this.target = target;
        }
    }
}
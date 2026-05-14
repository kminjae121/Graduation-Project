using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct UnitCamSettingEvent : IEvent
    {
        public GameObject target;
        public bool isLocking;
        public Vector3 dampingSpeed;

        public UnitCamSettingEvent(GameObject target , bool isLocking , Vector3 dampingSpeed)
        {
            this.target = target;
            this.isLocking = isLocking;
            this.dampingSpeed = dampingSpeed;
        }
    }
}
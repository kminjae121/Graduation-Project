using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct UnitMoveControlEvent : IEvent
    {
        public bool isMoving;

        public UnitMoveControlEvent(bool isMoving)
        {
            this.isMoving = isMoving;
        }
    }
}
using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct SetMarkEvent : IEvent
    {
        public GameObject target;
        public int cnt;

        public SetMarkEvent(GameObject target, int cnt)
        {
            this.cnt  = cnt;
            this.target = target;
        }
    }
}
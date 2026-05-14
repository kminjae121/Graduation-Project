using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct UnitSkilStartEvent : IEvent
    {
        public bool isStart;

        public UnitSkilStartEvent(bool isStart)
        {
            this.isStart = isStart;
        }
    }
}
using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct UnitSkillStartEvent : IEvent
    {
        public bool isStart;

        public UnitSkillStartEvent(bool isStart)
        {
            this.isStart = isStart;
        }
    }
}
using Code.Core.Events.Bus;

namespace _Code.Core.EventBus.Events.Trait
{
    public struct ArcherGimicEvent : IEvent
    {
        public float value;

        public ArcherGimicEvent(float value)
        {
            this.value = value;
        }
    }
}
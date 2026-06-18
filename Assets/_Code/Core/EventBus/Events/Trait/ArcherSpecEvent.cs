using Code.Core.Events.Bus;

namespace _Code.Core.EventBus.Events.Trait
{
    public struct ArcherSpecEvent : IEvent
    {
        public float value;

        public ArcherSpecEvent(float value)
        {
            this.value = value;
        }
    }
}
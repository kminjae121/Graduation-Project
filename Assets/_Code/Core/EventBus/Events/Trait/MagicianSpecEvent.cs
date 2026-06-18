using Code.Core.Events.Bus;

namespace _Code.Core.EventBus.Events.Trait
{
    public struct MagicianSpecEvent : IEvent
    {
        public float value;

        public MagicianSpecEvent(float value)
        {
            this.value = value;
        }
    }
}
using Code.Core.Events.Bus;

namespace _Code.Core.EventBus.Events.Trait
{
    public struct RogueSpecEvent : IEvent
    {
        public float value;

        public RogueSpecEvent(float value)
        {
            this.value = value;
        }
    }
}
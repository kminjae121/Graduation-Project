using Code.Core.Events.Bus;

namespace _Code.Core.EventBus.Events.Trait
{
    public struct RogueGimicBarEvent : IEvent
    {
        public float value;

        public RogueGimicBarEvent(float value)
        {
            this.value = value;
        }
    }
}
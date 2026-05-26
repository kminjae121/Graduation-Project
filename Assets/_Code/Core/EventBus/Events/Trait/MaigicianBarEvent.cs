using Code.Core.Events.Bus;

namespace _Code.Core.EventBus.Events.Trait
{
    public struct MaigicianBarEvent : IEvent
    {
        public float value;

        public MaigicianBarEvent(float value)
        {
            this.value = value;
        }
    }
}
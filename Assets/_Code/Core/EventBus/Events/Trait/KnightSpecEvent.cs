namespace Code.Core.Events.Bus
{
    public struct KnightSpecEvent : IEvent
    {
        public float value;

        public KnightSpecEvent(float value)
        {
            this.value = value;
        }
    }
}
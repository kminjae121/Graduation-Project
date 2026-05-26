namespace Code.Core.Events.Bus
{
    public struct KnightGimicBarEvent : IEvent
    {
        public float value;

        public KnightGimicBarEvent(float value)
        {
            this.value = value;
        }
    }
}
namespace Code.Core.Events.Bus
{
    public struct ActionGaugeEvent : IEvent
    {
        public float Value { get; private set; }

        public ActionGaugeEvent(float value)
        {
            Value = value;
        }
    }
}
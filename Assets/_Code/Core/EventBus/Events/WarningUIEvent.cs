namespace Code.Core.Events.Bus
{
    public struct WarningUIEvent : IEvent
    {
        public string message;

        public WarningUIEvent(string message)
        {
            this.message = message;
        }
    }
}
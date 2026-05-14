namespace Code.Core.Events.Bus
{
    public struct ShowMessageUIEvent : IEvent
    {
        public string Message { get; }

        public ShowMessageUIEvent(string message)
        {
            Message = message;
        }
    }
}
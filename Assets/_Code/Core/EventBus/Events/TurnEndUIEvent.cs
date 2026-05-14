namespace Code.Core.Events.Bus
{
    public struct TurnEndUIEvent : IEvent
    {
        public bool isActive;

        public TurnEndUIEvent(bool isActive)
        {
            this.isActive = isActive;
        }
    }
}
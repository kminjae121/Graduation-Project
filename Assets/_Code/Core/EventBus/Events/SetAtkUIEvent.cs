namespace Code.Core.Events.Bus
{
    public struct SetAtkUIEvent : IEvent
    {
        public bool IsActive { get; private set; }
        
        public SetAtkUIEvent(bool isActive)
        {
            IsActive = isActive;
        }
    }
}
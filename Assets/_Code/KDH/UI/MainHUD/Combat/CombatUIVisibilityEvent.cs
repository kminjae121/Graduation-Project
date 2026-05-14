namespace Code.Core.Events.Bus
{
    public struct CombatUIVisibilityEvent : IEvent
    {
        public bool IsVisible { get; }

        public CombatUIVisibilityEvent(bool isVisible)
        {
            IsVisible = isVisible;
        }
    }
}
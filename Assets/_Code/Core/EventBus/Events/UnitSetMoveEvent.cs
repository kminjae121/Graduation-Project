namespace Code.Core.Events.Bus
{
    public struct UnitSetMoveEvent : IEvent
    {
        public bool isStart;
        
        public UnitSetMoveEvent(bool isStart)
        {
            this.isStart = isStart;
        }
    }
}
namespace Code.Core.Events.Bus
{
    public struct KnightSwordEvent : IEvent
    {
        public int idx;
        
        public KnightSwordEvent(int idx)
        {
            this.idx = idx;
        }
    }
}
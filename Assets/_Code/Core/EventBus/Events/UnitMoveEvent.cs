namespace Code.Core.Events.Bus
{
    public struct UnitMoveEvent : IEvent
    {
        public bool isMove { get; }

        public UnitMoveEvent(bool isMove)
        {
            this.isMove = isMove;
        }
    }
}
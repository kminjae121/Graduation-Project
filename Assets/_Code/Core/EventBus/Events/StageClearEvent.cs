namespace Code.Core.Events.Bus
{
    public struct StageClearEvent : IEvent
    {
        public bool isClear;

        public StageClearEvent(bool isClear)
        {
            this.isClear = isClear;
        }
    }
}
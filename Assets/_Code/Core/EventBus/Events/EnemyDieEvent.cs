namespace Code.Core.Events.Bus
{
    public struct EnemyDieEvent : IEvent
    {
        public int count;

        public EnemyDieEvent(int count)
        {
            this.count = count;
        }
    }
}
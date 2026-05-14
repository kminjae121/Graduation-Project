namespace Code.Core.Events.Bus
{
    public struct UsingSkillEvent : IEvent
    {
        public bool isUsingSkill;

        public UsingSkillEvent(bool isUsingSkill)
        {
            this.isUsingSkill = isUsingSkill;
        }
    }
}
using Code.SkillSystem;

namespace Code.Core.Events.Bus
{
    public struct SendSkillEvent : IEvent
    {
        public BasicUnitSkill skill;

        public SendSkillEvent(BasicUnitSkill skill)
        {
            this.skill = skill;
        }
    }
}
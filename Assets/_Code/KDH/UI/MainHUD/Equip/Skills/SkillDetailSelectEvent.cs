using Code.SkillSystem;

namespace Code.Core.Events.Bus
{
    public class SkillDetailSelectEvent : IEvent
    {
        public SkillSO Skill { get; }

        public SkillDetailSelectEvent(SkillSO skill)
        {
            Skill = skill;
        }
    }
}
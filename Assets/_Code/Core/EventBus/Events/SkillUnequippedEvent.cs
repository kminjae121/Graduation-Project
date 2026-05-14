using Code.SkillSystem;

namespace Code.Core.Events.Bus
{
    public struct SkillUnequippedEvent : IEvent
    {
        public SkillSO Skill { get; }

        public SkillUnequippedEvent(SkillSO skill)
        {
            Skill = skill;
        }
    }
}
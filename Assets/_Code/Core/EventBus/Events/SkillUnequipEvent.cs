using Code.SkillSystem;

namespace Code.Core.Events.Bus
{
    public struct SkillUnequipEvent : IEvent
    {
        public SkillSO Skill { get; }

        public SkillUnequipEvent(SkillSO skill)
        {
            Skill = skill;
        }
    }
}
using Code.SkillSystem;

namespace Code.Core.Events.Bus
{
    public struct SkillEquippedEvent : IEvent
    {
        public SkillSO Skill { get; }
        
        public SkillEquippedEvent(SkillSO skill)
        {
            Skill = skill;
        }
    }
}
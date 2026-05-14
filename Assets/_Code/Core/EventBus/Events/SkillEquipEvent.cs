using Code.SkillSystem;

namespace Code.Core.Events.Bus
{
    public struct SkillEquipEvent : IEvent
    {
        public SkillSO Skill { get; }
        
        public SkillEquipEvent(SkillSO skill)
        {
            Skill = skill;
        }
    }
}
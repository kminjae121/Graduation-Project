using Code.SkillSystem;
using UnityEngine;

namespace Code.Core.Events.Bus
{
    public class CombatSkillSelectEvent : IEvent
    {
        public SkillSO SelectedSkill { get; }
        public CombatSkillSelectEvent(SkillSO selectedSkill) => SelectedSkill = selectedSkill;
    }

    public class CombatSkillCancelEvent : IEvent
    {
        public CombatSkillCancelEvent() { }
    }
}
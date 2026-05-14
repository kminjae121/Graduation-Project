using System.Collections.Generic;
using Code.SkillSystem;

namespace Code.Core.Events.Bus
{
    public struct SkillUIEvent : IEvent
    {
        public List<SkillSO> Skills { get; private set; }
        public SkillComponent SkillCompo { get; private set; }

        public SkillUIEvent(List<SkillSO> skills, SkillComponent skillCompo)
        {
            Skills = skills;
            SkillCompo = skillCompo;
        }
    }
}
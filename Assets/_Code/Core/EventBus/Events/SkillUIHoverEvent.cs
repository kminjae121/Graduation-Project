using Code.SkillSystem;
using UnityEngine;

namespace Code.Core.Events.Bus
{
    public class SkillUIHoverEvent : IEvent
    {
        public SkillSO Skill { get; }
        public RectTransform Pivot { get; }
        public Vector2 Offset { get; }

        public SkillUIHoverEvent(SkillSO skill, RectTransform pivot, Vector2 offset = default)
        {
            Skill = skill;
            Pivot = pivot;
            Offset = offset;
        }
    }
}
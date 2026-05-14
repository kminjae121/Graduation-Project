using Code.SkillSystem;
using UnityEngine;

namespace Code.Core.Events.Bus
{
    public class CombatSkillHoverEvent : IEvent
    {
        public SkillSO Skill { get; }
        public RectTransform Pivot { get; }
        public Vector2 Offset { get; }

        public CombatSkillHoverEvent(SkillSO skill, RectTransform pivot = null, Vector2 offset = default)
        {
            Skill = skill;
            Pivot = pivot;
            Offset = offset;
        }
    }
}
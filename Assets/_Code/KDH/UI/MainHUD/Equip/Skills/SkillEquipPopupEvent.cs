using Code.Core.Events.Bus;
using Code.SkillSystem;
using UnityEngine;

namespace Code.UI
{
    public class SkillEquipPopupEvent : IEvent
    {
        public SkillSO Skill { get; }
        public bool IsEquipped { get; }
        public RectTransform Pivot { get; }
        public Vector2 Offset { get; }
        public bool IsReadOnly { get; }

        public SkillEquipPopupEvent(SkillSO skill, bool isEquipped, RectTransform pivot, Vector2 offset = default, bool isReadOnly = false)
        {
            Skill = skill;
            IsEquipped = isEquipped;
            Pivot = pivot;
            Offset = offset;
            IsReadOnly = isReadOnly;
        }
    }
}
using Code.Core.Events.Bus;
using Code.Items;
using UnityEngine;

namespace Code.UI
{
    public class ArtifactPopupEvent : IEvent
    {
        public EquipmentItemSO EquipmentItem { get; }
        public bool IsEquipped { get; }
        public RectTransform Pivot { get; }
        public Vector2 Offset { get; }
        public bool IsReadOnly { get; }

        public ArtifactPopupEvent(EquipmentItemSO equipmentItem, bool isEquipped, RectTransform pivot, Vector2 offset = default, bool isReadOnly = false)
        {
            EquipmentItem = equipmentItem;
            IsEquipped = isEquipped;
            Pivot = pivot;
            Offset = offset;
            IsReadOnly = isReadOnly;
        }
    }

    public class ArtifactEquipEvent : IEvent
    {
        public EquipmentItemSO EquipmentItem { get; }
        public ArtifactEquipEvent(EquipmentItemSO equipmentItem) => EquipmentItem = equipmentItem;
    }

    public class ArtifactUnequipEvent : IEvent
    {
        public EquipmentItemSO EquipmentItem { get; }
        public ArtifactUnequipEvent(EquipmentItemSO equipmentItem) => EquipmentItem = equipmentItem;
    }
}
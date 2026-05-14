using Code.Items;
using Code.UnitSystem.ArtifactSystem;

namespace Code.Core.Events.Bus
{
    public struct EquipArtifactEvent : IEvent
    {
        public EquipmentItemSO EquipmentItem;

        public EquipArtifactEvent(EquipmentItemSO equipmentItem)
        {
            this.EquipmentItem = equipmentItem;
        }
    }
}
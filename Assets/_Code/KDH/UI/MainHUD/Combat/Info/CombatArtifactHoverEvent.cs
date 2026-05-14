using Code.Items;
using UnityEngine;

namespace Code.Core.Events.Bus
{
    public class CombatArtifactHoverEvent : IEvent
    {
        public ItemSO Artifact { get; }
        public bool IsShow { get; }
        public RectTransform Pivot { get; }
        public Vector2 Offset { get; }

        public CombatArtifactHoverEvent(ItemSO artifact, bool isShow, RectTransform pivot = null, Vector2 offset = default)
        {
            Artifact = artifact;
            IsShow = isShow;
            Pivot = pivot;
            Offset = offset;
        }
    }
}
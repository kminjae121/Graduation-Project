using UnityEngine;

namespace GameEventChannel
{
    public static class TextEvent
    {
        public static PopupTextEvent PopupTextEvent = new PopupTextEvent();
    }

    public class PopupTextEvent : GameEvent
    {
        public string text;
        public int textTypeHash;
        public Vector3 position;
        public float showDuraction;

        public PopupTextEvent Initializer(string text, int typeHash, Vector3 position, float showDuraction)
        {
            this.text = text;
            this.textTypeHash = typeHash;
            this.position = position;
            this.showDuraction = showDuraction;
            return this;
        }
    }
}
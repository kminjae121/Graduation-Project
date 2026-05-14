using UnityEngine;

namespace Code.Core.Events.Bus
{
    public class GageEvent : MonoBehaviour, IEvent
    {
        public float gageCost;
        
        public GageEvent(float gageCost)
        {
            this.gageCost = gageCost;
        }
    }
}
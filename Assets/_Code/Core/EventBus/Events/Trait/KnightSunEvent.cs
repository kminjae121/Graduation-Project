using Code.Core.Events.Bus;
using UnityEngine;

namespace _Code.Core.EventBus.Events.Trait
{
    public struct KnightSunEvent : IEvent
    {
        public bool IsActive { get; set; }
        
        public KnightSunEvent(bool isActive)
        {
            IsActive = isActive; 
        }
    }
}
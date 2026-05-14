using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct PartyCharacterHoverEvent : IEvent
    { 
        public UnitSO Unit { get; }
        
        public PartyCharacterHoverEvent(UnitSO unit)
        {
            Unit = unit;
        }
    }
}
using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct UnitAttackEvent : IEvent
    {
        public bool isAttack;

        public UnitAttackEvent(bool isAttack)
        {
            this.isAttack = isAttack;
        }
    }
}
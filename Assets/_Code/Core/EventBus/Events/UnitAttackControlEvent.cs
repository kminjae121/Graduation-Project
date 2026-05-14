using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct UnitAttackControlEvent : IEvent
    {
        public bool isAttacking;

        public UnitAttackControlEvent(bool isAttacking)
        {
            this.isAttacking = isAttacking;
        }
    }
}
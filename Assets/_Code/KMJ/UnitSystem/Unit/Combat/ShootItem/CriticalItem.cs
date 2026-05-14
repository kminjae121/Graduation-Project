using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.UnitSystem.Combat
{
    public class CriticalItem : ShootItem
    {
        public override void AttackEnd()
        {
            Bus<DamageEvent>.Raise(new DamageEvent(_shootItemManager.DamageData,_target,0,_shootItemManager.Unit
                , true,false,0.2f));
        }
    }
}
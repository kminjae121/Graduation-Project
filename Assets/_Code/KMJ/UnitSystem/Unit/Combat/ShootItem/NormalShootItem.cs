    using Code.Core.Events.Bus;
    using UnityEngine;

    namespace Code.UnitSystem.Combat
{
    public class NormalShootItem : ShootItem
    {
        public override void AttackEnd()
        {
            Debug.Log("Attacks");
            Bus<DamageEvent>.Raise(new DamageEvent(_shootItemManager.DamageData,_target,0,_shootItemManager.Unit
                , false,false,0.2f));
            
            gameObject.SetActive(false);
        }
    }
}
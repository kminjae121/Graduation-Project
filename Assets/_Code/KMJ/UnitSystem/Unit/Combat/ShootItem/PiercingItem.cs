using System;
using System.Collections;
using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.UnitSystem.Combat
{
    public class PiercingItem : ShootItem
    {
        private bool isPiercing = true;

        public override void AttackEnd()
        {
            ContinueArrow();
        }

        private void ContinueArrow()
        {
            Debug.Log(_target);
            
            Bus<DamageEvent>.Raise(new DamageEvent(_shootItemManager.DamageData,_target,0,_shootItemManager.Unit
                , false,false,0.2f));
            
            if (isPiercing)
            {
                StartCoroutine(Arrow());
                isPiercing = false;
            }
            else
                return;
        }

        private IEnumerator Arrow()
        {
            yield return new WaitForSeconds(3f);
            
            gameObject.SetActive(false);
        }   
    }
}
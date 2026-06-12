using System;
using System.Collections.Generic;
using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.UnitSystem.Combat
{
    public class ShootItemAttackManager : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private List<ShootItem> shootItems;
        
        public Unit Unit { get; private set; }
        public DamageData DamageData { get; private set; }  
        public Action hitEvent;
        
        private readonly Dictionary<string, ShootItem> _shootItemDict = new();
        private GameObject _target;
        
        public void Initialize(Unit owner)
        {
            Unit = owner;
            
            shootItems.ForEach(item =>
            {
                _shootItemDict.Add(item.itemName, item); ;
            });
        }

        public void SetTarget(GameObject target)
        {
            _target = target;
        }

        public void SetDamageData(DamageData damageData, float addDamage)
        {
            DamageData = damageData;
        }

        public void CreateShootItem(string itemName, Vector3 pos, Vector3 rotation)
        {
            ShootItem itemCompo = _shootItemDict.GetValueOrDefault(itemName);
            GameObject item = itemCompo.gameObject;

            if (item == null)
                return;
            
            if (_target == null)
                return;
            
            GameObject shootItem = Instantiate(item, pos ,Quaternion.identity);
            ShootItem shootItemCompo = shootItem.GetComponent<ShootItem>();
            
            UnitAnimation targetAnimation = _target.GetComponentInChildren<UnitAnimation>();
            GameObject hitTarget = targetAnimation != null ? targetAnimation.gameObject : _target;

            shootItemCompo.SetShootItemCompo(this);
            shootItemCompo.SetTarget(hitTarget);
            shootItem.transform.rotation = Quaternion.Euler(rotation);
        }
    }
}

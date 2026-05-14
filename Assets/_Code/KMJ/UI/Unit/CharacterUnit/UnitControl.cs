using System;
using Code.Core.Events.Bus;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UnitControl : MonoBehaviour
    {
        [SerializeField] private Button atkBtn;

        public bool isAttacking = true;

        private void Awake()
        {
            atkBtn.onClick.AddListener(HandleAttack);
            Bus<UnitAttackControlEvent>.Subscribe(SetAttacking);
        }

        private void OnDisable()
        {
            atkBtn.onClick.RemoveListener(HandleAttack);
            
            Bus<UnitAttackControlEvent>.Unsubscribe(SetAttacking);
        }
        

        public void SetAttacking(UnitAttackControlEvent evt)
        {
            isAttacking = evt.isAttacking;
        }
        

        private void HandleAttack()
        {
            Bus<UnitAttackEvent>.Raise(new UnitAttackEvent(true));  
        }
        
    }
}
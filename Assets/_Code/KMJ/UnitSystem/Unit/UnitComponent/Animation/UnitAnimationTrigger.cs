using System;
using UnityEngine;

namespace Code.UnitSystem
{
    public class UnitAnimationTrigger : MonoBehaviour, IUnitComponent
    {
        public Action OnDeadEvent;
        public Action OnAttackTrigger;
        public Action OnAnimationEndTrigger;
        public Action OnTakeDamageTrigger;
        
        private Unit _entity;
        
        public void Initialize(Unit entity)
        {
            _entity = entity;
        }

        private void TakeDamage() => OnTakeDamageTrigger?.Invoke();
        private void TriggerDead() => OnDeadEvent?.Invoke();
        private void AnimationEnd() => OnAnimationEndTrigger?.Invoke();
        private void Dead() => OnDeadEvent?.Invoke();
        private void Attack() => OnAttackTrigger?.Invoke();
    }
}
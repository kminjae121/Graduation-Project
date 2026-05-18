using System;
using System.Collections;
using Code.Core.Events.Bus;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyRangedAttack : EnemyBaseSkill
    {
        private GameObject _target;
        private AbstractEnemyUnit _owner;

        private void Awake()
        {
            _owner = GetComponentInParent<AbstractEnemyUnit>();
        }

        private void Start()
        {
            SkillEvent.AddListener(AttackAction);
        }

        public override void ForceUseSkill(GameObject target)
        {
            if (target == null)
                return;

            base.ForceUseSkill(target);
            PlayAttackAnimation();
        }

        protected override void StartEvent()
        {
            base.StartEvent();
            triggerCompo.OnAttackTrigger += TakeDamage;
            triggerCompo.OnAnimationEndTrigger += SkillEnd;
        }

        protected override void OnDestroy()
        {
            SkillEvent.RemoveListener(AttackAction);
            
            if (triggerCompo != null)
            {
                triggerCompo.OnAttackTrigger -= TakeDamage;
                triggerCompo.OnAnimationEndTrigger -= SkillEnd;
            }
            
            base.OnDestroy();
        }
        
        private void AttackAction(GameObject target)
        {
            _target = target;
        }

        private void TakeDamage()
        {
            Bus<DamageEvent>.Raise(new DamageEvent(DamageData, _target, AddDamage, null, false, false, 0.1f));
        }
        
        private void SkillEnd()
        {
            triggerCompo.OnAttackTrigger -= TakeDamage;
            triggerCompo.OnAnimationEndTrigger -= SkillEnd;
            _target = null;
            SkillEndEvent?.Invoke();
            SkillFinished(false);
        }

        private void PlayAttackAnimation()
        {
            if (_owner?.UnitAnimator == null || SkillSO == null || string.IsNullOrWhiteSpace(SkillSO.skillAnimationKey))
                return;

            _owner.UnitAnimator.RestartFromEntry();
            _owner.UnitAnimator.PlaySelectAnimation(SkillSO.skillAnimationKey);
        }
    }
}
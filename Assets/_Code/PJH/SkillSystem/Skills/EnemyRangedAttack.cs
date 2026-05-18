using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyRangedAttack : EnemyBaseSkill
    {
        private GameObject _target;
        private AbstractEnemyUnit _ownerEnemy;

        private void Awake()
        {
            _ownerEnemy = GetComponentInParent<AbstractEnemyUnit>();
        }

        protected void Start()
        {
            ResolveComponents();
            SkillEvent.AddListener(AttackAction);
        }

        public override void ForceUseSkill(GameObject target)
        {
            if (target == null)
                return;

            ResolveComponents();
            base.ForceUseSkill(target);
            PlayAttackAnimation();
        }

        protected override void StartEvent()
        {
            base.StartEvent();
            RegisterAnimationEvents();
        }

        protected override void OnDestroy()
        {
            SkillEvent.RemoveListener(AttackAction);
            UnregisterAnimationEvents();
            base.OnDestroy();
        }

        private void AttackAction(GameObject target)
        {
            _target = target;
        }

        private void TakeDamage()
        {
            if (_target == null)
                return;

            Bus<DamageEvent>.Raise(new DamageEvent(DamageData, _target, AddDamage, null, false, false, 0.1f));
        }

        private void SkillEnd()
        {
            UnregisterAnimationEvents();
            _target = null;

            SkillFinished(false);
            SkillEndEvent?.Invoke();
        }

        private void PlayAttackAnimation()
        {
            if (_ownerEnemy?.UnitAnimator == null || SkillSO == null || string.IsNullOrWhiteSpace(SkillSO.skillAnimationKey))
            {
                SkillEnd();
                return;
            }

            _ownerEnemy.UnitAnimator.RestartFromEntry();
            _ownerEnemy.UnitAnimator.PlaySelectAnimation(SkillSO.skillAnimationKey);
            SkillFeedbackEvent?.Invoke();
        }

        private void ResolveComponents()
        {
            if (_ownerEnemy == null)
                _ownerEnemy = GetComponentInParent<AbstractEnemyUnit>();

            if (triggerCompo == null)
                triggerCompo = _ownerEnemy?.AnimationTrigger ?? _ownerEnemy?.GetUnitCompo<UnitAnimationTrigger>();
        }

        private void RegisterAnimationEvents()
        {
            ResolveComponents();

            if (triggerCompo == null)
                return;

            triggerCompo.OnAttackTrigger -= TakeDamage;
            triggerCompo.OnAnimationEndTrigger -= SkillEnd;

            triggerCompo.OnAttackTrigger += TakeDamage;
            triggerCompo.OnAnimationEndTrigger += SkillEnd;
        }

        private void UnregisterAnimationEvents()
        {
            if (triggerCompo == null)
                return;

            triggerCompo.OnAttackTrigger -= TakeDamage;
            triggerCompo.OnAnimationEndTrigger -= SkillEnd;
        }
    }
}

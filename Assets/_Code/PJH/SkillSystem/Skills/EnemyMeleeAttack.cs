using Code.Core.Debugs;
using Code.Core.Events.Bus;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyMeleeAttack : EnemyBaseSkill
    {
        private GameObject _target;
        private AbstractEnemyUnit _ownerEnemy;

        private void Awake()
        {
            _ownerEnemy = GetComponentInParent<AbstractEnemyUnit>();
        }

        protected void Start()
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
            UnityLogger.Log("근접 공격으로 데미지");
            Bus<DamageEvent>.Raise(new DamageEvent(DamageData, _target, AddDamage, null, false,false,0.1f));
        }

        private void SkillEnd()
        {
            triggerCompo.OnAttackTrigger -= TakeDamage;
            triggerCompo.OnAnimationEndTrigger -= SkillEnd;
            _target = null;
            SkillFinished(false);
            SkillEndEvent?.Invoke();
        }

        private void PlayAttackAnimation()
        {
            if (_ownerEnemy?.UnitAnimator == null || string.IsNullOrWhiteSpace(SkillSO.skillAnimationKey))
                return;

            _ownerEnemy.UnitAnimator.RestartFromEntry();
            _ownerEnemy.UnitAnimator.PlaySelectAnimation(SkillSO.skillAnimationKey);
        }
    }
}

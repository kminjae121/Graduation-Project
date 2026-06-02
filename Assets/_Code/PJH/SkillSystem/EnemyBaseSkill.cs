using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.Map;
using Code.UnitSystem;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.SkillSystem
{
    public abstract class EnemyBaseSkill : EnemySkill
    {
        protected GameObject Target { get; private set; }
        protected AbstractEnemyUnit Owner { get; private set; }
        protected UnitManager UnitManager => Owner != null ? Owner.UnitManager : null;

        private bool _isFinished = true;
        private bool _isEventBound;

        protected virtual void Awake()
        {
            Owner = GetComponentInParent<AbstractEnemyUnit>();

            if (Owner != null)
                triggerCompo = Owner.GetUnitCompo<UnitAnimationTrigger>();
        }

        public override void ForceUseSkill(GameObject target)
        {
            if (target == null)
                return;

            _targetEnemy = target;
            Target = target;
            isCanUseSkill = true;
            _isFinished = false;

            if (RotatorCompo != null)
            {
                RotatorCompo.SetDir(target.transform.position, BeginSkill);
                return;
            }

            BeginSkill();
        }

        protected override void StartEvent()
        {
            base.StartEvent();
            BindAnimationEvents();
        }

        protected override void OnDestroy()
        {
            UnbindAnimationEvents();
            base.OnDestroy();
        }

        protected virtual void OnSkillStarted()
        {
        }

        protected virtual bool UseAttackEvent => false;
        protected virtual bool UseShowEffectEvent => false;

        protected virtual void OnAttack(GameObject target)
        {
        }

        protected virtual void OnShowEffect()
        {
        }

        protected Vector2Int GetCasterGridPos()
        {
            var gridMap = GridMap.Instance;

            if (gridMap == null)
                return Vector2Int.zero;

            return gridMap.WorldToGridPos(GetCasterWorldPos());
        }

        private void BeginSkill()
        {
            if (_targetEnemy == null)
            {
                FinishSkill();
                return;
            }

            StartEvent();

            if (_isFinished)
                return;

            Bus<UnitSkillStartEvent>.Raise(new UnitSkillStartEvent(true));
            SkillEvent?.Invoke(_targetEnemy);
            OnSkillStarted();

            if (!PlayAttackAnimation())
                FinishSkill();
        }

        private void BindAnimationEvents()
        {
            if (triggerCompo == null)
            {
                FinishSkill();
                return;
            }

            UnbindAnimationEvents();

            if (UseAttackEvent)
                triggerCompo.OnAttackTrigger += HandleAttack;

            if (UseShowEffectEvent)
                triggerCompo.OnShowEffectTrigger += HandleShowEffect;

            triggerCompo.OnAnimationEndTrigger += FinishSkill;
            _isEventBound = true;
        }

        private void UnbindAnimationEvents()
        {
            if (!_isEventBound || triggerCompo == null)
                return;

            if (UseAttackEvent)
                triggerCompo.OnAttackTrigger -= HandleAttack;

            if (UseShowEffectEvent)
                triggerCompo.OnShowEffectTrigger -= HandleShowEffect;

            triggerCompo.OnAnimationEndTrigger -= FinishSkill;
            _isEventBound = false;
        }

        private void HandleAttack()
        {
            OnAttack(Target);
        }

        private void HandleShowEffect()
        {
            OnShowEffect();
        }

        protected virtual void FinishSkill()
        {
            if (_isFinished)
                return;

            _isFinished = true;
            UnbindAnimationEvents();
            Target = null;

            try
            {
                SkillFinished(false);
            }
            finally
            {
                SkillEndEvent?.Invoke();
            }
        }

        private bool PlayAttackAnimation()
        {
            if (Owner?.UnitAnimator == null || SkillSO == null || string.IsNullOrWhiteSpace(SkillSO.skillAnimationKey))
                return false;

            Owner.UnitAnimator.RestartFromEntry();
            Owner.UnitAnimator.PlaySelectAnimation(SkillSO.skillAnimationKey);
            return true;
        }
    }
}

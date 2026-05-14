using System.Collections;
using Code.Core.Events.Bus;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyRangedAttack : EnemyBaseSkill
    {
        [SerializeField, Min(0f)] private float finishDelay = 0.35f;

        private GameObject _target;
        private AbstractEnemyUnit _ownerEnemy;
        private Coroutine _finishCoroutine;
        private bool _isFinished = true;

        private void Awake()
        {
            _ownerEnemy = GetComponentInParent<AbstractEnemyUnit>();
        }

        public override void ForceUseSkill(GameObject target)
        {
            _isFinished = false;

            if (target == null)
            {
                FinishSkill();
                return;
            }

            _targetEnemy = target;
            isCanUseSkill = true;

            if (RotatorCompo != null)
                RotatorCompo.SetDir(target.transform.position);

            BeginRangedAttack();
        }

        protected override void OnDestroy()
        {
            StopFinishCoroutine();
            base.OnDestroy();
        }

        private void BeginRangedAttack()
        {
            if (_targetEnemy == null)
            {
                FinishSkill();
                return;
            }

            _target = _targetEnemy;
            FinishAfterDelay();

            StartEvent();
            Bus<UnitSkilStartEvent>.Raise(new UnitSkilStartEvent(true));
            SkillEvent?.Invoke(_target);

            PlayAttackAnimation();
            ApplyDamage();
        }

        private void ApplyDamage()
        {
            if (_target == null)
                return;

            Bus<DamageEvent>.Raise(new DamageEvent(DamageData, _target, AddDamage, null, false, false, 0.1f));
        }

        private void PlayAttackAnimation()
        {
            if (_ownerEnemy?.UnitAnimator == null || SkillSO == null || string.IsNullOrWhiteSpace(SkillSO.skillAnimationKey))
                return;

            _ownerEnemy.UnitAnimator.RestartFromEntry();
            _ownerEnemy.UnitAnimator.PlaySelectAnimation(SkillSO.skillAnimationKey);
            SkillFeedbackEvent?.Invoke();
        }

        private void FinishAfterDelay()
        {
            StopFinishCoroutine();

            _finishCoroutine = StartCoroutine(FinishAfterDelayRoutine());
        }

        private IEnumerator FinishAfterDelayRoutine()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, finishDelay));

            _finishCoroutine = null;
            FinishSkill();
        }

        private void FinishSkill()
        {
            if (_isFinished)
                return;

            _isFinished = true;
            StopFinishCoroutine();
            _target = null;

            try
            {
                SkillFinished(false);
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                Bus<UnitSkilStartEvent>.Raise(new UnitSkilStartEvent(false));
            }
            finally
            {
                SkillEndEvent?.Invoke();
            }
        }

        private void StopFinishCoroutine()
        {
            if (_finishCoroutine == null)
                return;

            StopCoroutine(_finishCoroutine);
            _finishCoroutine = null;
        }
    }
}

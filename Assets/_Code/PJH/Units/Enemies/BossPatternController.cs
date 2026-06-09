using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.SkillSystem;
using Code.UnitSystem.Enemies.AI;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UnitSystem.Enemies
{
    public class BossPatternController : MonoBehaviour
    {
        private enum PatternStep
        {
            Basic,
            GimmickStart,
            GimmickResolve,
            Punish,
            Weakened
        }

        [Header("Pattern")]
        [SerializeField] private SkillSO basicSkill;
        [SerializeField] private SkillSO gimmickStartSkill;
        [SerializeField] private SkillSO punishSkill;
        [SerializeField] private SkillSO weakenedSkill;

        [Header("Phase")]
        [SerializeField, Min(1)] private int basicCount = 2;
        [SerializeField, Min(1)] private int lowHealthBasicCount = 3;
        [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.25f;

        [Header("Weaken")]
        [SerializeField, Min(1)] private int weakenTurnsOnSuccess = 1;
        [SerializeField] private float weakenedDamageTakenMultiplier = 1.5f;

        [Header("Events")]
        [SerializeField] private UnityEvent gimmickStartedEvent;
        [SerializeField] private UnityEvent gimmickSucceededEvent;
        [SerializeField] private UnityEvent gimmickFailedEvent;
        [SerializeField] private UnityEvent weakenedStartedEvent;
        [SerializeField] private UnityEvent weakenedEndedEvent;

        private AbstractEnemyUnit _owner;
        private BaseSkill _boundSkill;
        private PatternStep _step = PatternStep.Basic;

        private int _basicUseCount;
        private int _weakenTurnCount;
        private bool _isGimmickActive;
        private bool _isGimmickResolved;
        private bool _isGimmickSuccess;

        public bool IsGimmickActive => _isGimmickActive;
        public bool IsWeakened => _step == PatternStep.Weakened;
        public float DamageTakenMultiplier => IsWeakened ? weakenedDamageTakenMultiplier : 1f;

        private void Awake()
        {
            _owner = GetComponent<AbstractEnemyUnit>();
        }

        private void OnEnable()
        {
            Bus<UnitTurnEndEvent>.Subscribe(HandleUnitTurnEnd);
        }

        private void OnDisable()
        {
            Bus<UnitTurnEndEvent>.Unsubscribe(HandleUnitTurnEnd);
            UnbindSkillEnd();
        }

        public bool TryApplyToPlan(EnemyPlan plan, Vector2Int from, IReadOnlyList<Unit> targets)
        {
            if (_owner == null || plan == null)
                return false;

            Unit target = ResolveTarget(plan, targets);

            if (target == null)
                return false;

            if (_step == PatternStep.GimmickResolve)
                ResolveGimmickStep();

            SkillSO skillSO = GetStepSkill(plan);

            if (_step == PatternStep.Weakened && skillSO == null)
            {
                plan.Clear();
                return true;
            }

            if (!TryGetUsableSkill(skillSO, from, target, out BaseSkill skill))
                return false;

            plan.SetCombatDecision(target, skillSO);
            BindSkillEnd(skill);
            return true;
        }

        public void CompleteGimmick(bool success)
        {
            if (!_isGimmickActive)
                return;

            _isGimmickActive = false;
            _isGimmickResolved = true;
            _isGimmickSuccess = success;

            if (success)
                gimmickSucceededEvent?.Invoke();
            else
                gimmickFailedEvent?.Invoke();
        }

        public void ResetPattern()
        {
            _step = PatternStep.Basic;
            _basicUseCount = 0;
            _weakenTurnCount = 0;
            _isGimmickActive = false;
            _isGimmickResolved = false;
            _isGimmickSuccess = false;
            UnbindSkillEnd();
        }

        private void ResolveGimmickStep()
        {
            if (!_isGimmickResolved)
                CompleteGimmick(false);

            if (_isGimmickSuccess)
            {
                StartWeakened();
                return;
            }

            _step = PatternStep.Punish;
        }

        private SkillSO GetStepSkill(EnemyPlan plan)
        {
            return _step switch
            {
                PatternStep.Basic => basicSkill != null ? basicSkill : plan.SelectedSkill,
                PatternStep.GimmickStart => gimmickStartSkill,
                PatternStep.Punish => punishSkill,
                PatternStep.Weakened => weakenedSkill,
                _ => null
            };
        }

        private bool TryGetUsableSkill(SkillSO skillSO, Vector2Int from, Unit target, out BaseSkill skill)
        {
            skill = null;

            if (skillSO == null || target == null || _owner.SkillCompo?.Skills == null)
                return false;

            if (!_owner.SkillCompo.Skills.TryGetValue(skillSO, out skill) || skill == null)
                return false;

            if (skill is not EnemySkill enemySkill)
                return false;

            return enemySkill.CanUseAt(from, target.gameObject);
        }

        private void BindSkillEnd(BaseSkill skill)
        {
            if (_boundSkill == skill)
                return;

            UnbindSkillEnd();

            _boundSkill = skill;
            _boundSkill.SkillEndEvent?.AddListener(HandlePatternSkillEnd);
        }

        private void UnbindSkillEnd()
        {
            if (_boundSkill == null)
                return;

            _boundSkill.SkillEndEvent?.RemoveListener(HandlePatternSkillEnd);
            _boundSkill = null;
        }

        private void HandlePatternSkillEnd()
        {
            UnbindSkillEnd();

            switch (_step)
            {
                case PatternStep.Basic:
                    AdvanceBasicStep();
                    break;
                case PatternStep.GimmickStart:
                    StartGimmick();
                    break;
                case PatternStep.Punish:
                    ResetPattern();
                    break;
                case PatternStep.Weakened:
                    ConsumeWeakenTurn();
                    break;
            }
        }

        private void AdvanceBasicStep()
        {
            ++_basicUseCount;

            if (_basicUseCount < CurrentBasicCount())
                return;

            _basicUseCount = 0;
            _step = PatternStep.GimmickStart;
        }

        private void StartGimmick()
        {
            _step = PatternStep.GimmickResolve;
            _isGimmickActive = true;
            _isGimmickResolved = false;
            _isGimmickSuccess = false;
            gimmickStartedEvent?.Invoke();
        }

        private void StartWeakened()
        {
            _step = PatternStep.Weakened;
            _weakenTurnCount = Mathf.Max(1, weakenTurnsOnSuccess);
            weakenedStartedEvent?.Invoke();
        }

        private void ConsumeWeakenTurn()
        {
            if (_step != PatternStep.Weakened)
                return;

            --_weakenTurnCount;

            if (_weakenTurnCount > 0)
                return;

            weakenedEndedEvent?.Invoke();
            ResetPattern();
        }

        private void HandleUnitTurnEnd(UnitTurnEndEvent evt)
        {
            if (!ReferenceEquals(evt.Unit, _owner))
                return;

            if (_step == PatternStep.Weakened && weakenedSkill == null)
                ConsumeWeakenTurn();
        }

        private int CurrentBasicCount()
        {
            if (_owner?.HealthCompo == null || _owner.HealthCompo.MaxHealth <= 0f)
                return Mathf.Max(1, basicCount);

            float healthRatio = _owner.HealthCompo.CurrentHealth / _owner.HealthCompo.MaxHealth;
            return healthRatio <= lowHealthThreshold
                ? Mathf.Max(1, lowHealthBasicCount)
                : Mathf.Max(1, basicCount);
        }

        private static Unit ResolveTarget(EnemyPlan plan, IReadOnlyList<Unit> targets)
        {
            if (plan.Target != null)
                return plan.Target;

            if (targets == null)
                return null;

            foreach (var target in targets)
            {
                if (target != null && target.gameObject.activeInHierarchy)
                    return target;
            }

            return null;
        }
    }
}

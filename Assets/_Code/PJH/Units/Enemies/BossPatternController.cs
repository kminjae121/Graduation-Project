using System.Collections;
using Code.Combat;
using Code.Core.Events.Bus;
using Code.SkillSystem;
using Code.UnitSystem.Enemies.AI;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UnitSystem.Enemies
{
    public class BossPatternController : EnemyPlannerProvider, IDamageTakenModifier
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
        [SerializeField] private SkillSO gimmickSkill;
        [SerializeField] private SkillSO punishSkill;

        [Header("Movement")]
        [SerializeField] private bool canMove = true;
        [SerializeField] private bool moveForSkill = true;
        [SerializeField] private bool moveInGimmick = true;

        [Header("Fallback")]
        [SerializeField] private bool useDefaultBasic = true;
        [SerializeField] private bool fallbackToDefault;

        [Header("Phase")]
        [SerializeField, Min(1)] private int basicCount = 2;
        [SerializeField, Min(1)] private int lowHpBasicCount = 3;
        [SerializeField, Range(0f, 1f)] private float lowHpThreshold = 0.25f;

        [Header("Weaken")]
        [SerializeField, Min(1)] private int weakTurns = 2;
        [SerializeField] private float weakDamageTakenRate = 2f;
        [SerializeField] private string weakDownAnimationKey = "DOWN";
        [SerializeField] private string weakKneeAnimationKey = "KNEE";
        [SerializeField] private string weakUpAnimationKey = "UP";
        [SerializeField, Min(0f)] private float downToKneeDelay = 0.8f;

        [Header("Events")]
        [SerializeField] private UnityEvent onGimmickStart;
        [SerializeField] private UnityEvent onGimmickSuccess;
        [SerializeField] private UnityEvent onGimmickFail;
        [SerializeField] private UnityEvent onWeakStart;
        [SerializeField] private UnityEvent onWeakEnd;

        private AbstractEnemyUnit _owner;
        private BossPlanner _planner;
        private PatternStep _step = PatternStep.Basic;

        private int _basicUses;
        private int _weakTurnsLeft;
        private bool _gimmickActive;
        private bool _gimmickResolved;
        private bool _gimmickSuccess;
        private Coroutine _weakPoseRoutine;

        public override EnemyPlannerBase Planner => _planner ??= new BossPlanner(this);
        public override bool ShouldSkipTurn => IsWeakened;
        public override bool SuppressHitAnimation => IsWeakened;

        public bool IsGimmickActive => _gimmickActive;
        public bool IsWeakened => _step == PatternStep.Weakened;
        public float DamageTakenMultiplier => IsWeakened ? weakDamageTakenRate : 1f;

        public int ModifyDamageTaken(int damage)
        {
            if (damage <= 0)
                return damage;

            return Mathf.Max(0, Mathf.RoundToInt(damage * DamageTakenMultiplier));
        }

        internal SkillSO PatternSkill => _step switch
        {
            PatternStep.Basic => basicSkill,
            PatternStep.GimmickStart => gimmickSkill,
            PatternStep.GimmickResolve => basicSkill,
            PatternStep.Punish => punishSkill,
            PatternStep.Weakened => null,
            _ => null
        };

        internal bool UseDefaultPlan
            => (_step == PatternStep.Basic || _step == PatternStep.GimmickResolve)
               && basicSkill == null && useDefaultBasic;

        internal bool FallbackToDefault => fallbackToDefault;

        internal bool CanMoveNow
        {
            get
            {
                if (!canMove || !moveForSkill)
                    return false;

                return _step switch
                {
                    PatternStep.GimmickStart or PatternStep.GimmickResolve => moveInGimmick,
                    PatternStep.Weakened => false,
                    _ => true
                };
            }
        }

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
            StopWeakPoseRoutine();
            Bus<UnitTurnEndEvent>.Unsubscribe(HandleUnitTurnEnd);
        }

        public override void OnSkillFinished(SkillSO skillSO, Unit target)
        {
            if (!ShouldAdvanceBySkill(skillSO))
                return;

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
            }
        }

        public void CompleteGimmick(bool success)
        {
            if (!_gimmickActive)
                return;

            _gimmickActive = false;

            if (success)
            {
                _gimmickResolved = false;
                _gimmickSuccess = false;
                onGimmickSuccess?.Invoke();
                StartWeakened();
                return;
            }

            _gimmickResolved = true;
            _gimmickSuccess = false;
            onGimmickFail?.Invoke();
        }

        public void ResetPattern()
        {
            StopWeakPoseRoutine();
            _step = PatternStep.Basic;
            _basicUses = 0;
            _weakTurnsLeft = 0;
            _gimmickActive = false;
            _gimmickResolved = false;
            _gimmickSuccess = false;
        }

        internal void PreparePlanStep()
        {
            if (_step == PatternStep.GimmickResolve)
                ResolveGimmickStep();
        }

        private bool ShouldAdvanceBySkill(SkillSO skillSO)
        {
            if (_step == PatternStep.GimmickResolve)
                return false;

            if (_step == PatternStep.Weakened)
                return false;

            if (_step == PatternStep.Basic && basicSkill == null && useDefaultBasic)
                return skillSO != null;

            return skillSO != null && skillSO == PatternSkill;
        }

        private void ResolveGimmickStep()
        {
            if (!_gimmickResolved)
                return;

            if (_gimmickSuccess)
            {
                StartWeakened();
                return;
            }

            _step = PatternStep.Punish;
        }

        private void AdvanceBasicStep()
        {
            ++_basicUses;

            if (_basicUses < CurrentBasicCount())
                return;

            _basicUses = 0;
            _step = PatternStep.GimmickStart;
        }

        private void StartGimmick()
        {
            _step = PatternStep.GimmickResolve;
            _gimmickActive = true;
            _gimmickResolved = false;
            _gimmickSuccess = false;
            onGimmickStart?.Invoke();
        }

        private void StartWeakened()
        {
            StopWeakPoseRoutine();
            _step = PatternStep.Weakened;
            _weakTurnsLeft = Mathf.Max(1, weakTurns);
            _gimmickResolved = false;
            _gimmickSuccess = false;
            onWeakStart?.Invoke();
            PlayWeakenAnimation(weakDownAnimationKey);
            _weakPoseRoutine = StartCoroutine(PlayKneeAfterDelay());
        }

        private void ConsumeWeakenTurn()
        {
            if (_step != PatternStep.Weakened)
                return;

            --_weakTurnsLeft;

            if (_weakTurnsLeft > 0)
                return;

            StopWeakPoseRoutine();
            PlayWeakenAnimation(weakUpAnimationKey);
            onWeakEnd?.Invoke();
            ResetPattern();
        }

        private IEnumerator PlayKneeAfterDelay()
        {
            if (downToKneeDelay > 0f)
                yield return new WaitForSeconds(downToKneeDelay);
            else
                yield return null;

            _weakPoseRoutine = null;

            if (IsWeakened)
                PlayWeakenAnimation(weakKneeAnimationKey);
        }

        private void PlayWeakenAnimation(string animationKey)
        {
            if (string.IsNullOrWhiteSpace(animationKey) || _owner?.UnitAnimator == null)
                return;

            _owner.UnitAnimator.PlaySelectAnimation(animationKey);
        }

        private void StopWeakPoseRoutine()
        {
            if (_weakPoseRoutine == null)
                return;

            StopCoroutine(_weakPoseRoutine);
            _weakPoseRoutine = null;
        }

        private void HandleUnitTurnEnd(UnitTurnEndEvent evt)
        {
            if (!ReferenceEquals(evt.Unit, _owner))
                return;

            if (IsWeakened)
                ConsumeWeakenTurn();
        }

        private int CurrentBasicCount()
        {
            if (_owner?.HealthCompo == null || _owner.HealthCompo.MaxHealth <= 0f)
                return Mathf.Max(1, basicCount);

            float healthRatio = _owner.HealthCompo.CurrentHealth / _owner.HealthCompo.MaxHealth;
            return healthRatio <= lowHpThreshold
                ? Mathf.Max(1, lowHpBasicCount)
                : Mathf.Max(1, basicCount);
        }
    }
}

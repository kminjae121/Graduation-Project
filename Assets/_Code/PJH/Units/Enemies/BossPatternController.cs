using Code.Core.Events.Bus;
using Code.SkillSystem;
using Code.UnitSystem.Enemies.AI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Code.UnitSystem.Enemies
{
    public class BossPatternController : EnemyPlannerProvider
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
        [SerializeField] private SkillSO weakSkill;

        [Header("Movement")]
        [SerializeField] private bool canMove = true;
        [SerializeField] private bool moveForSkill = true;
        [SerializeField] private bool moveInGimmick = true;
        [SerializeField] private bool moveInWeak;

        [Header("Fallback")]
        [SerializeField] private bool useDefaultBasic = true;
        [SerializeField] private bool fallbackToDefault;

        [Header("Phase")]
        [SerializeField, Min(1)] private int basicCount = 2;
        [SerializeField, Min(1)] private int lowHpBasicCount = 3;
        [SerializeField, Range(0f, 1f)] private float lowHpThreshold = 0.25f;

        [Header("Weaken")]
        [SerializeField, Min(1)] private int weakTurns = 1;
        [SerializeField] private bool skipInWeak = true;
        [SerializeField] private float weakDamageTakenRate = 1.5f;

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

        public override EnemyPlannerBase Planner => _planner ??= new BossPlanner(this);

        public bool IsGimmickActive => _gimmickActive;
        public bool IsWeakened => _step == PatternStep.Weakened;
        public float DamageTakenMultiplier => IsWeakened ? weakDamageTakenRate : 1f;

        internal SkillSO PatternSkill => _step switch
        {
            PatternStep.Basic => basicSkill,
            PatternStep.GimmickStart => gimmickSkill,
            PatternStep.Punish => punishSkill,
            PatternStep.Weakened => weakSkill,
            _ => null
        };

        internal bool UseDefaultPlan
            => _step == PatternStep.Basic && basicSkill == null && useDefaultBasic;

        internal bool FallbackToDefault => fallbackToDefault;

        internal bool SkipTurn
            => _step == PatternStep.Weakened && weakSkill == null && skipInWeak;

        internal bool CanMoveNow
        {
            get
            {
                if (!canMove || !moveForSkill)
                    return false;

                return _step switch
                {
                    PatternStep.GimmickStart or PatternStep.GimmickResolve => moveInGimmick,
                    PatternStep.Weakened => moveInWeak,
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
                case PatternStep.Weakened:
                    ConsumeWeakenTurn();
                    break;
            }
        }

        public void CompleteGimmick(bool success)
        {
            if (!_gimmickActive)
                return;

            _gimmickActive = false;
            _gimmickResolved = true;
            _gimmickSuccess = success;

            if (success)
                onGimmickSuccess?.Invoke();
            else
                onGimmickFail?.Invoke();
        }

        public void ResetPattern()
        {
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
            if (_step == PatternStep.Basic && basicSkill == null && useDefaultBasic)
                return skillSO != null;

            return skillSO != null && skillSO == PatternSkill;
        }

        private void ResolveGimmickStep()
        {
            if (!_gimmickResolved)
                CompleteGimmick(false);

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
            _step = PatternStep.Weakened;
            _weakTurnsLeft = Mathf.Max(1, weakTurns);
            onWeakStart?.Invoke();
        }

        private void ConsumeWeakenTurn()
        {
            if (_step != PatternStep.Weakened)
                return;

            --_weakTurnsLeft;

            if (_weakTurnsLeft > 0)
                return;

            onWeakEnd?.Invoke();
            ResetPattern();
        }

        private void HandleUnitTurnEnd(UnitTurnEndEvent evt)
        {
            if (!ReferenceEquals(evt.Unit, _owner))
                return;

            if (SkipTurn)
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

using System.Linq;
using Code.Core.Debugs;
using Code.Core.Events.Bus;
using Code.Managers;
using Code.Map;
using Code.SkillSystem;
using Code.UnitSystem.Enemies.AI;
using Code.UnitSystem.UnitComponent;
using Code.Utils;
using GondrLib.Dependencies;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Code.Core.Interfaces;

namespace Code.UnitSystem.Enemies
{
    public abstract class AbstractEnemyUnit : Unit
    {
        public BehaviorGraphAgent BTAgent { get; private set; }
        public PathMover PathMover { get; private set; }
        public EnemySkillComponent SkillCompo { get; private set; }
        public TurnChannel TurnChannel { get; private set; }
        public UnitAnimation UnitAnimator { get; private set; }
        public UnitRotator UnitRotatorCompo { get; private set; }
        public UnitAnimationTrigger AnimationTrigger { get; private set; }
        public EnemyAIProfileSO AIProfile => aiProfile;
        public EnemyManager EnemyManager => _enemyManager;
        public UnitManager UnitManager => _unitManager;
        
        protected GridMap GridMapInstance { get; private set; }
        protected Unit CurrentTarget { get; private set; }

        [SerializeField] private EnemyAIProfileSO aiProfile;

        [Inject] protected EnemyManager _enemyManager;
        [Inject] protected UnitManager _unitManager;

        private bool _hasEndedTurn;
        private bool _isDead;
        
        private readonly Vector3 _dampingSpeed = new(1.5f, 1.5f, 1.5f);

        protected override void Awake()
        {
            base.Awake();
            
            BTAgent = GetComponent<BehaviorGraphAgent>();
        }

        protected override void AfterInitComponents()
        {
            base.AfterInitComponents();
            
            PathMover = GetUnitCompo<PathMover>();
            SkillCompo = GetUnitCompo<EnemySkillComponent>();
            UnitAnimator = GetUnitCompo<UnitAnimation>();
            UnitRotatorCompo = GetUnitCompo<UnitRotator>();
            AnimationTrigger = GetUnitCompo<UnitAnimationTrigger>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _isDead = false;

            if (AnimationTrigger != null)
                AnimationTrigger.OnDeadEvent += HandleDeathAnimEnd;
        }

        protected override void OnDisable()
        {
            if (AnimationTrigger != null)
                AnimationTrigger.OnDeadEvent -= HandleDeathAnimEnd;

            base.OnDisable();
        }

        protected virtual void Start()
        {
            SetVariableValue(BTVars.UnitAnimator, UnitAnimator);

            if (GetVariableValue(BTVars.TurnChannel, out BlackboardVariable<TurnChannel> targetChannel))
                TurnChannel = targetChannel.Value;

            GridMapInstance = GridMap.Instance;
            UpdateTargetBlackboard();
        }

        public override void OnTurnStart()
        {
            _hasEndedTurn = false;
            base.OnTurnStart();

            if (!PrepareTurnStart())
            {
                StartCoroutine(EndTurnNextFrame());
                return;
            }
            
            Bus<UnitCamSettingEvent>.Raise(new UnitCamSettingEvent(gameObject, false, _dampingSpeed));

            StartCoroutine(WaitActive());
        }
        
        private IEnumerator WaitActive()
        {
            yield return new WaitForSeconds(0.4f);
            
            TurnChannel?.SendEventMessage();
        }

        private IEnumerator EndTurnNextFrame()
        {
            yield return null;

            if (this != null && gameObject.activeInHierarchy)
                OnTurnEnd();
        }

        public override void OnTurnEnd()
        {
            if (_hasEndedTurn)
                return;

            _hasEndedTurn = true;
            Bus<UnitCamSettingEvent>.Raise(new UnitCamSettingEvent(null,
                false, new Vector3(0.1f, 0.1f, 0.1f)));
            
            base.OnTurnEnd();
        }

        protected override void Dead()
        {
            if (_isDead)
                return;

            _isDead = true;
            base.Dead();

            if (UnitAnimator == null || AnimationTrigger == null)
            {
                HandleDeathAnimEnd();
                return;
            }

            UnitAnimator.PlaySelectAnimation("DIE");
        }

        protected override void Hit()
        {
            if (_isDead)
                return;

            if (UnitAnimator != null)
            {
                UnitAnimator.RestartFromEntry();
                UnitAnimator.PlaySelectAnimation("HIT");
            }

            StartCoroutine(HitIdle());

            base.Hit();
        }

        private IEnumerator HitIdle()
        {
            yield return new WaitForSeconds(0.4f);
            
            if(UnitAnimator != null)
            {
                UnitAnimator.ReturnIdleAnimation();
            }
        }

        private void HandleDeathAnimEnd()
        {
            if (!_isDead)
                return;

            _isDead = false;
            ClearCurrentTile();

            if (Core.Managers.StageManager.Instance != null)
                Core.Managers.StageManager.Instance.RemoveEnemy(gameObject);

            gameObject.SetActive(false);
        }

        private void ClearCurrentTile()
        {
            Vector2Int gridPos = GridMapInstance.WorldToGridPos(transform.position);
            IMapTile currentTile = GridMapInstance.GetTile(gridPos);

            if (currentTile == null)
                return;

            currentTile.SetState(TileState.Enemy, false);
            currentTile.SetState(TileState.Obstacle, false);
            currentTile.SetState(TileState.Walkable, true);
        }

        protected virtual bool PrepareTurnStart()
            => UpdateTargetBlackboard();

        public void OrderSkill(SkillSO skillSO, GameObject target, System.Action onComplete)
        {
            if (!TryGetSkill(skillSO, out SkillSO selectedSkillSO, out BaseSkill selectedSkill))
            {
                onComplete?.Invoke();
                return;
            }

            EnemyAttack(selectedSkillSO, selectedSkill, target, onComplete);
        }

        private bool TryGetSkill(SkillSO skillSO, out SkillSO selectedSkillSO, out BaseSkill selectedSkill)
        {
            selectedSkillSO = null;
            selectedSkill = null;

            if (SkillCompo?.Skills == null || SkillCompo.Skills.Count == 0)
            {
                UnityLogger.LogError($"[{nameof(AbstractEnemyUnit)}] {name} has no registered skills.");
                return false;
            }

            if (skillSO != null && SkillCompo.Skills.TryGetValue(skillSO, out BaseSkill exactSkill) && exactSkill != null)
            {
                selectedSkillSO = skillSO;
                selectedSkill = exactSkill;
                return true;
            }

            foreach (var pair in SkillCompo.Skills)
            {
                if (pair.Key == null || pair.Value == null)
                    continue;

                selectedSkillSO = pair.Key;
                selectedSkill = pair.Value;
                return true;
            }

            UnityLogger.LogError($"[{nameof(AbstractEnemyUnit)}] {name} could not resolve a skill to execute.");
            return false;
        }

        private void EnemyAttack(SkillSO skillSO, BaseSkill skill, GameObject target, System.Action onComplete)
        {
            UnityAction endListener = null;
            endListener = () =>
            {
                skill.SkillEndEvent?.RemoveListener(endListener);
                onComplete?.Invoke();
            };

            skill.SkillEndEvent?.AddListener(endListener);
            skill.RotatorCompo = UnitRotatorCompo;
            skill.ConfigureSkillRange(skillSO);
            skill.ForceUseSkill(target);
        }

        public bool CanUseSkillOnTarget(SkillSO skillSO, GameObject target)
        {
            if (target == null || SkillCompo?.Skills == null || SkillCompo.Skills.Count == 0)
            {
                UnityLogger.LogError($"[{nameof(AbstractEnemyUnit)}] {name} cannot check skill range without target or skills.");
                return false;
            }

            if (!TryGetSkill(skillSO, out _, out var selectedSkill))
                return false;

            if (selectedSkill is not EnemyBaseSkill enemySkill)
            {
                UnityLogger.LogError($"[{nameof(AbstractEnemyUnit)}] {name} tried to evaluate a non-enemy skill.");
                return false;
            }

            return enemySkill.CanUse(target);
        }

        public bool TrySelectAttackSkill(GameObject target, out SkillSO selectedSkillSO)
        {
            selectedSkillSO = null;

            if (EnemyManager == null)
                return false;

            EnemyManager.RefreshPlan(this);
            if (!EnemyManager.TryGetPlan(this, out EnemyPlan plan) || plan.Target == null || plan.SelectedSkill == null)
                return false;

            if (target != null && plan.Target.gameObject != target)
                return false;

            selectedSkillSO = plan.SelectedSkill;
            return true;
        }

        protected virtual bool UpdateTargetBlackboard()
        {
            EnemyPlan plan = null;

            if (EnemyManager != null)
            {
                EnemyManager.RefreshPlan(this);
                EnemyManager.TryGetPlan(this, out plan);
            }

            CurrentTarget = plan?.Target ?? FindClosestPlayerTarget();
            SetVariableValue(BTVars.Target, CurrentTarget != null ? CurrentTarget.gameObject : null);
            return CurrentTarget != null;
        }

        protected virtual Unit FindClosestPlayerTarget()
        {
            if (GridMapInstance == null || UnitManager == null)
                return null;

            Vector2Int myPos = GridMapInstance.WorldToGridPos(transform.position);

            return UnitManager.GetPlayerUnits()
                .Where(unit => unit != null && unit.gameObject.activeInHierarchy)
                .OrderBy(unit => DistanceUtils.GetEuclideanDistance(myPos,
                    GridMapInstance.WorldToGridPos(unit.transform.position)))
                .FirstOrDefault();
        }

        public void SetVariableValue<T>(string variableName, T value)
        {
            Debug.Assert(!string.IsNullOrEmpty(variableName), "Variable name is empty");

            if (BTAgent.GetVariable(variableName, out BlackboardVariable<T> variable))
                variable.Value = value;
            else
                UnityLogger.LogError($"Variable {variableName} not found");
        }

        public bool GetVariableValue<T>(string variableName, out BlackboardVariable<T> variable)
        {
            Debug.Assert(!string.IsNullOrEmpty(variableName), "Variable name is empty");
            return BTAgent.GetVariable(variableName, out variable);
        }
    }
}

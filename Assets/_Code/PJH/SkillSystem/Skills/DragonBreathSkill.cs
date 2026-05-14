using System.Collections.Generic;
using Code.Combat.StatusEffect;
using Code.Core.Debugs;
using Code.Core.Events.Bus;
using Code.Managers;
using Code.Map;
using Code.UnitSystem;
using Code.UnitSystem.Enemies;
using Code.UnitSystem.Enemies.AI;
using UnityEngine;

namespace Code.SkillSystem
{
    public class DragonBreathSkill : EnemyBaseSkill
    { 
        [SerializeField] private int pierceLength = 3;
        [SerializeField] private int burnDuration = 2;
        [SerializeField] private int burnDamage = 5;

        private GameObject _target;
        private AbstractEnemyUnit _ownerEnemy;
        private UnitManager _unitManager;

        private void Awake()
        {
            _ownerEnemy = GetComponentInParent<AbstractEnemyUnit>();
            triggerCompo = _ownerEnemy.GetUnitCompo<UnitAnimationTrigger>();
        }

        protected void Start()
        {
            SkillEvent.AddListener(AttackAction);
            
            if (_ownerEnemy != null)
                _unitManager = _ownerEnemy.UnitManager;
        }

        public override void ForceUseSkill(GameObject target)
        {
            if (target == null)
                return;

            _targetEnemy = target;
            isCanUseSkill = true;

            if (RotatorCompo != null)
            {
                RotatorCompo.SetDir(target.transform.position, BeginBreathSkill);
                return;
            }

            BeginBreathSkill();
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
            if (_target == null)
                return;

            foreach (var hitTarget in GetHitTargets(_target))
            {
                Bus<DamageEvent>.Raise(new DamageEvent(DamageData, hitTarget, AddDamage,
                    null, false,false,0.1f));

                if (burnDuration <= 0 || burnDamage <= 0)
                    continue;

                if (!hitTarget.TryGetComponent(out Unit targetUnit))
                    continue;

                Bus<ApplyStatusEffectEvent>.Raise(new ApplyStatusEffectEvent(targetUnit, EffectType.Burn,
                    new StatusEffectApplyData(burnDuration, burnDamage)));
            }
        }

        private bool CanHitTargetFromPos(Vector2Int origin, GameObject target)
        {
            if (target == null)
                return false;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
                return false;

            Vector2Int targetPos = gridMap.WorldToGridPos(target.transform.position);
            Vector2Int forwardDir = GetForwardDir(origin, targetPos);

            if (forwardDir == Vector2Int.zero)
                return false;

            for (int i = 1; i <= pierceLength; ++i)
                if (origin + (forwardDir * i) == targetPos)
                    return true;

            return false;
        }

        public override bool CanUse(GameObject target)
            => CanUseAt(GetCasterGridPos(), target);

        public override bool CanUseAt(Vector2Int sourcePos, GameObject target)
        {
            if (!CanHitTargetFromPos(sourcePos, target))
                return false;

            var gridMap = GridMap.Instance;

            if (gridMap == null || target == null)
                return false;

            return PassRange(sourcePos, gridMap.WorldToGridPos(target.transform.position), false);
        }

        public int GetPredictedHitCount(GameObject target)
            => GetHitTargetsFromPos(GetCasterGridPos(), target).Count;

        private int GetPredictedHitCountFromPos(Vector2Int sourcePos, GameObject target)
            => GetHitTargetsFromPos(sourcePos, target).Count;

        public override float ScoreAt(Vector2Int sourcePos, GameObject target, EnemyAIProfileSO ai)
        {
            if (target == null || SkillSO == null || !CanUseAt(sourcePos, target))
                return float.MinValue;

            int predictedHitCount = GetPredictedHitCountFromPos(sourcePos, target);
            
            if (predictedHitCount <= 0)
                return float.MinValue;

            return MakeScore(predictedHitCount * SkillSO.SkillDamage, sourcePos, target, ai);
        }

        private List<GameObject> GetHitTargets(GameObject target)
            => GetHitTargetsFromPos(GetCasterGridPos(), target);

        private List<GameObject> GetHitTargetsFromPos(Vector2Int origin, GameObject target)
        {
            var hitTargets = new List<GameObject>();

            if (target == null)
                return hitTargets;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
            {
                UnityLogger.LogError($"[{nameof(DragonBreathSkill)}] GridMap is missing.");
                return hitTargets;
            }

            if (_ownerEnemy != null)
                _unitManager = _ownerEnemy.UnitManager;
            
            if (_unitManager == null)
            {
                UnityLogger.LogError($"[{nameof(DragonBreathSkill)}] UnitManager is missing.");
                return hitTargets;
            }

            Vector2Int targetPos = gridMap.WorldToGridPos(target.transform.position);
            Vector2Int forwardDir = GetForwardDir(origin, targetPos);

            if (forwardDir == Vector2Int.zero)
                return hitTargets;

            var hitTargetSet = new HashSet<GameObject>();

            for (int i = 1; i <= pierceLength; ++i)
            {
                Vector2Int hitPos = origin + forwardDir * i;

                foreach (var unit in _unitManager.GetPlayerUnits())
                {
                    if (unit == null)
                        continue;

                    if (gridMap.WorldToGridPos(unit.transform.position) != hitPos)
                        continue;

                    if (!hitTargetSet.Add(unit.gameObject))
                        continue;

                    hitTargets.Add(unit.gameObject);
                }
            }

            return hitTargets;
        }

        private Vector2Int GetCasterGridPos()
        {
            GridMap gridMap = GridMap.Instance;

            if (gridMap == null)
                return Vector2Int.zero;

            return gridMap.WorldToGridPos(GetCasterWorldPos());
        }

        private void SkillEnd()
        {
            triggerCompo.OnAttackTrigger -= TakeDamage;
            triggerCompo.OnAnimationEndTrigger -= SkillEnd;
            _target = null;
            SkillFinished(false);
            SkillEndEvent?.Invoke();
        }

        private void BeginBreathSkill()
        {
            if (_targetEnemy == null)
                return;

            StartEvent();
            Bus<UnitSkilStartEvent>.Raise(new UnitSkilStartEvent(true));
            SkillEvent?.Invoke(_targetEnemy);
            PlayBreathAnimation();
        }

        private void PlayBreathAnimation()
        {
            if (_ownerEnemy?.UnitAnimator == null || string.IsNullOrWhiteSpace(SkillSO.skillAnimationKey))
                return;
            
            _ownerEnemy.UnitAnimator.RestartFromEntry();
            _ownerEnemy.UnitAnimator.PlaySelectAnimation(SkillSO.skillAnimationKey);
            SkillFeedbackEvent?.Invoke();
        }

        private static Vector2Int GetForwardDir(Vector2Int origin, Vector2Int target)
        {
            Vector2Int delta = target - origin;

            if (delta == Vector2Int.zero)
                return Vector2Int.zero;

            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                return new Vector2Int(delta.x > 0 ? 1 : -1, 0);

            return new Vector2Int(0, delta.y > 0 ? 1 : -1);
        }
    }
}

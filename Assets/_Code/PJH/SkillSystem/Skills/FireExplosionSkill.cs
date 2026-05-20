using System.Collections.Generic;
using Code.Combat.StatusEffect;
using Code.Core.Debugs;
using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.Map;
using Code.UnitSystem;
using Code.UnitSystem.Enemies.AI;
using UnityEngine;

namespace Code.SkillSystem
{
    public class FireExplosionSkill : EnemyBaseSkill
    {
        [SerializeField] private int radius = 1;

        private UnitManager _unitManager;

        protected override void Awake()
        {
            base.Awake();

            if (Owner != null)
                _unitManager = Owner.UnitManager;
        }

        protected override void OnAttack(GameObject target)
        {
            if (target == null)
                return;

            SkillFeedbackEvent?.Invoke();
            Owner.VFXCompo.PlayVFX("FireExplosion", target.transform.position, Quaternion.identity);
            
            foreach (var hitTarget in GetHitTargets(target))
            {
                Bus<DamageEvent>.Raise(new DamageEvent(DamageData, hitTarget, AddDamage, null, false, false, 0.1f));
                Bus<ApplyStatusEffectEvent>.Raise(new ApplyStatusEffectEvent(target.GetComponent<Unit>(), EffectType.Burn, new StatusEffectApplyData(2, 5)));
            }
        }

        public override bool CanUseAt(Vector2Int sourcePos, GameObject target)
        {
            if (target == null || SkillSO == null)
                return false;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
                return false;

            return PassRange(sourcePos, gridMap.WorldToGridPos(target.transform.position));
        }

        public override float ScoreAt(Vector2Int sourcePos, GameObject target, EnemyAIProfileSO ai)
        {
            if (target == null || SkillSO == null || !CanUseAt(sourcePos, target))
                return float.MinValue;

            int hitCount = GetHitTargets(target).Count;

            if (hitCount <= 0)
                return float.MinValue;

            return MakeScore(hitCount * SkillSO.SkillDamage, sourcePos, target, ai);
        }

        private List<GameObject> GetHitTargets(GameObject target)
        {
            var hitTargets = new List<GameObject>();

            if (target == null)
                return hitTargets;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
            {
                UnityLogger.LogError($"[{nameof(FireExplosionSkill)}] GridMap is missing.");
                return hitTargets;
            }

            if (Owner != null)
                _unitManager = Owner.UnitManager;

            if (_unitManager == null)
            {
                UnityLogger.LogError($"[{nameof(FireExplosionSkill)}] UnitManager is missing.");
                return hitTargets;
            }

            Vector2Int center = gridMap.WorldToGridPos(target.transform.position);
            int size = Mathf.Max(0, radius);

            foreach (var unit in _unitManager.GetPlayerUnits())
            {
                if (unit == null)
                    continue;

                Vector2Int unitPos = gridMap.WorldToGridPos(unit.transform.position);

                if (Mathf.Abs(unitPos.x - center.x) > size || Mathf.Abs(unitPos.y - center.y) > size)
                    continue;

                hitTargets.Add(unit.gameObject);
            }

            return hitTargets;
        }
    }
}
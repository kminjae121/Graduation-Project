using System.Collections.Generic;
using Code.Core.Debugs;
using Code.Map;
using Code.UnitSystem.Enemies.AI;
using UnityEngine;

namespace Code.SkillSystem
{
    public abstract class EnemyAreaBaseSkill : EnemyAttackBaseSkill
    {
        [SerializeField] private int radius = 1;

        protected List<GameObject> GetAreaTargets(GameObject target)
        {
            var gridMap = GridMap.Instance;

            if (gridMap == null || target == null)
                return new List<GameObject>();

            return GetAreaTargetsAt(gridMap.WorldToGridPos(target.transform.position));
        }

        protected List<GameObject> GetAreaTargetsAt(Vector2Int center)
        {
            var targets = new List<GameObject>();
            var gridMap = GridMap.Instance;

            if (gridMap == null)
            {
                UnityLogger.LogError($"[{GetType().Name}] GridMap is missing.");
                return targets;
            }

            int size = Mathf.Max(0, radius);
            var unitManager = UnitManager;

            if (unitManager == null)
            {
                UnityLogger.LogError($"[{GetType().Name}] UnitManager is missing.");
                return targets;
            }

            foreach (var unit in unitManager.GetPlayerUnits())
            {
                if (unit == null)
                    continue;

                Vector2Int unitPos = gridMap.WorldToGridPos(unit.transform.position);

                if (Mathf.Abs(unitPos.x - center.x) > size || Mathf.Abs(unitPos.y - center.y) > size)
                    continue;

                targets.Add(unit.gameObject);
            }

            return targets;
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

            int hitCount = GetAreaTargets(target).Count;

            if (hitCount <= 0)
                return float.MinValue;

            return MakeScore(hitCount * SkillSO.SkillDamage, sourcePos, target, ai);
        }
    }
}

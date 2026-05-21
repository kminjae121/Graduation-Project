using System.Collections.Generic;
using Code.Core.Debugs;
using Code.Map;
using Code.UnitSystem.Enemies.AI;
using UnityEngine;

namespace Code.SkillSystem
{
    public abstract class EnemyLineSkillBase : EnemyBaseSkill
    {
        [SerializeField] private int pierceLength;
        [SerializeField] private bool useMaxRange;

        protected List<GameObject> GetLineTargets(GameObject target)
            => GetLineTargetsFromPos(GetCasterGridPos(), target);

        protected List<GameObject> GetLineTargetsFromPos(Vector2Int origin, GameObject target)
        {
            var targets = new List<GameObject>();

            if (target == null)
                return targets;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
            {
                UnityLogger.LogError($"[{GetType().Name}] GridMap is missing.");
                return targets;
            }

            Vector2Int targetPos = gridMap.WorldToGridPos(target.transform.position);
            Vector2Int forwardDir = GetForwardDir(origin, targetPos);

            if (forwardDir == Vector2Int.zero)
                return targets;

            var hitTargetSet = new HashSet<GameObject>();
            int length = Mathf.Max(1, pierceLength);

            for (int i = 1; i <= length; ++i)
            {
                Vector2Int hitPos = origin + forwardDir * i;

                foreach (var unit in UnitManager.GetPlayerUnits())
                {
                    if (unit == null)
                        continue;

                    if (gridMap.WorldToGridPos(unit.transform.position) != hitPos)
                        continue;

                    if (!hitTargetSet.Add(unit.gameObject))
                        continue;

                    targets.Add(unit.gameObject);
                }
            }

            return targets;
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

            return PassRange(sourcePos, gridMap.WorldToGridPos(target.transform.position), useMaxRange);
        }

        public override float ScoreAt(Vector2Int sourcePos, GameObject target, EnemyAIProfileSO ai)
        {
            if (target == null || SkillSO == null || !CanUseAt(sourcePos, target))
                return float.MinValue;

            int hitCount = GetLineTargetsFromPos(sourcePos, target).Count;

            if (hitCount <= 0)
                return float.MinValue;

            return MakeScore(hitCount * SkillSO.SkillDamage, sourcePos, target, ai);
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

            int length = Mathf.Max(1, pierceLength);

            for (int i = 1; i <= length; ++i)
                if (origin + forwardDir * i == targetPos)
                    return true;

            return false;
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
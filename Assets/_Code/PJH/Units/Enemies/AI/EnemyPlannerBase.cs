using System.Collections.Generic;
using Code.Map;
using Code.SkillSystem;
using Code.Utils;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public abstract class EnemyPlannerBase
    {
        public abstract void Build(EnemyPlan plan, AbstractEnemyUnit enemy, Vector2Int from,
            IReadOnlyList<Unit> targets, IReadOnlyList<EnemyMoveTile> tiles, EnemyRouteMap routes);

        protected static Unit PickClosestTarget(Vector2Int from, IReadOnlyList<Unit> targets, EnemyRouteMap routes)
        {
            if (targets == null || GridMap.Instance == null)
                return null;

            Unit closest = null;
            var bestCost = int.MaxValue;
            var bestDistance = float.MaxValue;

            foreach (var target in targets)
            {
                if (target == null)
                    continue;

                float distance = DistanceUtils.GetEuclideanDistance(from,
                    GridMap.Instance.WorldToGridPos(target.transform.position));

                int cost = 0;
                bool hasRoute = routes != null && routes.TryGetCost(target, from, out cost);

                if (hasRoute)
                {
                    if (closest != null && cost > bestCost)
                        continue;

                    if (closest != null && cost == bestCost && distance >= bestDistance)
                        continue;

                    closest = target;
                    bestCost = cost;
                    bestDistance = distance;
                    continue;
                }

                if (bestCost != int.MaxValue)
                    continue;

                if (closest != null && distance >= bestDistance)
                    continue;

                closest = target;
                bestDistance = distance;
            }

            return closest;
        }

        protected static bool TryGetEnemySkill(AbstractEnemyUnit enemy, SkillSO skillSO, out EnemySkill skill)
        {
            skill = null;

            if (enemy?.SkillCompo?.Skills == null || skillSO == null)
                return false;

            if (!enemy.SkillCompo.Skills.TryGetValue(skillSO, out BaseSkill baseSkill))
                return false;

            skill = baseSkill as EnemySkill;
            return skill != null;
        }
    }
}
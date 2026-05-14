using Code.Map;
using Code.Utils;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public static class EnemyMoveScorer
    {
        private const float LeaveDeadRange = 1000f;
        private const float CanUse = 500f;
        private const float RetreatPos = 10f;

        public static EnemyMoveEval Space(EnemyMoveTile tile, EnemySkillPick pick, GameObject target, Vector2Int targetPos)
        {
            if (!pick.Skill.CanUseAt(tile.Pos, target) || pick.Skill.WantsMove(tile.Pos, target))
                return EnemyMoveEval.Invalid;

            return new EnemyMoveEval(tile.Pos, pick.Skill.PosScore(tile.Pos, target), tile.Cost,
                DistanceUtils.GetManhattanDistance(tile.Pos, targetPos));
        }

        public static EnemyMoveEval Retreat(EnemyMoveTile tile, EnemySkillPick pick, GameObject target, Vector2Int targetPos, EnemyThreatMap threats, float currentThreatDist)
        {
            float dist = DistanceUtils.GetManhattanDistance(tile.Pos, targetPos);
            float threatDist = threats.MinDist(tile.Pos, dist);

            if (threatDist <= currentThreatDist)
                return EnemyMoveEval.Invalid;

            float score = threatDist;

            if (!pick.Skill.TooClose(tile.Pos, target))
                score += LeaveDeadRange;

            if (pick.Skill.CanUseAt(tile.Pos, target))
                score += CanUse;

            score += pick.Skill.PosScore(tile.Pos, target) * RetreatPos;

            return new EnemyMoveEval(tile.Pos, score, tile.Cost, threatDist);
        }

        public static EnemyMoveEval Approach(EnemyMoveTile tile, Unit target, Vector2Int targetPos,
            EnemyRouteMap routes, bool hasCurrentRoute, int currentRoute, float currentDist)
        {
            float dist = DistanceUtils.GetManhattanDistance(tile.Pos, targetPos);
            int route = 0;
            bool hasRoute = false;

            if (routes != null)
                hasRoute = routes.TryGetCost(target, tile.Pos, out route);

            if (hasRoute)
            {
                if (hasCurrentRoute && route >= currentRoute)
                    return EnemyMoveEval.Invalid;

                return EnemyMoveEval.WithRoute(tile.Pos, route, tile.Cost, dist);
            }

            if (hasCurrentRoute || dist > currentDist)
                return EnemyMoveEval.Invalid;

            return EnemyMoveEval.WithDist(tile.Pos, tile.Cost, dist);
        }
    }
}
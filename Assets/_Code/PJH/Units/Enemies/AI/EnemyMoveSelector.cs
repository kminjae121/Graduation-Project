using System.Collections.Generic;
using Code.Map;
using Code.Utils;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public sealed class EnemyMoveSelector
    {
        public bool TrySkillTile(AbstractEnemyUnit enemy, IReadOnlyList<EnemyMoveOption> options, out EnemyMovePick move)
        {
            move = default;

            if (enemy == null || options == null)
                return false;

            var best = default(EnemyMoveOption);
            var found = false;

            foreach (var option in options)
            {
                if (!option.IsValid || !EnemyMoveRanker.SkillTile(enemy, option, best, found))
                    continue;

                best = option;
                found = true;
            }

            if (!found)
                return false;

            move = new EnemyMovePick(best.Target, best.Tile);
            return true;
        }

        public bool TrySpaceTile(AbstractEnemyUnit enemy, Vector2Int from, EnemySkillPick pick, IReadOnlyList<EnemyMoveTile> tiles, out Vector2Int selectedTile)
        {
            selectedTile = default;

            if (enemy == null || !pick.IsValid || tiles == null || GridMap.Instance == null)
                return false;

            var target = pick.Target.gameObject;
            Vector2Int targetPos = GridMap.Instance.WorldToGridPos(pick.Target.transform.position);
            var best = EnemyMoveEval.Invalid;

            foreach (var tile in tiles)
            {
                if (tile.Pos == from)
                    continue;

                EnemyMoveEval score = EnemyMoveScorer.Space(tile, pick, target, targetPos);

                if (EnemyMoveRanker.Space(score, best))
                    best = score;
            }

            return TrySet(best, out selectedTile);
        }

        public bool TryRetreatTile(AbstractEnemyUnit enemy, Vector2Int from, EnemySkillPick pick,
            IReadOnlyList<EnemyMoveTile> tiles, out Vector2Int selectedTile)
            => TryRetreatTile(enemy, from, pick, null, tiles, out selectedTile);

        public bool TryRetreatTile(AbstractEnemyUnit enemy, Vector2Int from, EnemySkillPick pick, IReadOnlyList<Unit> threats, IReadOnlyList<EnemyMoveTile> tiles, out Vector2Int selectedTile)
        {
            selectedTile = default;

            if (enemy == null || !pick.IsValid || tiles == null || GridMap.Instance == null)
                return false;

            var target = pick.Target.gameObject;
            Vector2Int targetPos = GridMap.Instance.WorldToGridPos(pick.Target.transform.position);
            float currentDist = DistanceUtils.GetManhattanDistance(from, targetPos);
            var threatMap = new EnemyThreatMap(threats);
            float currentThreatDist = threatMap.MinDist(from, currentDist);
            var best = EnemyMoveEval.Invalid;

            foreach (var tile in tiles)
            {
                if (tile.Pos == from)
                    continue;

                EnemyMoveEval score = EnemyMoveScorer.Retreat(tile, pick, target, targetPos, threatMap, currentThreatDist);

                if (EnemyMoveRanker.Retreat(score, best))
                    best = score;
            }

            return TrySet(best, out selectedTile);
        }

        public bool TryApproachTile(Vector2Int from, Unit target, Vector2Int targetPos, IReadOnlyList<EnemyMoveTile> tiles, EnemyRouteMap routes, out Vector2Int selectedTile)
        {
            selectedTile = default;

            if (target == null || tiles == null)
                return false;

            int currentRoute = 0;
            bool hasRoute = false;

            if (routes != null)
                hasRoute = routes.TryGetCost(target, from, out currentRoute);

            float currentDist = DistanceUtils.GetManhattanDistance(from, targetPos);
            var best = EnemyMoveEval.Invalid;

            foreach (var tile in tiles)
            {
                if (tile.Pos == from)
                    continue;

                EnemyMoveEval score = EnemyMoveScorer.Approach(tile, target, targetPos, routes, hasRoute,
                    currentRoute, currentDist);

                if (EnemyMoveRanker.Approach(score, best))
                    best = score;
            }

            return TrySet(best, out selectedTile);
        }

        private static bool TrySet(EnemyMoveEval score, out Vector2Int tile)
        {
            tile = score.Pos;
            return score.IsValid;
        }
    }
}
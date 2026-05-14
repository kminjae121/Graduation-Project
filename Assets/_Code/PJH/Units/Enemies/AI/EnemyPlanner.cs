using System.Collections.Generic;
using Code.Map;
using Code.Utils;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public sealed class EnemyPlanner
    {
        private readonly EnemySkillSelector _skills = new();
        private readonly EnemyMoveSelector _moves = new();

        public void Build(EnemyPlan plan, AbstractEnemyUnit enemy, Vector2Int from,
            IReadOnlyList<Unit> targets, IReadOnlyList<EnemyMoveTile> tiles, EnemyRouteMap routes)
        {
            if (plan == null || enemy == null || targets == null || targets.Count == 0)
                return;

            bool canKeepSpace = CanKeepSpace(enemy);
            bool hasPick = _skills.TryBest(enemy, from, targets, out EnemySkillPick pick);

            // 거리 유지형 적은 일반 공격 판단보다 가까운 위협을 먼저 처리한다.
            if (canKeepSpace && _skills.TryCloseThreat(enemy, from, targets, out EnemySkillPick threatPick))
            {
                plan.SetTarget(threatPick.Target);

                if (_moves.TryRetreatTile(enemy, from, threatPick, targets, tiles, out Vector2Int threatRetreatTile))
                {
                    plan.SetMoveTile(threatRetreatTile);
                    return;
                }

                if (hasPick)
                    plan.SetCombatDecision(pick.Target, pick.SkillSO);

                return;
            }

            bool wantsMove = hasPick && canKeepSpace && pick.Skill.WantsMove(from, pick.Target.gameObject);

            // 선택한 스킬을 바로 쓸 수 있고 거리 문제가 없으면 즉시 공격한다.
            if (hasPick && !wantsMove)
            {
                plan.SetCombatDecision(pick.Target, pick.SkillSO);
                return;
            }

            // 스킬 사용이 가능하면서 위험 거리에서 벗어나는 칸을 우선한다.
            if (hasPick && _moves.TrySpaceTile(enemy, from, pick, tiles, out Vector2Int spaceTile))
            {
                plan.SetTarget(pick.Target);
                plan.SetMoveTile(spaceTile);
                return;
            }

            // 이상적인 거리 유지 칸이 없으면 일단 더 멀어지는 이동을 시도한다.
            if (wantsMove && _moves.TryRetreatTile(enemy, from, pick, tiles, out Vector2Int retreatTile))
            {
                plan.SetTarget(pick.Target);
                plan.SetMoveTile(retreatTile);
                return;
            }

            // 거리 확보에 실패해도 스킬을 쓸 수 있으면 공격한다.
            if (hasPick)
            {
                plan.SetCombatDecision(pick.Target, pick.SkillSO);
                return;
            }

            // 현재 위치에서 쓸 스킬은 없지만 대상이 너무 가까우면 후퇴를 시도한다.
            if (canKeepSpace && _skills.TryTooClose(enemy, from, targets, out EnemySkillPick closePick))
            {
                plan.SetTarget(closePick.Target);

                if (_moves.TryRetreatTile(enemy, from, closePick, tiles, out Vector2Int closeRetreatTile))
                    plan.SetMoveTile(closeRetreatTile);

                return;
            }

            // 이동 후 스킬을 사용할 수 있는 도달 가능 칸을 찾는다.
            List<EnemyMoveOption> skillTiles = BuildSkillTileOptions(enemy, from, targets, tiles);

            if (_moves.TrySkillTile(enemy, skillTiles, out EnemyMovePick move))
            {
                plan.SetTarget(move.Target);
                plan.SetMoveTile(move.Tile);
                return;
            }

            // 마지막으로 직선거리가 아닌 실제 경로 비용 기준으로 가장 가까운 대상을 쫓는다.
            Unit target = PickClosest(from, targets, routes);

            if (target == null)
                return;

            if (_skills.IsTooClose(enemy, from, target.gameObject))
            {
                plan.SetTarget(target);
                return;
            }

            plan.SetTarget(target);

            if (GridMap.Instance == null)
                return;

            Vector2Int targetPos = GridMap.Instance.WorldToGridPos(target.transform.position);

            if (_moves.TryApproachTile(from, target, targetPos, tiles, routes, out Vector2Int approachTile))
                plan.SetMoveTile(approachTile);
        }

        // 벽과 우회 경로를 반영하기 위해 직선거리보다 실제 경로 비용을 우선한다.
        private static Unit PickClosest(Vector2Int from, IReadOnlyList<Unit> targets, EnemyRouteMap routes)
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
                bool hasRoute = false;

                if (routes != null)
                    hasRoute = routes.TryGetCost(target, from, out cost);

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

        // 현재 이동 가능 칸 중 이동 후 스킬 사용이 가능한 후보를 만든다.
        private List<EnemyMoveOption> BuildSkillTileOptions(AbstractEnemyUnit enemy, Vector2Int from,
            IReadOnlyList<Unit> targets, IReadOnlyList<EnemyMoveTile> tiles)
        {
            var options = new List<EnemyMoveOption>();
            var gridMap = GridMap.Instance;

            if (enemy == null || targets == null || tiles == null || gridMap == null)
                return options;

            foreach (var target in targets)
            {
                if (target == null)
                    continue;

                Vector2Int targetPos = gridMap.WorldToGridPos(target.transform.position);

                foreach (var tile in tiles)
                {
                    if (tile.Pos == from)
                        continue;

                    if (!_skills.TrySkill(enemy, tile.Pos, target.gameObject, out EnemySkillPick pick))
                        continue;

                    options.Add(new EnemyMoveOption(
                        target,
                        tile.Pos,
                        pick.Score,
                        pick.Skill.PosScore(tile.Pos, target.gameObject),
                        tile.Cost,
                        DistanceUtils.GetManhattanDistance(tile.Pos, targetPos)));
                }
            }

            return options;
        }

        private static bool CanKeepSpace(AbstractEnemyUnit enemy)
            => enemy?.AIProfile == null || enemy.AIProfile.WantsSpace;
    }
}
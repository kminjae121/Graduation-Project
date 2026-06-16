using System.Collections.Generic;
using Code.Map;
using Code.SkillSystem;
using Code.Utils;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public sealed class BossPlanner : EnemyPlannerBase
    {
        private readonly BossPatternController _pattern;
        private readonly EnemyPlanner _fallbackPlanner = new();

        public BossPlanner(BossPatternController pattern)
        {
            _pattern = pattern;
        }

        public override void Build(EnemyPlan plan, AbstractEnemyUnit enemy, Vector2Int from,
            IReadOnlyList<Unit> targets, IReadOnlyList<EnemyMoveTile> tiles, EnemyRouteMap routes)
        {
            if (plan == null || enemy == null || _pattern == null || targets == null || targets.Count == 0)
                return;

            _pattern.PreparePlanStep();

            if (_pattern.SkipTurn)
            {
                plan.Clear();
                return;
            }

            if (_pattern.UseDefaultPlan)
            {
                _fallbackPlanner.Build(plan, enemy, from, targets, tiles, routes);
                return;
            }

            Unit target = PickClosestTarget(from, targets, routes);

            if (target == null)
                return;

            SkillSO skillSO = _pattern.PatternSkill;

            if (skillSO == null)
            {
                plan.SetTarget(target);
                return;
            }

            if (!TryGetEnemySkill(enemy, skillSO, out EnemySkill skill))
            {
                if (_pattern.FallbackToDefault)
                    _fallbackPlanner.Build(plan, enemy, from, targets, tiles, routes);
                else
                    plan.SetTarget(target);

                return;
            }

            if (skill.CanUseAt(from, target.gameObject))
            {
                plan.SetCombatDecision(target, skillSO);
                return;
            }

            if (_pattern.CanMoveNow &&
                TryFindSkillTile(skill, target, from, tiles, out Vector2Int moveTile))
            {
                plan.SetTarget(target);
                plan.SetMoveTile(moveTile);
                return;
            }

            if (_pattern.FallbackToDefault)
            {
                _fallbackPlanner.Build(plan, enemy, from, targets, tiles, routes);
                return;
            }

            plan.SetTarget(target);
        }

        private bool TryFindSkillTile(EnemySkill skill, Unit target, Vector2Int from,
            IReadOnlyList<EnemyMoveTile> tiles, out Vector2Int selectedTile)
        {
            selectedTile = default;

            if (skill == null || target == null || tiles == null || GridMap.Instance == null)
                return false;

            var bestOption = default(EnemyMoveOption);
            var found = false;
            Vector2Int targetPos = GridMap.Instance.WorldToGridPos(target.transform.position);

            foreach (var tile in tiles)
            {
                if (tile.Pos == from)
                    continue;

                if (!skill.CanUseAt(tile.Pos, target.gameObject))
                    continue;

                var option = new EnemyMoveOption(
                    target,
                    tile.Pos,
                    skill.ScoreAt(tile.Pos, target.gameObject, null),
                    skill.PosScore(tile.Pos, target.gameObject),
                    tile.Cost,
                    DistanceUtils.GetManhattanDistance(tile.Pos, targetPos));

                if (!found || IsBetterMoveOption(option, bestOption))
                {
                    bestOption = option;
                    found = true;
                }
            }

            selectedTile = bestOption.Tile;
            return found;
        }

        private static bool IsBetterMoveOption(EnemyMoveOption option, EnemyMoveOption best)
        {
            if (!best.IsValid)
                return true;

            if (!Mathf.Approximately(option.SkillScore, best.SkillScore))
                return option.SkillScore > best.SkillScore;

            if (!Mathf.Approximately(option.PosScore, best.PosScore))
                return option.PosScore > best.PosScore;

            if (option.Cost != best.Cost)
                return option.Cost < best.Cost;

            return option.Distance < best.Distance;
        }
    }
}
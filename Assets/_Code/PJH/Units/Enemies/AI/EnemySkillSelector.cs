using System.Collections.Generic;
using Code.Map;
using Code.SkillSystem;
using Code.Utils;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public sealed class EnemySkillSelector
    {
        public bool TryBest(AbstractEnemyUnit enemy, Vector2Int from, IReadOnlyList<Unit> targets, out EnemySkillPick bestPick)
        {
            bestPick = default;

            if (enemy == null || targets == null || GridMap.Instance == null)
                return false;

            var bestScore = float.MinValue;
            var bestDistance = float.MaxValue;

            foreach (var target in targets)
            {
                if (target == null)
                    continue;

                if (!TrySkill(enemy, from, target.gameObject, out EnemySkillPick pick))
                    continue;

                float distance = DistanceUtils.GetEuclideanDistance(from, GridMap.Instance.WorldToGridPos(target.transform.position));

                if (bestPick.IsValid && pick.Score < bestScore)
                    continue;

                if (bestPick.IsValid && Mathf.Approximately(pick.Score, bestScore))
                {
                    if (distance > bestDistance)
                        continue;

                    if (Mathf.Approximately(distance, bestDistance) && !IsBetterSkill(pick.SkillSO, bestPick.SkillSO))
                        continue;
                }

                bestPick = pick;
                bestScore = pick.Score;
                bestDistance = distance;
            }

            return bestPick.IsValid;
        }

        public bool TrySkill(AbstractEnemyUnit enemy, Vector2Int from, GameObject target, out EnemySkillPick pick)
        {
            pick = default;

            if (enemy == null || target == null || enemy.SkillCompo?.Skills == null || enemy.SkillCompo.Skills.Count == 0)
                return false;

            SkillSO bestSkillSO = null;
            EnemyBaseSkill bestSkill = null;
            var bestScore = float.MinValue;

            foreach (var (skillSO, skill) in enemy.SkillCompo.Skills)
            {
                if (skillSO == null || skill == null)
                    continue;

                if (skill is not EnemyBaseSkill enemySkill || !enemySkill.CanUseAt(from, target))
                    continue;

                float score = enemySkill.ScoreAt(from, target, enemy.AIProfile);

                if (Mathf.Approximately(score, float.MinValue))
                    continue;

                if (bestSkillSO != null && score < bestScore)
                    continue;

                if (bestSkillSO != null && Mathf.Approximately(score, bestScore) && !IsBetterSkill(skillSO, bestSkillSO))
                    continue;

                bestSkillSO = skillSO;
                bestSkill = enemySkill;
                bestScore = score;
            }

            if (bestSkillSO == null)
                return false;

            var targetUnit = target.GetComponent<Unit>();

            if (targetUnit == null)
                return false;

            pick = new EnemySkillPick(targetUnit, bestSkillSO, bestSkill, bestScore);
            return true;
        }

        public bool TryTooClose(AbstractEnemyUnit enemy, Vector2Int from, IReadOnlyList<Unit> targets, out EnemySkillPick pick)
        {
            pick = default;

            if (enemy == null || targets == null || enemy.SkillCompo?.Skills == null ||
                enemy.SkillCompo.Skills.Count == 0 || GridMap.Instance == null)
                return false;

            float bestScore = float.MinValue;
            float bestDistance = float.MaxValue;

            foreach (var target in targets)
            {
                if (target == null)
                    continue;

                foreach (var (skillSO, skill) in enemy.SkillCompo.Skills)
                {
                    if (skillSO == null || skill is not EnemyBaseSkill enemySkill)
                        continue;

                    if (!enemySkill.TooClose(from, target.gameObject))
                        continue;

                    float score = skillSO.SkillDamage + enemySkill.AIPriority * 10f +
                                  enemySkill.PosScore(from, target.gameObject);
                    float distance = DistanceUtils.GetEuclideanDistance(from,
                        GridMap.Instance.WorldToGridPos(target.transform.position));

                    if (pick.IsValid && score < bestScore)
                        continue;

                    if (pick.IsValid && Mathf.Approximately(score, bestScore))
                    {
                        if (distance > bestDistance)
                            continue;

                        if (Mathf.Approximately(distance, bestDistance) &&
                            !IsBetterSkill(skillSO, pick.SkillSO))
                            continue;
                    }

                    pick = new EnemySkillPick(target, skillSO, enemySkill, score);
                    bestScore = score;
                    bestDistance = distance;
                }
            }

            return pick.IsValid;
        }

        public bool TryCloseThreat(AbstractEnemyUnit enemy, Vector2Int from, IReadOnlyList<Unit> targets, out EnemySkillPick pick)
        {
            pick = default;

            if (enemy == null || targets == null || enemy.SkillCompo?.Skills == null ||
                enemy.SkillCompo.Skills.Count == 0 || GridMap.Instance == null)
                return false;

            var bestDistance = float.MaxValue;
            var bestScore = float.MinValue;

            foreach (var target in targets)
            {
                if (target == null)
                    continue;

                Vector2Int targetPos = GridMap.Instance.WorldToGridPos(target.transform.position);
                float distance = DistanceUtils.GetManhattanDistance(from, targetPos);

                foreach (var (skillSO, skill) in enemy.SkillCompo.Skills)
                {
                    if (skillSO == null || skill is not EnemyBaseSkill enemySkill)
                        continue;

                    if (!enemySkill.WantsMove(from, target.gameObject))
                        continue;

                    float score = skillSO.SkillDamage + enemySkill.AIPriority * 10f;

                    if (pick.IsValid && distance > bestDistance)
                        continue;

                    if (pick.IsValid && Mathf.Approximately(distance, bestDistance) && score <= bestScore)
                        continue;

                    pick = new EnemySkillPick(target, skillSO, enemySkill, score);
                    bestDistance = distance;
                    bestScore = score;
                }
            }

            return pick.IsValid;
        }

        public bool IsTooClose(AbstractEnemyUnit enemy, Vector2Int from, GameObject target)
        {
            if (enemy?.SkillCompo?.Skills == null || target == null)
                return false;

            foreach (var skill in enemy.SkillCompo.Skills.Values)
                if (skill is EnemyBaseSkill enemySkill && enemySkill.TooClose(from, target))
                    return true;

            return false;
        }

        private static bool IsBetterSkill(SkillSO candidate, SkillSO current)
        {
            if (candidate == null)
                return false;

            if (current == null)
                return true;

            if (candidate.SkillDamage != current.SkillDamage)
                return candidate.SkillDamage > current.SkillDamage;

            if (candidate.SkillCost != current.SkillCost)
                return candidate.SkillCost < current.SkillCost;

            return true;
        }
    }
}
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public static class EnemyMoveRanker
    {
        public static bool SkillTile(AbstractEnemyUnit enemy, EnemyMoveOption option, EnemyMoveOption best, bool found)
        {
            if (!found)
                return true;

            if (!Mathf.Approximately(option.SkillScore, best.SkillScore))
                return option.SkillScore > best.SkillScore;

            if (!Mathf.Approximately(option.PosScore, best.PosScore))
                return option.PosScore > best.PosScore;

            if (option.Cost != best.Cost)
                return option.Cost < best.Cost;

            return !WorseDist(enemy, option.Distance, best.Distance);
        }

        public static bool Space(EnemyMoveEval score, EnemyMoveEval best)
        {
            if (!score.IsValid)
                return false;

            if (!best.IsValid)
                return true;

            if (!Mathf.Approximately(score.Score, best.Score))
                return score.Score > best.Score;

            if (score.Cost != best.Cost)
                return score.Cost < best.Cost;

            return score.Dist > best.Dist;
        }

        public static bool Retreat(EnemyMoveEval score, EnemyMoveEval best)
        {
            if (!score.IsValid)
                return false;

            if (!best.IsValid)
                return true;

            if (!Mathf.Approximately(score.Score, best.Score))
                return score.Score > best.Score;

            return score.Cost < best.Cost;
        }

        public static bool Approach(EnemyMoveEval score, EnemyMoveEval best)
        {
            if (!score.IsValid)
                return false;

            if (!best.IsValid)
                return true;

            if (score.HasRoute != best.HasRoute)
                return score.HasRoute;

            if (score.HasRoute && score.Route != best.Route)
                return score.Route < best.Route;

            if (!score.HasRoute && !Mathf.Approximately(score.Dist, best.Dist))
                return score.Dist < best.Dist;

            return score.Cost < best.Cost;
        }

        private static bool WorseDist(AbstractEnemyUnit enemy, float candidate, float current)
        {
            if (enemy?.AIProfile != null && enemy.AIProfile.WantsSpace)
                return candidate <= current;

            return candidate >= current;
        }
    }
}
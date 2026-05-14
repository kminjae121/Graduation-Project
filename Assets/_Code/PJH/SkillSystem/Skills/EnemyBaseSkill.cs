using Code.Map;
using Code.UnitSystem.Enemies.AI;
using Code.Utils;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.SkillSystem
{
    public abstract class EnemyBaseSkill : BaseSkill
    {
        [Header("AI Positioning")]
        [SerializeField] private bool useRange;
        [SerializeField] private int prefRange = 1;
        [SerializeField] private int safeRange;

        [Header("AI Score")]
        [SerializeField] private int aiPriority;
        [SerializeField] private bool needSight;

        public int AIPriority => aiPriority;

        private void OnValidate()
        {
            safeRange = Mathf.Max(0, safeRange);
            prefRange = Mathf.Max(0, prefRange);

            if (safeRange < DeadRange())
                safeRange = DeadRange();

            if (prefRange < safeRange)
                prefRange = safeRange;
        }

        public virtual bool CanUse(GameObject target)
        {
            var gridMap = GridMap.Instance;

            if (gridMap == null)
                return false;

            return CanUseAt(gridMap.WorldToGridPos(GetCasterWorldPos()), target);
        }

        public virtual bool CanUseAt(Vector2Int from, GameObject target)
        {
            if (target == null || SkillSO == null)
                return false;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
                return false;

            return PassRange(from, gridMap.WorldToGridPos(target.transform.position));
        }

        public virtual float ScoreAt(Vector2Int from, GameObject target, EnemyAIProfileSO ai)
        {
            if (target == null || SkillSO == null || !CanUseAt(from, target))
                return float.MinValue;

            return MakeScore(SkillSO.SkillDamage, from, target, ai);
        }

        public virtual bool WantsMove(Vector2Int from, GameObject target)
        {
            if (!useRange || target == null)
                return false;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
                return false;

            float distance = RangeDistance(from, gridMap.WorldToGridPos(target.transform.position));

            return distance <= SafeRange();
        }

        public bool TooClose(Vector2Int from, GameObject target)
        {
            if (target == null)
                return false;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
                return false;

            float distance = RangeDistance(from, gridMap.WorldToGridPos(target.transform.position));

            return distance <= DeadRange();
        }

        public virtual float PosScore(Vector2Int from, GameObject target)
        {
            if (!useRange || target == null)
                return 0f;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
                return float.MinValue;

            float distance = RangeDistance(from, gridMap.WorldToGridPos(target.transform.position));
            float score = -Mathf.Abs(distance - PrefRange());

            if (distance <= SafeRange())
                score -= 1000f;

            return score;
        }

        protected Vector3 GetCasterWorldPos()
        {
            var ownerEnemy = GetComponentInParent<AbstractEnemyUnit>();
            return ownerEnemy != null ? ownerEnemy.transform.position : transform.position;
        }

        protected bool PassRange(Vector2Int from, Vector2Int to, bool useMax = true)
        {
            var gridMap = GridMap.Instance;

            if (gridMap == null)
                return false;

            float distance = RangeDistance(from, to);

            if (distance <= DeadRange())
                return false;

            if (useMax && distance > Mathf.Max(0f, SkillSO.SkillRange))
                return false;

            if (!needSight)
                return true;

            return DistanceUtils.HasLineOfSight(from, to, pos =>
            {
                var tile = gridMap.GetTile(pos);
                return tile != null && tile.HasState(TileState.Obstacle);
            });
        }

        protected float MakeScore(float power, Vector2Int from, GameObject target, EnemyAIProfileSO ai)
        {
            if (target == null)
                return float.MinValue;

            if (ai == null)
                return power + aiPriority * 10f + PosScore(from, target);

            float score = power * ai.DmgWeight;
            score += aiPriority * ai.PrioWeight;
            score += PosScore(from, target) * ai.PosWeight;

            if (ai.WantsSpace && WantsMove(from, target))
                score -= ai.ClosePenalty;

            return score;
        }

        private int PrefRange()
            => Mathf.Max(0, prefRange);

        private int SafeRange()
            => Mathf.Max(Mathf.Clamp(safeRange, 0, PrefRange()), DeadRange());

        private int DeadRange()
            => SkillSO == null ? 0 : Mathf.Max(0, SkillSO.MinRange);

        private static float RangeDistance(Vector2Int from, Vector2Int to)
            => DistanceUtils.GetManhattanDistance(from, to);
    }
}

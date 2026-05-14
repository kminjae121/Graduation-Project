using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public readonly struct EnemyMoveEval
    {
        public static readonly EnemyMoveEval Invalid = new(default, 0f, 0, 0f, 0, false, false);

        public readonly Vector2Int Pos;
        public readonly float Score;
        public readonly int Cost;
        public readonly float Dist;
        public readonly int Route;
        public readonly bool HasRoute;
        public readonly bool IsValid;

        private EnemyMoveEval(Vector2Int pos, float score, int cost, float dist, int route, bool hasRoute,
            bool isValid)
        {
            Pos = pos;
            Score = score;
            Cost = cost;
            Dist = dist;
            Route = route;
            HasRoute = hasRoute;
            IsValid = isValid;
        }

        public EnemyMoveEval(Vector2Int pos, float score, int cost, float dist)
            : this(pos, score, cost, dist, 0, false, true)
        {
        }

        public static EnemyMoveEval WithRoute(Vector2Int pos, int route, int cost, float dist)
            => new(pos, 0f, cost, dist, route, true, true);

        public static EnemyMoveEval WithDist(Vector2Int pos, int cost, float dist)
            => new(pos, 0f, cost, dist, 0, false, true);
    }
}

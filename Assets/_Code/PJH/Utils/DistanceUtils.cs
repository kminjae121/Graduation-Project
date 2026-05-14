using System;
using UnityEngine;

namespace Code.Utils
{
    public static class DistanceUtils
    {
        public static float GetEuclideanDistance(Vector2Int start, Vector2Int destination)
            => Vector2Int.Distance(start, destination);

        public static float GetEuclideanDistance(Vector3Int start, Vector3Int destination)
            => Vector3Int.Distance(start, destination);

        public static int GetManhattanDistance(Vector2Int start, Vector2Int destination)
            => Mathf.Abs(start.x - destination.x) + Mathf.Abs(start.y - destination.y);

        public static int GetChebyshevDistance(Vector2Int start, Vector2Int destination)
            => Mathf.Max(Mathf.Abs(start.x - destination.x), Mathf.Abs(start.y - destination.y));

        public static bool HasLineOfSight(Vector2Int start, Vector2Int destination, Func<Vector2Int, bool> blocksSight)
        {
            if (blocksSight == null)
                return true;

            Vector2Int delta = destination - start;
            int steps = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));

            if (steps <= 1)
                return true;

            Vector2 origin = new(start.x, start.y);
            Vector2 step = new Vector2(delta.x, delta.y) / steps;

            for (int i = 1; i < steps; ++i)
            {
                Vector2 point = origin + step * i;
                Vector2Int gridPos = new(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));

                if (gridPos == start || gridPos == destination)
                    continue;

                if (blocksSight(gridPos))
                    return false;
            }

            return true;
        }
    }
}

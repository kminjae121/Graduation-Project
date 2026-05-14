using System;
using System.Collections.Generic;
using Code.Map;
using Code.Navigation;
using Code.Utils;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public sealed class EnemyRouteMap
    {
        private readonly Queue<Vector2Int> _open = new();
        private readonly Dictionary<Vector2Int, int> _costs = new();
        private readonly Dictionary<Unit, Dictionary<Vector2Int, int>> _routes = new();

        public void Build(IReadOnlyList<Unit> targets, PathBaker baker, Func<Vector2Int, bool> canEnter, ISet<Vector2Int> watch = null)
        {
            _routes.Clear();

            if (targets == null || GridMap.Instance == null)
                return;

            foreach (var target in targets)
            {
                if (target == null)
                    continue;

                Vector2Int goal = GridMap.Instance.WorldToGridPos(target.transform.position);
                _routes[target] = BuildRoute(goal, baker, canEnter, watch);
            }
        }

        public bool TryGetCost(Unit target, Vector2Int pos, out int cost)
        {
            cost = 0;

            if (target == null)
                return false;

            if (!_routes.TryGetValue(target, out var route))
                return false;

            return route.TryGetValue(pos, out cost);
        }

        private Dictionary<Vector2Int, int> BuildRoute(Vector2Int goal, PathBaker baker, Func<Vector2Int, bool> canEnter, ISet<Vector2Int> watch)
        {
            _open.Clear();
            _costs.Clear();

            if (!CanUseBake(goal, baker))
                return new Dictionary<Vector2Int, int>();

            int left = HasWatch(watch) ? watch.Count : 0;
            Add(goal, 0, watch, ref left);

            if (!IsDone(watch, left))
                BuildBaked(baker, canEnter, watch, ref left);

            return new Dictionary<Vector2Int, int>(_costs);
        }

        private void BuildBaked(PathBaker baker, Func<Vector2Int, bool> canEnter, ISet<Vector2Int> watch, ref int left)
        {
            while (_open.Count > 0 && !IsDone(watch, left))
            {
                Vector2Int pos = _open.Dequeue();
                int cost = _costs[pos];

                if (!baker.bakedData.GetNodeIfExist(GridCoordUtils.GridToCell(pos), out NodeData node))
                    continue;

                foreach (var neighbor in node.neighbors)
                {
                    Vector2Int next = GridCoordUtils.CellToGrid(neighbor.endCellPos);

                    if (!IsCardinal(pos, next))
                        continue;

                    TryAdd(next, cost + 1, canEnter, watch, ref left);

                    if (IsDone(watch, left))
                        break;
                }
            }
        }

        private void TryAdd(Vector2Int pos, int cost, Func<Vector2Int, bool> canEnter, ISet<Vector2Int> watch, ref int left)
        {
            if (_costs.ContainsKey(pos))
                return;

            if (GridMap.Instance != null && !GridMap.Instance.IsValidPosition(pos))
                return;

            if (canEnter != null && !canEnter(pos))
                return;

            Add(pos, cost, watch, ref left);
        }

        private void Add(Vector2Int pos, int cost, ISet<Vector2Int> watch, ref int left)
        {
            _costs.Add(pos, cost);
            _open.Enqueue(pos);

            if (HasWatch(watch) && watch.Contains(pos))
                --left;
        }

        private static bool HasWatch(ISet<Vector2Int> watch)
            => watch is { Count: > 0 };

        private static bool IsDone(ISet<Vector2Int> watch, int left)
            => HasWatch(watch) && left <= 0;

        private static bool CanUseBake(Vector2Int goal, PathBaker baker)
        {
            return baker?.bakedData != null &&
                   baker.bakedData.GetNodeIfExist(GridCoordUtils.GridToCell(goal), out _);
        }

        // 대각선 방지
        private static bool IsCardinal(Vector2Int from, Vector2Int to)
        {
            Vector2Int delta = to - from;
            return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) == 1;
        }
    }
}
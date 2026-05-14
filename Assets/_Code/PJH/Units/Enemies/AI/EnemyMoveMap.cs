using System;
using System.Collections.Generic;
using Code.Map;
using Code.Navigation;
using Code.Utils;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public sealed class EnemyMoveMap
    {
        private readonly Queue<Vector2Int> _open = new();
        private readonly Dictionary<Vector2Int, int> _costs = new();
        private readonly List<EnemyMoveTile> _tiles = new();

        public List<EnemyMoveTile> Build(Vector2Int start, int range, PathBaker baker, Func<Vector2Int, bool> canEnter)
        {
            _open.Clear();
            _costs.Clear();
            _tiles.Clear();

            if (range < 0)
                return _tiles;

            Add(start, 0);

            if (range == 0 || !CanUseBake(start, baker))
                return _tiles;

            BuildBaked(range, baker, canEnter);
            return _tiles;
        }

        private void BuildBaked(int range, PathBaker baker, Func<Vector2Int, bool> canEnter)
        {
            while (_open.Count > 0)
            {
                Vector2Int pos = _open.Dequeue();
                int cost = _costs[pos];

                if (cost >= range)
                    continue;

                if (!baker.bakedData.GetNodeIfExist(GridCoordUtils.GridToCell(pos), out NodeData node))
                    continue;

                foreach (var neighbor in node.neighbors)
                {
                    Vector2Int next = GridCoordUtils.CellToGrid(neighbor.endCellPos);

                    if (!IsCardinal(pos, next))
                        continue;

                    TryAdd(next, cost + 1, range, canEnter);
                }
            }
        }

        private void TryAdd(Vector2Int pos, int cost, int range, Func<Vector2Int, bool> canEnter)
        {
            if (cost > range || _costs.ContainsKey(pos))
                return;

            if (GridMap.Instance != null && !GridMap.Instance.IsValidPosition(pos))
                return;

            if (canEnter != null && !canEnter(pos))
                return;

            Add(pos, cost);
        }

        private void Add(Vector2Int pos, int cost)
        {
            _costs.Add(pos, cost);
            _open.Enqueue(pos);
            _tiles.Add(new EnemyMoveTile(pos, cost));
        }

        private static bool CanUseBake(Vector2Int start, PathBaker baker)
        {
            return baker?.bakedData != null &&
                   baker.bakedData.GetNodeIfExist(GridCoordUtils.GridToCell(start), out _);
        }

        // 대각선 방지
        private static bool IsCardinal(Vector2Int from, Vector2Int to)
        {
            Vector2Int delta = to - from;
            return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) == 1;
        }
    }
}
using System;
using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.Core.Interfaces;
using Code.Map;
using UnityEngine;

namespace Code.UnitSystem
{
    public class RangeComponent : MonoBehaviour, IUnitComponent
    {
        public bool IsActive { get; set; }
        public bool isMove;

        protected Action _resetTileEvent;
        protected Unit _owner;

        public readonly List<IMapTile> TilesInRange = new();

        private UnitManageRangeCompo _rangeComponent;

        public void Initialize(Unit owner)
        {
            _owner = owner;
            _rangeComponent = owner.GetUnitCompo<UnitManageRangeCompo>();
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        public void FindObjectInRange(int range)
        {
            _rangeComponent.RemoveAllRange(); 

            CalculateRange(range);
            ProcessTiles(TilesInRange, true);
            IsActive = true;
        }

        protected virtual void CalculateRange(int range)
        {
            TilesInRange.Clear();
            
            Vector2Int start = GetRangeStartGridPos();

            Queue<(Vector2Int pos, int dist)> queue = new();
            HashSet<Vector2Int> visited = new();

            queue.Enqueue((start, 0));
            visited.Add(start);

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            while (queue.Count > 0)
            {
                var (pos, dist) = queue.Dequeue();

                if (dist >= range)
                    continue;

                foreach (var dir in dirs)
                {
                    Vector2Int next = pos + dir;

                    if (visited.Contains(next))
                        continue;

                    IMapTile tile = GridMap.Instance.GetTile(next);

                    if (tile == null)
                        continue;

                    visited.Add(next);
                    
                    if (tile.HasState(TileState.Obstacle))
                    {
                        TilesInRange.Add(tile);

                        continue;
                    }
                    
                    TilesInRange.Add(tile);
                    queue.Enqueue((next, dist + 1));
                }
            }
        }

        private Vector2Int GetRangeStartGridPos()
        {
            if (_owner != null)
            {
                UnitMoveCompo moveCompo = _owner.GetUnitCompo<UnitMoveCompo>();

                if (moveCompo?.CurrentMapTile != null)
                    return moveCompo.CurrentMapTile.GridPos;

                return GridMap.Instance.WorldToGridPos(_owner.transform.position);
            }

            return GridMap.Instance.WorldToGridPos(transform.position);
        }

        public void ResetTile()
        {
            if (TilesInRange.Count == 0)
                return;

            ProcessTiles(TilesInRange, false);

            IsActive = false;

            _resetTileEvent?.Invoke();
        }

        public void ReCheckInRange()
        {
            foreach (var tile in TilesInRange)
            {
                if (isMove && !tile.HasState(TileState.Obstacle) && !tile.HasState(TileState.Enemy))
                    tile.SetState(TileState.Walkable, true);

                ApplyOverlay(tile);
            }

            IsActive = true;
        }

        public void EndAct()
        {
            IsActive = false;
        }

        private void ProcessTiles(List<IMapTile> tiles, bool enable)
        {
            foreach (var tile in tiles)
            {
                if (isMove && !tile.HasState(TileState.Obstacle) && !tile.HasState(TileState.Enemy))
                    tile.SetState(TileState.Walkable, enable);

                if (enable)
                    ApplyOverlay(tile);
                else
                    tile.ClearOverlay();
            }
        }

        private void ApplyOverlay(IMapTile tile)
        {
            tile.SetDecalActive(true);

            if (tile.HasState(TileState.Obstacle))
                tile.SetOverlay(TileOverlayType.Blocked);
            else if (isMove)
                tile.SetOverlay(TileOverlayType.Move);
            else
                tile.SetOverlay(TileOverlayType.Attack);
        }
    }
}

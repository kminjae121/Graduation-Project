using System.Collections.Generic;
using Code.Core.Debugs;
using Code.Core.Interfaces;
using Code.Map;
using UnityEngine;

namespace Code.UnitSystem.Enemies
{
    public class BossGimmickSpawner : MonoBehaviour
    {
        [SerializeField] private BossPatternController boss;
        [SerializeField] private BossGimmickObject gimmickPrefab;
        [SerializeField] private Transform spawnParent;
        [SerializeField] private Vector3 spawnOffset;
        [SerializeField] private bool blockTile = true;

        private readonly List<IMapTile> _candidates = new();

        private BossGimmickObject _currentGimmick;
        private IMapTile _currentTile;

        private bool _tileWasWalkable;
        private bool _tileWasObstacle;
        private bool _tileWasEnemy;

        private void Awake()
        {
            if (boss == null)
                boss = GetComponentInParent<BossPatternController>();
        }

        private void OnDisable()
        {
            ClearCurrent();
        }

        public void Spawn()
        {
            ClearCurrent();

            if (gimmickPrefab == null)
            {
                UnityLogger.LogError($"[{nameof(BossGimmickSpawner)}] Gimmick prefab is missing.");
                return;
            }

            if (!TryPickTile(out IMapTile tile))
                return;

            ReserveTile(tile);

            Transform parent = spawnParent != null ? spawnParent : transform;
            _currentGimmick = Instantiate(gimmickPrefab, tile.WorldPos + spawnOffset,
                Quaternion.identity, parent);
            _currentGimmick.Initialize(this);
        }

        public void ClearCurrent()
        {
            BossGimmickObject gimmick = _currentGimmick;
            _currentGimmick = null;

            ReleaseTile();

            if (gimmick != null)
                gimmick.ClearWithoutComplete();
        }

        public void CompleteGimmick(BossGimmickObject gimmick)
        {
            if (gimmick != _currentGimmick)
                return;

            _currentGimmick = null;
            ReleaseTile();
            boss?.CompleteGimmick(true);
        }

        internal void ReleaseGimmick(BossGimmickObject gimmick)
        {
            if (gimmick != _currentGimmick)
                return;

            _currentGimmick = null;
            ReleaseTile();
        }

        private bool TryPickTile(out IMapTile selectedTile)
        {
            selectedTile = null;

            GridMap gridMap = GridMap.Instance;

            if (gridMap == null)
            {
                UnityLogger.LogError($"[{nameof(BossGimmickSpawner)}] GridMap is missing.");
                return false;
            }

            _candidates.Clear();

            for (int x = 0; x < gridMap.Width; ++x)
                for (int y = 0; y < gridMap.Height; ++y)
                {
                    IMapTile tile = gridMap.GetTile(x, y);

                    if (IsSpawnableTile(gridMap, tile))
                        _candidates.Add(tile);
                }

            if (_candidates.Count == 0)
            {
                UnityLogger.LogWarning($"[{nameof(BossGimmickSpawner)}] No spawnable tile found.");
                return false;
            }

            selectedTile = _candidates[Random.Range(0, _candidates.Count)];
            return true;
        }

        private static bool IsSpawnableTile(GridMap gridMap, IMapTile tile)
        {
            if (tile == null || tile.GetTileUnit() != null)
                return false;

            return gridMap.CanMoveTo(tile.GridPos);
        }

        private void ReserveTile(IMapTile tile)
        {
            _currentTile = tile;
            _tileWasWalkable = tile.HasState(TileState.Walkable);
            _tileWasObstacle = tile.HasState(TileState.Obstacle);
            _tileWasEnemy = tile.HasState(TileState.Enemy);

            if (!blockTile)
                return;

            tile.SetState(TileState.Walkable, false);
            tile.SetState(TileState.Obstacle, true);
        }

        private void ReleaseTile()
        {
            if (_currentTile == null)
                return;

            if (blockTile)
            {
                _currentTile.SetState(TileState.Walkable, _tileWasWalkable);
                _currentTile.SetState(TileState.Obstacle, _tileWasObstacle);
                _currentTile.SetState(TileState.Enemy, _tileWasEnemy);
            }

            _currentTile = null;
        }
    }
}

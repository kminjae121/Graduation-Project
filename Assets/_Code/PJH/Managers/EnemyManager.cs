using System.Collections.Generic;
using System.Linq;
using Code.Core.Debugs;
using Code.Map;
using Code.Navigation;
using Code.UnitSystem;
using Code.UnitSystem.Enemies;
using Code.UnitSystem.Enemies.AI;
using GondrLib.Dependencies;
using UnityEngine;

namespace Code.Managers
{
    [Provide]
    public class EnemyManager : MonoBehaviour, IDependencyProvider
    {
        [SerializeField] private UnitManager unitManager;
        [Inject] private PathBaker _pathBaker;

        private readonly Dictionary<AbstractEnemyUnit, EnemyPlan> _plans = new();
        private readonly Dictionary<Vector2Int, AbstractEnemyUnit> _reservedTiles = new();
        private readonly EnemyPlanner _planner = new();
        private readonly EnemyMoveMap _moveMap = new();
        private readonly EnemyRouteMap _routeMap = new();
        private readonly HashSet<Vector2Int> _routeWatch = new();

        public void RefreshPlan(AbstractEnemyUnit enemy)
        {
            if (enemy == null)
            {
                UnityLogger.LogError("enemy is null.");
                return;
            }

            EnemyPlan plan = GetOrCreatePlan(enemy);
            plan.Clear();

            var gridMap = GridMap.Instance;

            if (unitManager == null || gridMap == null)
            {
                UnityLogger.LogError($"unitManager : {unitManager}, gridMap : {gridMap}.");
                return;
            }

            Vector2Int currentPos = gridMap.WorldToGridPos(enemy.transform.position);
            List<Unit> targets = GetTargets();
            List<EnemyMoveTile> tiles = GetTiles(enemy, currentPos);
            _routeMap.Build(targets, _pathBaker, tile => CanMoveTo(enemy, currentPos, tile),
                GetRouteWatch(currentPos, tiles));

            _planner.Build(plan, enemy, currentPos, targets, tiles, _routeMap);
        }

        public bool TryGetPlan(AbstractEnemyUnit enemy, out EnemyPlan plan)
        {
            if (enemy == null)
            {
                plan = null;
                return false;
            }

            return _plans.TryGetValue(enemy, out plan);
        }

        public bool TryReserveTile(AbstractEnemyUnit enemy, Vector2Int tilePos)
        {
            if (enemy == null)
                return false;

            if (_reservedTiles.TryGetValue(tilePos, out var reservedEnemy) && reservedEnemy != enemy)
                return false;

            ReleaseReservation(enemy);
            _reservedTiles[tilePos] = enemy;
            return true;
        }

        public void ReleaseReservation(AbstractEnemyUnit enemy)
        {
            if (enemy == null)
                return;

            Vector2Int releaseKey = default;
            bool found = false;

            foreach (var pair in _reservedTiles)
            {
                if (pair.Value != enemy)
                    continue;

                releaseKey = pair.Key;
                found = true;
                break;
            }

            if (found)
                _reservedTiles.Remove(releaseKey);
        }

        public void RemovePlan(AbstractEnemyUnit enemy)
        {
            if (enemy == null)
                return;

            ReleaseReservation(enemy);
            _plans.Remove(enemy);
        }

        public void ClearTurnReservations()
            => _reservedTiles.Clear();

        private EnemyPlan GetOrCreatePlan(AbstractEnemyUnit enemy)
        {
            if (_plans.TryGetValue(enemy, out var plan))
                return plan;

            plan = new EnemyPlan();
            _plans.Add(enemy, plan);
            return plan;
        }

        private List<Unit> GetTargets()
        {
            if (unitManager == null)
            {
                UnityLogger.LogError("unitManager is null.");
                return new List<Unit>();
            }

            return unitManager.GetPlayerUnits()
                .Where(unit => unit != null && unit.gameObject.activeInHierarchy)
                .ToList();
        }

        private List<EnemyMoveTile> GetTiles(AbstractEnemyUnit enemy, Vector2Int currentPos)
            => _moveMap.Build(currentPos, GetMoveRange(enemy), _pathBaker,
                tile => CanMoveTo(enemy, currentPos, tile));

        private ISet<Vector2Int> GetRouteWatch(Vector2Int currentPos, IReadOnlyList<EnemyMoveTile> tiles)
        {
            _routeWatch.Clear();
            _routeWatch.Add(currentPos);

            if (tiles == null)
                return _routeWatch;

            foreach (var tile in tiles)
                _routeWatch.Add(tile.Pos);

            return _routeWatch;
        }

        private static int GetMoveRange(AbstractEnemyUnit enemy)
            => enemy?.unitSO == null ? 0 : Mathf.Max(0, enemy.unitSO.MoveRange);

        private bool CanMoveTo(AbstractEnemyUnit enemy, Vector2Int currentPos, Vector2Int tile)
        {
            if (tile == currentPos)
                return true;
            
            if (!GridMap.Instance.CanMoveTo(tile))
                return false;
            
            // 예약된 칸이면 false, 그게 나면 상관 X
            return !_reservedTiles.TryGetValue(tile, out var reservedEnemy) || reservedEnemy == enemy;
        }
    }
}

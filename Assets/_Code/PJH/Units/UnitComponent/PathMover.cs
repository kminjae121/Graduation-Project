using System;
using System.Threading.Tasks;
using Code.Core.Debugs;
using Code.Core.Interfaces;
using Code.Map;
using Code.Navigation;
using Code.Utils;
using UnityEngine;

namespace Code.UnitSystem.UnitComponent
{
    public class PathMover : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private int maxPathCount = 50;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private Vector3[] pointArray;

        public event Action OnMoveEnd;

        private PathAgent _pathAgent;
        private Unit _owner;
        private GridMap _gridMap;
        private UnitRotator _rotatorCompo;
        private int _pathLength;

        public void Initialize(Unit owner)
        {
            _owner = owner;
            _pathAgent = owner.GetComponent<PathAgent>();
            _gridMap = GridMap.Instance;
            _rotatorCompo = owner.GetUnitCompo<UnitRotator>();
            pointArray = new Vector3[maxPathCount];
        }

        public void SetPathAndMove(Vector2Int startPos, Vector2Int destination, bool allowPartialPath = false)
        {
            SetPathAndMove(GridCoordUtils.GridToCell(startPos), GridCoordUtils.GridToCell(destination), allowPartialPath);
        }

        private async void SetPathAndMove(Vector3Int startPos, Vector3Int destination, bool allowPartialPath)
        {
            try
            {
                if (_pathAgent == null)
                {
                    UnityLogger.LogError("PathAgent is missing.");
                    OnMoveEnd?.Invoke();
                    return;
                }

                _gridMap ??= GridMap.Instance;

                if (_gridMap == null)
                {
                    UnityLogger.LogError("GridMap is missing.");
                    OnMoveEnd?.Invoke();
                    return;
                }

                _rotatorCompo ??= _owner.GetUnitCompo<UnitRotator>();

                //UnityLogger.Log($"Start : {startPos}, Destination : {destination}");
                _pathLength = await _pathAgent.GetPath(startPos, destination, pointArray, allowPartialPath);

                if (_pathLength <= 0)
                {
                    UnityLogger.Log("pathLength is zero");
                    OnMoveEnd?.Invoke();
                    return;
                }

                int remainingMovePoint = GetMoveRange();
                Vector3Int previousCell = startPos;
                Vector3Int finalCell = startPos;

                for (int i = 1; i < _pathLength; ++i)
                {
                    Vector3Int nextCell = GridCoordUtils.GridToCell(_gridMap.WorldToGridPos(pointArray[i]));
                    Vector3Int reachableCell = GetReachableCell(previousCell, nextCell, remainingMovePoint, out int movedCost);

                    if (movedCost <= 0)
                        break;

                    Vector3 targetPoint = GetWorldPosition(reachableCell);
                    RotateToPoint(targetPoint);
                    await MoveToPoint(targetPoint);

                    remainingMovePoint -= movedCost;
                    previousCell = reachableCell;
                    finalCell = reachableCell;

                    if (reachableCell != nextCell || remainingMovePoint <= 0)
                        break;
                }

                UpdateUnitTileState(startPos, finalCell);
                OnMoveEnd?.Invoke();
            }
            catch (Exception e)
            {
                UnityLogger.LogError(e.Message);
                OnMoveEnd?.Invoke();
            }
        }

        private async Task MoveToPoint(Vector3 point)
        {
            while (Vector3.Distance(_owner.transform.position, point) > 0.1f)
            {
                _owner.transform.position =
                    Vector3.MoveTowards(_owner.transform.position, point, moveSpeed * Time.deltaTime);
                await Awaitable.NextFrameAsync();
            }
        }

        private void RotateToPoint(Vector3 point)
        {
            if (_rotatorCompo == null)
                return;

            _rotatorCompo.SetDir(point);
        }

        private Vector3Int GetReachableCell(Vector3Int startCell, Vector3Int endCell, int remainingMovePoint, out int movedCost)
        {
            movedCost = 0;
            Vector3Int delta = endCell - startCell;
            int segmentCost = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));

            if (segmentCost <= 0 || remainingMovePoint <= 0)
                return startCell;

            Vector3Int dir = new Vector3Int
            (
                Math.Sign(delta.x),
                Math.Sign(delta.y),
                Math.Sign(delta.z)
            );

            Vector3Int currentCell = startCell;
            int maxStep = Mathf.Min(segmentCost, remainingMovePoint);

            for (int step = 0; step < maxStep; ++step)
            {
                Vector3Int nextCell = currentCell + dir;

                if (!CanTraverseCell(nextCell))
                    break;

                currentCell = nextCell;
                ++movedCost;
            }

            return currentCell;
        }

        private Vector3 GetWorldPosition(Vector3Int cellPosition)
        {
            Vector3 worldPosition = _gridMap.GridToWorldPos(cellPosition.x, cellPosition.y);
            return ToMovePoint(worldPosition);
        }

        private Vector3 ToMovePoint(Vector3 worldPosition)
            => new(worldPosition.x, _owner.transform.position.y, worldPosition.z);

        private int GetMoveRange()
        {
            if (_owner?.unitSO == null)
                return 0;

            return Mathf.Max(0, _owner.unitSO.MoveRange);
        }

        private bool CanTraverseCell(Vector3Int cellPosition)
        {
            if (_gridMap == null)
                return false;

            return _gridMap.CanMoveTo(new Vector2Int(cellPosition.x, cellPosition.y));
        }

        private void UpdateUnitTileState(Vector3Int previousCell, Vector3Int currentCell)
        {
            if (_owner == null || _gridMap == null)
                return;

            IMapTile previousTile = _gridMap.GetTile(previousCell.x, previousCell.y);
            IMapTile currentTile = _gridMap.GetTile(currentCell.x, currentCell.y);

            if (previousTile != null)
            {
                previousTile.SetState(TileState.Enemy, false);
                previousTile.SetState(TileState.Obstacle, false);
                previousTile.SetState(TileState.Walkable, true);
            }

            if (currentTile != null)
            {
                currentTile.SetState(TileState.Walkable, false);
                currentTile.SetState(TileState.Obstacle, true);
                currentTile.SetState(TileState.Enemy, !_owner.IsPlayerUnit);
            }
        }
    }
}

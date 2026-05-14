using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Code.Map;
using Code.Core.Debugs;
using Code.UnitSystem;
using Code.Utils;
using GondrLib.Dependencies;
using UnityEngine;

namespace Code.Navigation
{
    public class PathAgent : MonoBehaviour
    {
        [Inject] private PathBaker _pathBaker;

        private CancellationTokenSource _cts = new();
        private bool _isCalculating;

        private void Awake()
        {
            Injector.InjectInto(this);
        }

        public async Task<int> GetPath(Vector3Int startPos, Vector3Int destination, Vector3[] pointArr, bool allowPartialPath = false)
        {
            if (_isCalculating && _cts != null)
                _cts.Cancel();

            if (_cts is { IsCancellationRequested: true })
                _cts = new CancellationTokenSource();

            try
            {
                _isCalculating = true;
                HashSet<Vector3Int> blockedCells = CollectBlockedCells(startPos, destination);

                (List<AstarNode> list, bool isSuccess) =
                    await Task.Run(() => CalculatePath(startPos, destination, blockedCells, allowPartialPath), _cts.Token);

                _isCalculating = false;

                int cornerIndex = 0;

                if (!isSuccess)
                {
                    UnityLogger.Log("Calculation Failed");
                    return cornerIndex;
                }

                pointArr[cornerIndex] = list[0].worldPos;
                ++cornerIndex;

                for (int i = 1; i < list.Count - 1; ++i)
                {
                    if (cornerIndex >= pointArr.Length)
                        break;

                    pointArr[cornerIndex] = list[i].worldPos;
                    ++cornerIndex;
                }

                pointArr[cornerIndex] = list[^1].worldPos;
                ++cornerIndex;

                return cornerIndex;
            }
            catch (OperationCanceledException)
            {
                return -1;
            }
            catch (Exception ex)
            {
                UnityLogger.Log(ex.Message);
                return -1;
            }
            finally
            {
                _isCalculating = false;
            }
        }

        private (List<AstarNode>, bool) CalculatePath(Vector3Int startPoint, Vector3Int destination, HashSet<Vector3Int> blockedCells, bool allowPartialPath)
        {
            //UnityLogger.Log("Calculate 진입");
            
            PriorityQueue<AstarNode> openList = new();
            HashSet<Vector3Int> closedSet = new();
            Dictionary<Vector3Int, float> bestGByCell = new();
            List<AstarNode> path = new();
            
            bool result = false;
            AstarNode goalNode = null;
            AstarNode bestReachableNode = null;
            float bestReachableDistance = float.MaxValue;
            float bestReachableCost = float.MaxValue;

            bool startSuccess = _pathBaker.bakedData.GetNodeIfExist(startPoint, out var startNode);
            bool endSuccess = _pathBaker.bakedData.GetNodeIfExist(destination, out var endNode);
            Vector3Int destinationCell = endSuccess ? endNode.cellPos : destination;
            //UnityLogger.Log($"st : {startPoint}, {startSuccess}, ed : {destination}, {endSuccess}");
            
            if (!startSuccess || (!endSuccess && !allowPartialPath))
                return (path, false);

            var startAstarNode = new AstarNode
            {
                nodeData = startNode,
                cellPos = startNode.cellPos,
                worldPos = startNode.worldPos,
                parentNode = null,
                g = 0,
                f = CalculateH(startNode.cellPos, destinationCell)
            };

            openList.Push(startAstarNode);
            bestGByCell[startAstarNode.cellPos] = startAstarNode.g;
            UpdateBestReachableNode(startAstarNode, destination, ref bestReachableNode, ref bestReachableDistance, ref bestReachableCost);
            
            while (openList.Count > 0)
            {
                if (_cts.Token.IsCancellationRequested)
                    throw new OperationCanceledException(_cts.Token);

                AstarNode currentNode = openList.Pop();

                if (closedSet.Contains(currentNode.cellPos))
                    continue;

                if (bestGByCell.TryGetValue(currentNode.cellPos, out float bestKnownG)
                    && currentNode.g > bestKnownG)
                    continue;

                closedSet.Add(currentNode.cellPos);
                UpdateBestReachableNode(currentNode, destination, ref bestReachableNode, ref bestReachableDistance, ref bestReachableCost);

                if (endSuccess && currentNode.nodeData == endNode)
                {
                    result = true;
                    goalNode = currentNode;
                    break;
                }

                foreach (var link in currentNode.nodeData.neighbors)
                {
                    if (!IsCardinal(currentNode.cellPos, link.endCellPos))
                        continue;

                    if (closedSet.Contains(link.endCellPos))
                        continue;

                    if (blockedCells != null && blockedCells.Contains(link.endCellPos))
                        continue;

                    if (!_pathBaker.bakedData.GetNodeIfExist(link.endCellPos, out NodeData nextNode))
                        continue;

                    float newG = currentNode.g + link.cost;

                    if (bestGByCell.TryGetValue(nextNode.cellPos, out float oldG) && newG >= oldG)
                        continue;

                    bestGByCell[nextNode.cellPos] = newG;

                    openList.Push(new AstarNode
                    {
                        nodeData = nextNode,
                        cellPos = nextNode.cellPos,
                        worldPos = nextNode.worldPos,
                        parentNode = currentNode,
                        g = newG,
                        f = newG + CalculateH(nextNode.cellPos, destinationCell)
                    });
                }
            }

            if (result)
            {
                AstarNode last = goalNode;

                while (last.parentNode != null)
                {
                    path.Add(last);
                    last = last.parentNode;
                }

                path.Add(last); // 시작점
                path.Reverse();
            }
            else if (allowPartialPath && bestReachableNode != null && bestReachableNode.parentNode != null)
            {
                AstarNode last = bestReachableNode;

                while (last.parentNode != null)
                {
                    path.Add(last);
                    last = last.parentNode;
                }

                path.Add(last);
                path.Reverse();
                result = path.Count > 1;
            }
             
            return (path, result);
        }

        private HashSet<Vector3Int> CollectBlockedCells(Vector3Int startPos, Vector3Int destination)
        {
            var blockedCells = new HashSet<Vector3Int>();
            GridMap gridMap = GridMap.Instance;

            if (gridMap == null)
                return blockedCells;

            for (int y = 0; y < gridMap.Height; ++y)
            {
                for (int x = 0; x < gridMap.Width; ++x)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);

                    if (cellPos == startPos || cellPos == destination)
                        continue;

                    var tile = gridMap.GetTile(x, y);

                    if (tile == null)
                        continue;

                    if (tile.HasAnyState(TileState.Enemy | TileState.Obstacle))
                        blockedCells.Add(cellPos);
                }
            }

            return blockedCells;
        }

        private static float CalculateH(Vector3Int startPoint, Vector3Int destination)
        {
            return Mathf.Abs(startPoint.x - destination.x) +
                   Mathf.Abs(startPoint.y - destination.y) +
                   Mathf.Abs(startPoint.z - destination.z);
        }

        private static void UpdateBestReachableNode(
            AstarNode candidate,
            Vector3Int destination,
            ref AstarNode bestNode,
            ref float bestDistance,
            ref float bestCost)
        {
            if (candidate == null)
                return;

            float candidateDistance = CalculateH(candidate.cellPos, destination);

            if (bestNode != null && candidateDistance > bestDistance)
                return;

            if (bestNode != null && Mathf.Approximately(candidateDistance, bestDistance) && candidate.g >= bestCost)
                return;

            bestNode = candidate;
            bestDistance = candidateDistance;
            bestCost = candidate.g;
        }

        private static bool IsCardinal(Vector3Int from, Vector3Int to)
        {
            Vector3Int delta = to - from;
            return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) + Mathf.Abs(delta.z) == 1;
        }
    }
}

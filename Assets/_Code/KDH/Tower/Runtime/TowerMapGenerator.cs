using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Tower
{
    public static class TowerMapGenerator
    {
        private const int LaneCount = 7;
        private const int PathCount = 6;
        private const int NormalStageRows = 7;
        private const int BossStageRows = 6;
        private const int MinimumDistinctStartLanes = 2;
        private const int EliteMinimumRow = 3;

        private const float EventWeight = 0.22f;
        private const float EliteWeight = 0.10f;
        private const float RewardWeight = 0.12f;

        private readonly struct PathEdge
        {
            public readonly Vector2Int From;
            public readonly Vector2Int To;

            public PathEdge(Vector2Int from, Vector2Int to)
            {
                From = from;
                To = to;
            }
        }

        public static TowerFloorMap Generate(TowerFloorKey floorKey, int seed = 0)
        {
            int actualSeed = seed != 0 ? MixSeed(seed, floorKey) : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            System.Random rng = new(actualSeed);

            int generatedRows = floorKey.IsBossStage ? BossStageRows : NormalStageRows;
            Dictionary<Vector2Int, TowerRoomNode> nodesByCell = new();
            HashSet<Vector2Int> activeCells = new();
            List<PathEdge> pathEdges = new();

            GenerateSlayStylePaths(generatedRows, rng, activeCells, pathEdges);

            TowerFloorMap map = new(floorKey);
            TowerRoomNode startRoom = new(0, new Vector2Int(0, 0), TowerRoomType.Start);
            map.AddRoom(startRoom);
            startRoom.Clear();

            int nextId = 1;
            foreach (Vector2Int cell in activeCells.OrderBy(cell => cell.x).ThenBy(cell => cell.y))
            {
                TowerRoomNode room = new(nextId++, ToRoomGridPosition(cell), TowerRoomType.Combat);
                nodesByCell[cell] = room;
                map.AddRoom(room);
            }

            TowerRoomNode finalRoom = new(
                nextId,
                new Vector2Int(generatedRows + 1, 0),
                floorKey.IsBossStage ? TowerRoomType.Boss : TowerRoomType.Portal);

            map.AddRoom(finalRoom);

            ConnectStartToFirstRow(startRoom, nodesByCell);
            ConnectPathEdges(nodesByCell, pathEdges);
            ConnectLastRowToFinal(nodesByCell, finalRoom, generatedRows);

            AssignRoomTypes(floorKey, nodesByCell.Values.ToList(), generatedRows, rng);
            map.RevealFromCurrentRoom();
            return map;
        }

        private static void GenerateSlayStylePaths(
            int generatedRows,
            System.Random rng,
            HashSet<Vector2Int> activeCells,
            List<PathEdge> pathEdges)
        {
            HashSet<int> startLanes = new();

            for (int pathIndex = 0; pathIndex < PathCount; ++pathIndex)
            {
                int lane = PickStartLane(pathIndex, rng, startLanes);
                startLanes.Add(lane);

                Vector2Int currentCell = new(1, lane);
                activeCells.Add(currentCell);

                for (int row = 1; row < generatedRows; ++row)
                {
                    int nextLane = PickNextLane(row, lane, rng, pathEdges);
                    Vector2Int nextCell = new(row + 1, nextLane);

                    PathEdge edge = new(currentCell, nextCell);
                    if (!pathEdges.Contains(edge))
                        pathEdges.Add(edge);

                    activeCells.Add(nextCell);
                    currentCell = nextCell;
                    lane = nextLane;
                }
            }
        }

        private static int PickStartLane(int pathIndex, System.Random rng, HashSet<int> usedStartLanes)
        {
            if (pathIndex >= MinimumDistinctStartLanes)
                return rng.Next(LaneCount);

            List<int> candidates = Enumerable.Range(0, LaneCount).ToList();
            Shuffle(candidates, rng);

            foreach (int lane in candidates)
                if (!usedStartLanes.Contains(lane))
                    return lane;

            return rng.Next(LaneCount);
        }

        private static int PickNextLane(int row, int currentLane, System.Random rng, IReadOnlyList<PathEdge> existingEdges)
        {
            List<int> candidates = new();

            for (int delta = -1; delta <= 1; ++delta)
            {
                int lane = currentLane + delta;
                if (lane >= 0 && lane < LaneCount)
                    candidates.Add(lane);
            }

            Shuffle(candidates, rng);

            foreach (int candidate in candidates)
                if (!WouldCrossExistingEdge(row, currentLane, candidate, existingEdges))
                    return candidate;

            return currentLane;
        }

        private static bool WouldCrossExistingEdge(int row, int fromLane, int toLane, IReadOnlyList<PathEdge> existingEdges)
        {
            foreach (PathEdge edge in existingEdges)
            {
                if (edge.From.x != row)
                    continue;

                int otherFromLane = edge.From.y;
                int otherToLane = edge.To.y;

                if (fromLane < otherFromLane && toLane > otherToLane)
                    return true;

                if (fromLane > otherFromLane && toLane < otherToLane)
                    return true;
            }

            return false;
        }

        private static Vector2Int ToRoomGridPosition(Vector2Int cell)
        {
            int centerLane = LaneCount / 2;
            return new Vector2Int(cell.x, cell.y - centerLane);
        }

        private static void ConnectStartToFirstRow(TowerRoomNode startRoom, Dictionary<Vector2Int, TowerRoomNode> nodesByCell)
        {
            foreach (TowerRoomNode room in GetRoomsInRow(nodesByCell, 1))
                ConnectForward(startRoom, room);
        }

        private static void ConnectPathEdges(Dictionary<Vector2Int, TowerRoomNode> nodesByCell, IEnumerable<PathEdge> pathEdges)
        {
            foreach (PathEdge edge in pathEdges)
            {
                if (!nodesByCell.TryGetValue(edge.From, out TowerRoomNode fromRoom))
                    continue;

                if (!nodesByCell.TryGetValue(edge.To, out TowerRoomNode toRoom))
                    continue;

                ConnectForward(fromRoom, toRoom);
            }
        }

        private static void ConnectLastRowToFinal(Dictionary<Vector2Int, TowerRoomNode> nodesByCell, TowerRoomNode finalRoom, int generatedRows)
        {
            foreach (TowerRoomNode room in GetRoomsInRow(nodesByCell, generatedRows))
                ConnectForward(room, finalRoom);
        }

        private static IEnumerable<TowerRoomNode> GetRoomsInRow(Dictionary<Vector2Int, TowerRoomNode> nodesByCell, int row)
        {
            return nodesByCell
                .Where(pair => pair.Key.x == row)
                .OrderBy(pair => pair.Key.y)
                .Select(pair => pair.Value);
        }

        private static void AssignRoomTypes(
            TowerFloorKey floorKey,
            IReadOnlyList<TowerRoomNode> rooms,
            int generatedRows,
            System.Random rng)
        {
            if (rooms == null || rooms.Count == 0)
                return;

            int rewardRow = Mathf.Clamp((generatedRows + 1) / 2, 2, generatedRows - 1);

            foreach (TowerRoomNode room in rooms)
            {
                if (room.GridPosition.x == 1)
                    room.RoomType = TowerRoomType.Combat;
                else if (room.GridPosition.x == rewardRow)
                    room.RoomType = TowerRoomType.Reward;
            }

            List<TowerRoomNode> assignableRooms = rooms
                .Where(room => room.RoomType == TowerRoomType.Combat && room.GridPosition.x != 1)
                .OrderBy(room => room.GridPosition.x)
                .ThenBy(_ => rng.Next())
                .ToList();

            List<TowerRoomType> bucket = BuildRoomTypeBucket(assignableRooms.Count, floorKey, rng);

            foreach (TowerRoomNode room in assignableRooms)
            {
                int typeIndex = bucket.FindIndex(type => CanAssignRoomType(room, type, rooms));

                if (typeIndex < 0)
                {
                    room.RoomType = TowerRoomType.Combat;
                    continue;
                }

                room.RoomType = bucket[typeIndex];
                bucket.RemoveAt(typeIndex);
            }
        }

        private static List<TowerRoomType> BuildRoomTypeBucket(int roomCount, TowerFloorKey floorKey, System.Random rng)
        {
            int eliteCount = Mathf.RoundToInt(roomCount * (floorKey.IsBossStage ? EliteWeight * 1.25f : EliteWeight));
            int eventCount = Mathf.RoundToInt(roomCount * EventWeight);
            int rewardCount = Mathf.RoundToInt(roomCount * RewardWeight);

            List<TowerRoomType> bucket = new();
            AddTypes(bucket, TowerRoomType.EliteCombat, eliteCount);
            AddTypes(bucket, TowerRoomType.Event, eventCount);
            AddTypes(bucket, TowerRoomType.Reward, rewardCount);

            while (bucket.Count < roomCount)
                bucket.Add(TowerRoomType.Combat);

            Shuffle(bucket, rng);
            return bucket;
        }

        private static void AddTypes(List<TowerRoomType> bucket, TowerRoomType type, int count)
        {
            for (int i = 0; i < count; ++i)
                bucket.Add(type);
        }

        private static bool CanAssignRoomType(TowerRoomNode room, TowerRoomType type, IReadOnlyList<TowerRoomNode> allRooms)
        {
            if (type == TowerRoomType.EliteCombat && room.GridPosition.x < EliteMinimumRow)
                return false;

            if (HasSameSpecialParent(room, type, allRooms))
                return false;

            if (HasSameTypeSibling(room, type, allRooms))
                return false;

            return true;
        }

        private static bool HasSameSpecialParent(TowerRoomNode room, TowerRoomType type, IReadOnlyList<TowerRoomNode> allRooms)
        {
            if (type is not (TowerRoomType.EliteCombat or TowerRoomType.Reward))
                return false;

            foreach (TowerRoomNode parent in allRooms)
                if (parent.RoomType == type && parent.IsConnectedTo(room.Id))
                    return true;

            return false;
        }

        private static bool HasSameTypeSibling(TowerRoomNode room, TowerRoomType type, IReadOnlyList<TowerRoomNode> allRooms)
        {
            foreach (TowerRoomNode parent in allRooms)
            {
                if (!parent.IsConnectedTo(room.Id))
                    continue;

                foreach (int siblingId in parent.ConnectedRoomIds)
                {
                    if (siblingId == room.Id)
                        continue;

                    TowerRoomNode sibling = allRooms.FirstOrDefault(candidate => candidate.Id == siblingId);
                    if (sibling != null && sibling.RoomType == type)
                        return true;
                }
            }

            return false;
        }

        private static void ConnectForward(TowerRoomNode fromRoom, TowerRoomNode toRoom)
        {
            fromRoom.AddConnection(toRoom.Id);
        }

        private static void Shuffle<T>(IList<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; --i)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static int MixSeed(int seed, TowerFloorKey floorKey)
        {
            unchecked
            {
                int hash = seed;
                hash = hash * 397 ^ floorKey.TowerFloor;
                hash = hash * 397 ^ floorKey.StageFloor;
                return hash;
            }
        }
    }
}

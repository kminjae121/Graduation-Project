using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Tower
{
    public static class TowerMapGenerator
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        public static TowerFloorMap Generate(TowerFloorKey floorKey, int seed = 0)
        {
            int actualSeed = seed != 0 ? seed : Random.Range(int.MinValue, int.MaxValue);
            System.Random rng = new(actualSeed);
            int roomCount = floorKey.IsBossStage ? rng.Next(6, 9) : rng.Next(8, 12);

            TowerFloorMap map = new(floorKey);
            Dictionary<Vector2Int, TowerRoomNode> roomsByPosition = new();
            List<TowerRoomNode> rooms = new();

            TowerRoomNode startRoom = new(0, Vector2Int.zero, TowerRoomType.Start);
            AddRoom(map, roomsByPosition, rooms, startRoom);
            startRoom.Clear();

            for (int id = 1; id < roomCount; ++id)
            {
                TowerRoomNode parent = PickExpandableRoom(rooms, roomsByPosition, rng);
                Vector2Int position = PickFreeNeighbor(parent.GridPosition, roomsByPosition, rng);

                TowerRoomNode room = new(id, position, TowerRoomType.Combat);
                AddRoom(map, roomsByPosition, rooms, room);
                Connect(parent, room);
            }

            AddExtraConnections(rooms, roomsByPosition, rng);
            AssignRoomTypes(floorKey, rooms, rng);
            map.RevealFromCurrentRoom();
            return map;
        }

        private static void AddRoom(
            TowerFloorMap map,
            Dictionary<Vector2Int, TowerRoomNode> roomsByPosition,
            List<TowerRoomNode> rooms,
            TowerRoomNode room)
        {
            roomsByPosition[room.GridPosition] = room;
            rooms.Add(room);
            map.AddRoom(room);
        }

        private static TowerRoomNode PickExpandableRoom(
            IReadOnlyList<TowerRoomNode> rooms,
            IReadOnlyDictionary<Vector2Int, TowerRoomNode> roomsByPosition,
            System.Random rng)
        {
            List<TowerRoomNode> candidates = rooms
                .Where(room => HasFreeNeighbor(room.GridPosition, roomsByPosition))
                .ToList();

            if (candidates.Count == 0)
                return rooms[^1];

            return candidates[rng.Next(candidates.Count)];
        }

        private static bool HasFreeNeighbor(Vector2Int position, IReadOnlyDictionary<Vector2Int, TowerRoomNode> roomsByPosition)
        {
            foreach (Vector2Int dir in Directions)
                if (!roomsByPosition.ContainsKey(position + dir))
                    return true;

            return false;
        }

        private static Vector2Int PickFreeNeighbor(
            Vector2Int position,
            IReadOnlyDictionary<Vector2Int, TowerRoomNode> roomsByPosition,
            System.Random rng)
        {
            List<Vector2Int> candidates = Directions
                .Select(dir => position + dir)
                .Where(candidate => !roomsByPosition.ContainsKey(candidate))
                .ToList();

            return candidates[rng.Next(candidates.Count)];
        }

        private static void AddExtraConnections(
            IReadOnlyList<TowerRoomNode> rooms,
            IReadOnlyDictionary<Vector2Int, TowerRoomNode> roomsByPosition,
            System.Random rng)
        {
            foreach (TowerRoomNode room in rooms)
            {
                if (rng.NextDouble() > 0.18f)
                    continue;

                foreach (Vector2Int dir in Directions.OrderBy(_ => rng.Next()))
                {
                    if (!roomsByPosition.TryGetValue(room.GridPosition + dir, out TowerRoomNode neighbor))
                        continue;

                    Connect(room, neighbor);
                    break;
                }
            }
        }

        private static void AssignRoomTypes(TowerFloorKey floorKey, IReadOnlyList<TowerRoomNode> rooms, System.Random rng)
        {
            TowerRoomNode farthestRoom = rooms
                .Where(room => room.RoomType != TowerRoomType.Start)
                .OrderByDescending(room => room.GridPosition.sqrMagnitude)
                .FirstOrDefault();

            if (farthestRoom == null)
                return;

            farthestRoom.RoomType = floorKey.IsBossStage ? TowerRoomType.Boss : TowerRoomType.Portal;

            List<TowerRoomNode> normalRooms = rooms
                .Where(room => room.RoomType == TowerRoomType.Combat && room.Id != 0)
                .OrderBy(_ => rng.Next())
                .ToList();

            int rewardCount = Mathf.Clamp(rooms.Count / 5, 1, 2);
            int eventCount = Mathf.Clamp(rooms.Count / 4, 1, 2);
            int eliteCount = floorKey.IsBossStage ? 1 : Mathf.Clamp(rooms.Count / 6, 1, 2);

            AssignType(normalRooms, TowerRoomType.Reward, rewardCount);
            AssignType(normalRooms, TowerRoomType.Event, eventCount);
            AssignType(normalRooms, TowerRoomType.EliteCombat, eliteCount);

            foreach (TowerRoomNode room in normalRooms)
                room.RoomType = TowerRoomType.Combat;
        }

        private static void AssignType(List<TowerRoomNode> rooms, TowerRoomType type, int count)
        {
            for (int i = 0; i < count && rooms.Count > 0; ++i)
            {
                TowerRoomNode room = rooms[0];
                rooms.RemoveAt(0);
                room.RoomType = type;
            }
        }

        private static void Connect(TowerRoomNode a, TowerRoomNode b)
        {
            a.AddConnection(b.Id);
            b.AddConnection(a.Id);
        }
    }
}

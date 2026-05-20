using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Tower
{
    public static class TowerMapGenerator
    {
        public static TowerFloorMap Generate(TowerFloorKey floorKey, int seed = 0)
        {
            int actualSeed = seed != 0 ? seed : Random.Range(int.MinValue, int.MaxValue);
            System.Random rng = new(actualSeed);

            int middleLayerCount = floorKey.IsBossStage ? 2 : 3;
            int targetRoomCount = floorKey.IsBossStage ? rng.Next(6, 9) : rng.Next(9, 13);

            TowerFloorMap map = new(floorKey);
            List<TowerRoomNode> rooms = new();
            List<List<TowerRoomNode>> layers = new();

            TowerRoomNode startRoom = new(0, Vector2Int.zero, TowerRoomType.Start);
            AddRoom(map, rooms, startRoom);
            startRoom.Clear();
            layers.Add(new List<TowerRoomNode> { startRoom });

            int nextId = 1;
            int remainingMiddleRooms = Mathf.Max(1, targetRoomCount - 2);

            for (int layerIndex = 1; layerIndex <= middleLayerCount; layerIndex++)
            {
                int layersLeft = middleLayerCount - layerIndex;
                int maxForLayer = Mathf.Min(floorKey.IsBossStage ? 2 : 3, remainingMiddleRooms - layersLeft);
                int minForLayer = Mathf.Min(layerIndex == 1 ? 2 : 1, maxForLayer);
                int roomCount = rng.Next(minForLayer, maxForLayer + 1);

                List<TowerRoomNode> layer = new();
                foreach (int y in GetLayerYPositions(roomCount))
                {
                    TowerRoomNode room = new(nextId++, new Vector2Int(layerIndex, y), TowerRoomType.Combat);
                    AddRoom(map, rooms, room);
                    layer.Add(room);
                }

                layers.Add(layer);
                remainingMiddleRooms -= roomCount;
            }

            TowerRoomNode finalRoom = new(
                nextId,
                new Vector2Int(middleLayerCount + 1, 0),
                floorKey.IsBossStage ? TowerRoomType.Boss : TowerRoomType.Portal);

            AddRoom(map, rooms, finalRoom);
            layers.Add(new List<TowerRoomNode> { finalRoom });

            for (int i = 0; i < layers.Count - 1; i++)
                ConnectLayer(layers[i], layers[i + 1], rng);

            AssignRoomTypes(floorKey, rooms, finalRoom, rng);
            map.RevealFromCurrentRoom();
            return map;
        }

        private static void AddRoom(TowerFloorMap map, List<TowerRoomNode> rooms, TowerRoomNode room)
        {
            rooms.Add(room);
            map.AddRoom(room);
        }

        private static IEnumerable<int> GetLayerYPositions(int count)
        {
            return count switch
            {
                1 => new[] { 0 },
                2 => new[] { 1, -1 },
                3 => new[] { 1, 0, -1 },
                _ => Enumerable.Range(0, count).Select(index => index - count / 2)
            };
        }

        private static void ConnectLayer(IReadOnlyList<TowerRoomNode> previousLayer, IReadOnlyList<TowerRoomNode> nextLayer, System.Random rng)
        {
            if (previousLayer == null || nextLayer == null || previousLayer.Count == 0 || nextLayer.Count == 0)
                return;

            foreach (TowerRoomNode nextRoom in nextLayer)
            {
                TowerRoomNode previousRoom = previousLayer[rng.Next(previousLayer.Count)];
                Connect(previousRoom, nextRoom);
            }

            foreach (TowerRoomNode previousRoom in previousLayer)
            {
                TowerRoomNode nextRoom = nextLayer[rng.Next(nextLayer.Count)];
                Connect(previousRoom, nextRoom);

                if (nextLayer.Count > 1 && rng.NextDouble() < 0.42f)
                {
                    TowerRoomNode extraRoom = nextLayer[rng.Next(nextLayer.Count)];
                    Connect(previousRoom, extraRoom);
                }
            }
        }

        private static void AssignRoomTypes(
            TowerFloorKey floorKey,
            IReadOnlyList<TowerRoomNode> rooms,
            TowerRoomNode finalRoom,
            System.Random rng)
        {
            List<TowerRoomNode> normalRooms = rooms
                .Where(room => room.RoomType == TowerRoomType.Combat && room.Id != finalRoom.Id)
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

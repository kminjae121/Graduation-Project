using System.Collections.Generic;
using UnityEngine;

namespace Code.Tower
{
    [CreateAssetMenu(menuName = "Tower/Floor Map Definition", fileName = "TowerFloorMapDefinition")]
    public sealed class TowerFloorMapDefinition : ScriptableObject
    {
        [SerializeField] private int towerFloor = 1;
        [SerializeField, Range(1, 5)] private int stageFloor = 1;
        [SerializeField] private List<TowerMapRoomDefinition> rooms = new();

        public TowerFloorKey FloorKey => new(towerFloor, stageFloor);

        public bool Matches(TowerFloorKey floorKey)
        {
            return FloorKey.TowerFloor == floorKey.TowerFloor &&
                   FloorKey.StageFloor == floorKey.StageFloor;
        }

        public TowerFloorMap BuildMap()
        {
            if (rooms == null || rooms.Count == 0)
            {
                Debug.LogWarning($"[{name}] 맵 정의에 방이 없습니다.", this);
                return null;
            }

            TowerFloorMap map = new(FloorKey);
            Dictionary<int, TowerRoomNode> nodesById = new();

            foreach (TowerMapRoomDefinition roomDefinition in rooms)
            {
                if (roomDefinition == null)
                    continue;

                if (nodesById.ContainsKey(roomDefinition.Id))
                {
                    Debug.LogWarning($"[{name}] 중복 방 ID가 있습니다: {roomDefinition.Id}", this);
                    continue;
                }

                TowerRoomNode node = roomDefinition.CreateNode();
                nodesById.Add(node.Id, node);
                map.AddRoom(node);
            }

            foreach (TowerMapRoomDefinition roomDefinition in rooms)
            {
                if (roomDefinition == null || !nodesById.TryGetValue(roomDefinition.Id, out TowerRoomNode fromNode))
                    continue;

                foreach (int connectedRoomId in roomDefinition.ConnectedRoomIds)
                {
                    if (connectedRoomId == fromNode.Id)
                        continue;

                    if (!nodesById.TryGetValue(connectedRoomId, out TowerRoomNode toNode))
                    {
                        Debug.LogWarning($"[{name}] {fromNode.Id}번 방이 존재하지 않는 {connectedRoomId}번 방과 연결되어 있습니다.", this);
                        continue;
                    }

                    fromNode.AddConnection(toNode.Id);
                    toNode.AddConnection(fromNode.Id);
                }
            }

            TowerRoomNode startRoom = map.GetCurrentRoom();
            if (startRoom == null)
            {
                Debug.LogWarning($"[{name}] 시작방(Start)이 필요합니다.", this);
                return null;
            }

            startRoom.Clear();
            map.RevealFromCurrentRoom();
            return map;
        }
    }
}

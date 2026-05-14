using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Tower
{
    [Serializable]
    public sealed class TowerFloorMap
    {
        private readonly Dictionary<int, TowerRoomNode> _roomsById = new();

        public TowerFloorKey FloorKey { get; }
        public int StartRoomId { get; private set; }
        public int CurrentRoomId { get; private set; }
        public IReadOnlyList<TowerRoomNode> Rooms => _roomsById.Values.OrderBy(room => room.Id).ToList();

        public TowerFloorMap(TowerFloorKey floorKey)
        {
            FloorKey = floorKey;
        }

        public void AddRoom(TowerRoomNode room)
        {
            if (room == null)
                return;

            _roomsById[room.Id] = room;

            if (room.RoomType == TowerRoomType.Start)
            {
                StartRoomId = room.Id;
                CurrentRoomId = room.Id;
            }
        }

        public bool TryGetRoom(int id, out TowerRoomNode room)
            => _roomsById.TryGetValue(id, out room);

        public TowerRoomNode GetCurrentRoom()
        {
            TryGetRoom(CurrentRoomId, out TowerRoomNode room);
            return room;
        }

        public IEnumerable<TowerRoomNode> GetConnectedRooms(int roomId)
        {
            if (!TryGetRoom(roomId, out TowerRoomNode room))
                yield break;

            foreach (int connectedId in room.ConnectedRoomIds)
                if (TryGetRoom(connectedId, out TowerRoomNode connectedRoom))
                    yield return connectedRoom;
        }

        public bool CanMoveTo(int roomId)
        {
            TowerRoomNode currentRoom = GetCurrentRoom();
            return currentRoom != null && currentRoom.IsConnectedTo(roomId);
        }

        public bool MoveTo(int roomId)
        {
            if (!CanMoveTo(roomId))
                return false;

            CurrentRoomId = roomId;
            RevealFromCurrentRoom();
            return true;
        }

        public void RevealFromCurrentRoom()
        {
            TowerRoomNode currentRoom = GetCurrentRoom();

            if (currentRoom == null)
                return;

            currentRoom.Visit();

            foreach (TowerRoomNode connectedRoom in GetConnectedRooms(CurrentRoomId))
                connectedRoom.Discover();
        }

        public void ClearCurrentRoom()
            => GetCurrentRoom()?.Clear();
    }
}

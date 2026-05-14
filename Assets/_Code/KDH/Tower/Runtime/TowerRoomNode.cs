using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Tower
{
    [Serializable]
    public sealed class TowerRoomNode
    {
        private readonly List<int> _connectedRoomIds = new();

        public int Id { get; }
        public Vector2Int GridPosition { get; }
        public TowerRoomType RoomType { get; set; }
        public TowerRoomState State { get; private set; }
        public IReadOnlyList<int> ConnectedRoomIds => _connectedRoomIds;

        public bool IsDiscovered => HasState(TowerRoomState.Discovered);
        public bool IsVisited => HasState(TowerRoomState.Visited);
        public bool IsCleared => HasState(TowerRoomState.Cleared);

        public TowerRoomNode(int id, Vector2Int gridPosition, TowerRoomType roomType)
        {
            Id = id;
            GridPosition = gridPosition;
            RoomType = roomType;
        }

        public void AddConnection(int roomId)
        {
            if (roomId == Id || _connectedRoomIds.Contains(roomId))
                return;

            _connectedRoomIds.Add(roomId);
        }

        public bool IsConnectedTo(int roomId)
            => _connectedRoomIds.Contains(roomId);

        public void Discover()
            => State |= TowerRoomState.Discovered;

        public void Visit()
            => State |= TowerRoomState.Discovered | TowerRoomState.Visited;

        public void Clear()
            => State |= TowerRoomState.Discovered | TowerRoomState.Visited | TowerRoomState.Cleared;

        public bool HasState(TowerRoomState state)
            => (State & state) == state;
    }
}

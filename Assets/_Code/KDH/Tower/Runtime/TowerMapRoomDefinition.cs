using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Tower
{
    [Serializable]
    public sealed class TowerMapRoomDefinition
    {
        [SerializeField] private int id;
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private TowerRoomType roomType = TowerRoomType.Combat;
        [SerializeField] private List<int> connectedRoomIds = new();

        public int Id => id;
        public Vector2Int GridPosition => gridPosition;
        public TowerRoomType RoomType => roomType;
        public IReadOnlyList<int> ConnectedRoomIds => connectedRoomIds;

        public TowerRoomNode CreateNode()
        {
            return new TowerRoomNode(id, gridPosition, roomType);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Navigation
{
    [Serializable]
    public class NodeData
    {
        public Vector3 worldPos;
        public Vector3Int cellPos;
        public List<LinkData> neighbors;

        public NodeData(Vector3 worldPos, Vector3Int cellPos)
        {
            this.worldPos = worldPos;
            this.cellPos = cellPos;
            neighbors = new List<LinkData>();
        }

        public void SetCellPos(Vector3Int newCellPos)
        {
            cellPos = newCellPos;
        }

        public void AddNeighbor(NodeData neighborNode)
        {
            neighbors.Add(new LinkData
            {
                startPos = worldPos,
                startCellPos = cellPos,
                endPos = neighborNode.worldPos,
                endCellPos = neighborNode.cellPos,
                cost = Vector3Int.Distance(cellPos, neighborNode.cellPos)
            });
        }

        public override int GetHashCode()
            => cellPos.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is NodeData data)
                return data.cellPos == cellPos;

            return false;
        }

        public static bool operator ==(NodeData a, NodeData b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(NodeData a, NodeData b)
            => !(a == b);
    }
}

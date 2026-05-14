using System;
using UnityEngine;

namespace Code.Navigation
{
    public class AstarNode : IComparable<AstarNode>
    {
        public Vector3 worldPos;
        public Vector3Int cellPos;
        public NodeData nodeData;

        public AstarNode parentNode;

        public float g;
        public float f;

        public int CompareTo(AstarNode other)
        {
            if (Mathf.Approximately(f, other.f))
                return 0;

            return f > other.f ? -1 : 1;
        }

        public override bool Equals(object obj)
        {
            if (obj is AstarNode node)
                return Equals(node);

            return false;
        }

        public override int GetHashCode()
            => cellPos.GetHashCode();

        public bool Equals(AstarNode other)
        {
            if (other is null)
                return false;

            return cellPos == other.cellPos;
        }

        public static bool operator ==(AstarNode a, AstarNode b)
        {
            if (a is null)
                return b is null;

            return a.Equals(b);
        }

        public static bool operator !=(AstarNode a, AstarNode b)
            => !(a == b);
    }
}
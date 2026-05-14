using System;
using UnityEngine;

namespace Code.Navigation
{
    [Serializable]
    public struct LinkData
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public Vector3Int startCellPos;
        public Vector3Int endCellPos;
        public float cost;
    }
}
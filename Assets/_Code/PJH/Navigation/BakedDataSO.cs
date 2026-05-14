using System.Collections.Generic;
using Code.Map;
using UnityEngine;

namespace Code.Navigation
{
    [CreateAssetMenu(fileName = "Baked Data", menuName = "SO/Map/BakedData", order = 0)]
    public class BakedDataSO : ScriptableObject
    {
        public List<NodeData> points = new();
        private Dictionary<Vector3Int, NodeData> _pointDict;

        private void OnEnable()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            _pointDict = new Dictionary<Vector3Int, NodeData>();

            foreach (NodeData node in points)
            {
                if (node == null)
                    continue;

                Vector3Int normalizedCellPos = GetNormalizedCellPos(node);
                node.SetCellPos(normalizedCellPos);

                _pointDict.TryAdd(normalizedCellPos, node);
            }
        }
        
        public void ClearPoints()
        {
            points?.Clear();
            _pointDict?.Clear();
        }
        
        public void AddPoint(Vector3 worldPos, Vector3Int cellPos)
        {
            points.Add(new NodeData(worldPos, cellPos));
        }

        public bool HasNode(Vector3Int cellPos)
            => _pointDict != null && _pointDict.ContainsKey(cellPos);
        
        public bool GetNodeIfExist(Vector3Int cellPos, out NodeData nodeData)
        {
            if (HasNode(cellPos))
            {
                nodeData = _pointDict[cellPos];
                return true;
            }

            nodeData = null;
            return false;
        }

        private static Vector3Int GetNormalizedCellPos(NodeData node)
        {
            if (node.neighbors != null && node.neighbors.Count > 0)
                return node.neighbors[0].startCellPos;

            if (GridMap.Instance == null)
                return node.cellPos;
            
            Vector2Int gridPos = GridMap.Instance.WorldToGridPos(node.worldPos);
            return new Vector3Int(gridPos.x, gridPos.y, 0);
        }
    }
}
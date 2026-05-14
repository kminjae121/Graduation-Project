using UnityEngine;

namespace Code.Expedition.Data
{
    [CreateAssetMenu(fileName = "ExpeditionNode", menuName = "SO/Expedition/NodeData")]
    public class ExpeditionNodeSO : ScriptableObject
    {
        public string nodeName;
        public ExpeditionNodeType nodeType;
        public Sprite icon;
    }
}
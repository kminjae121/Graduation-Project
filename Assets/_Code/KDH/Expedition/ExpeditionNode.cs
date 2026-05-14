using System.Collections.Generic;
using Code.Expedition.Data;
using EPOOutline;
using UnityEngine;

namespace Code.Expedition.Components
{
    public class ExpeditionNode : MonoBehaviour
    {
        [Header("Node Data")]
        [SerializeField] private ExpeditionNodeSO nodeData;
        [SerializeField] private string targetSceneName;
        [SerializeField] private bool isUnlocked = false;
        [SerializeField] private bool isCleared = false;

        [Header("Visual")]
        [SerializeField] private Outlinable outlinable;
        [SerializeField] private MeshRenderer nodeRenderer;
        [SerializeField] private Material clearedMaterial;
        [SerializeField] private Material unclearedMaterial;

        [Header("Connections")]
        [SerializeField] private List<ExpeditionPath> connectedPaths = new List<ExpeditionPath>();

        public ExpeditionNodeSO NodeData => nodeData;
        public string TargetSceneName => targetSceneName;
        public bool IsUnlocked => isUnlocked;
        public bool IsCleared => isCleared;
        public List<ExpeditionPath> ConnectedPaths => connectedPaths;

        private void Awake()
        {
            if (outlinable == null)
                outlinable = GetComponent<Outlinable>();
            
            if (nodeRenderer == null)
                nodeRenderer = GetComponent<MeshRenderer>();

            SetOutline(false);
        }

        public void SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
        }

        public void SetCleared(bool cleared)
        {
            isCleared = cleared;
        }

        public void SetOutline(bool isActive)
        {
            if (outlinable != null)
            {
                outlinable.enabled = isActive;
            }
        }

        public void SetReadyToMoveColor(bool isReady)
        {
            var visual = GetComponentInChildren<ExpeditionNodeVisual>();
            if (visual != null)
            {
                visual.SetIconColor(isReady ? Color.cyan : Color.white);
            }
        }
        
        public void UpdateMaterial(bool isCurrentNode)
        {
            if (nodeRenderer == null) return;

            if (isCleared || isCurrentNode)
            {
                if (clearedMaterial != null)
                    nodeRenderer.material = clearedMaterial;
            }
            else
            {
                if (unclearedMaterial != null)
                    nodeRenderer.material = unclearedMaterial;
            }
        }

        public ExpeditionPath GetPathTo(ExpeditionNode targetNode)
        {
            foreach (var path in connectedPaths)
            {
                if (path.TargetNode == targetNode)
                {
                    return path;
                }
            }
            return null;
        }
    }
}
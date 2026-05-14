using System.Collections.Generic;
using UnityEngine;

namespace Code.Expedition.Components
{
    [RequireComponent(typeof(LineRenderer))]
    public class ExpeditionPath : MonoBehaviour
    {
        [SerializeField] private ExpeditionNode targetNode;
        [SerializeField] private Transform[] waypoints;

        public ExpeditionNode TargetNode => targetNode;

        private void OnValidate()
        {
            DrawPathPreview();
        }

        public List<Vector3> GetCurvePoints(Vector3 startPosition)
        {
            List<Vector3> points = new List<Vector3>();
            points.Add(startPosition);

            if (waypoints != null && waypoints.Length > 0)
            {
                foreach (var wp in waypoints)
                {
                    if (wp != null)
                        points.Add(wp.position);
                }
            }

            if (targetNode != null)
            {
                points.Add(targetNode.transform.position);
            }

            return points;
        }

        private void DrawPathPreview()
        {
            if (targetNode == null) return;
            
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            List<Vector3> points = GetCurvePoints(transform.position); 
            
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
    }
}
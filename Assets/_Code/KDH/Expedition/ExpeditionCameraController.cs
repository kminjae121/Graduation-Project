using UnityEngine;

namespace Code.Expedition.Components
{
    public class ExpeditionCameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Offset Settings")]
        [SerializeField] private bool maintainInitialOffset = true;
        [SerializeField] private Vector3 manualOffset;

        private Vector3 _currentVelocity;
        private Vector3 _offset;

        private void Start()
        {
            if (target == null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) target = playerObj.transform;
            }

            if (target != null && maintainInitialOffset)
            {
                _offset = transform.position - target.position;
            }
            else
            {
                _offset = manualOffset;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desiredPosition = target.position + _offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, 1f / smoothSpeed);
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null && maintainInitialOffset)
            {
                _offset = transform.position - target.position;
            }
        }
    }
}
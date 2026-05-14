using DG.Tweening;
using UnityEngine;

namespace Code.UnitSystem
{
    public class UnitRotator : MonoBehaviour, IUnitComponent
    {
        private Vector3 _targetDirection;
        private Tween _rotationTween;

        private Unit _owner;
        
        public void Initialize(Unit owner)
        {
            _targetDirection = transform.forward;
            _owner = owner;
        }

        public void SetDir(Vector3 targetPosition, TweenCallback onComplete = null)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;
            
            if (direction.sqrMagnitude > 0.001f)
                _targetDirection = direction.normalized;

            RotationUnit(onComplete);
        }
        
        public void RotationUnit(TweenCallback onComplete = null)
        {
            if (_targetDirection.sqrMagnitude <= 0.001f)
            {
                onComplete?.Invoke();
                return;
            }

            var targetRotation = Quaternion.LookRotation(_targetDirection);

            _rotationTween?.Kill();

            if (Quaternion.Angle(_owner.transform.rotation, targetRotation) <= 0.1f)
            {
                _owner.transform.rotation = targetRotation;
                onComplete?.Invoke();
                return;
            }

            _rotationTween = _owner.transform
                .DORotateQuaternion(targetRotation, 0.5f)
                .OnComplete(onComplete);
        }

        private void OnDestroy()
        {
            _rotationTween?.Kill();
        }
    }
}

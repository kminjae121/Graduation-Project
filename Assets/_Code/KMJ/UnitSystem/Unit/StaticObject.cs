using UnityEngine;

namespace Code.UnitSystem
{
    public class StaticObject : MonoBehaviour
    {
        public bool lockToInitialRotation = true;


        public Vector3 fixedWorldEuler = Vector3.zero;

        Quaternion _fixedWorldRotation;

        void Awake()
        {
            if (lockToInitialRotation)
                _fixedWorldRotation = transform.rotation;          
            else
                _fixedWorldRotation = Quaternion.Euler(fixedWorldEuler);
        }

        void LateUpdate()
        {
            transform.rotation = _fixedWorldRotation;
        }

        public void RebindToCurrent()
        {
            _fixedWorldRotation = transform.rotation;
        }
    }
}
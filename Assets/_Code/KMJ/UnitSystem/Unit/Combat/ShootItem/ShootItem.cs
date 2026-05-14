 using UnityEngine;
using UnityEngine.Events;

namespace Code.UnitSystem.Combat
{
    public abstract class ShootItem : MonoBehaviour
    {
        [field : SerializeField] public string itemName { get; private set; }
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] protected bool isOwnTarget = true;
        
        public UnityEvent AtkEvent;

        protected ShootItemAttackManager _shootItemManager;
        [SerializeField] protected Rigidbody _rbCompo = null;
        protected GameObject _target = null;
        
        private void Awake()
        {
            _rbCompo = GetComponent<Rigidbody>();
        }

        public void SetTarget(GameObject target)
        {
            _target = target;

            if (_target == null) return;
            
            Vector3 dir = (_target.transform.position - transform.position).normalized;

            transform.rotation = Quaternion.Euler(transform.position - _target.transform.position);

            _rbCompo.AddForce(dir * _moveSpeed, ForceMode.Impulse);
        }

        public void SetShootItemCompo(ShootItemAttackManager shootItemManaer)
        {
            _shootItemManager = shootItemManaer;
        }

        public abstract void AttackEnd();

        private void OnTriggerEnter(Collider other)
        {;
            if (isOwnTarget)
            {
                if (other.GetComponentInChildren<UnitAnimation>())
                {
                    UnitAnimation animation = other.GetComponentInChildren<UnitAnimation>();
                    
                    if (_target == animation.gameObject)
                    {
                        _target = other.gameObject;
                        
                        _shootItemManager.hitEvent?.Invoke();
                        AtkEvent?.Invoke();
            
                        AttackEnd();     
                    }   
                }
            }
            else
            {
                if (other.GetComponentInChildren<UnitAnimation>())
                {
                    _target = other.gameObject;
                    
                    _shootItemManager.hitEvent?.Invoke();
                    AtkEvent?.Invoke();
                    
                    AttackEnd();     
                }
            }
        }
        
    }
}
using Code.Core.Events.Bus;
using Code.UnitSystem.Combat;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.UnitSystem.Enemies
{
    public class EnemyProjectile : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public PoolingItemSO PoolingType { get; private set; }
        [SerializeField] private float speed = 5f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private Rigidbody rigid;

        public GameObject GameObject => gameObject;

        private Pool _myPool;
        private Unit _owner;
        private GameObject _target;
        private DamageData _damageData;
        private float _addDamage;
        private float _timer;
        private bool _isHit;

        private void Awake()
        {
            if (rigid == null)
                rigid = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!gameObject.activeSelf)
                return;

            _timer += Time.deltaTime;

            if (_timer >= lifetime)
                Push();
        }

        public void Initialize(Unit owner, GameObject target, DamageData damageData, float addDamage)
        {
            _owner = owner;
            _target = target;
            _damageData = damageData;
            _addDamage = addDamage;
            _timer = 0f;
            _isHit = false;
        }

        public void Launch(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.001f)
                direction = transform.forward;

            transform.forward = direction.normalized;

            if (rigid != null)
                rigid.linearVelocity = direction.normalized * speed;
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
            _timer = 0f;
            _isHit = false;

            if (rigid != null)
            {
                rigid.linearVelocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isHit || _target == null)
                return;

            if (!other.transform.IsChildOf(_target.transform) && other.gameObject != _target)
                return;

            _isHit = true;
            Bus<DamageEvent>.Raise(new DamageEvent(_damageData, _target, _addDamage, _owner, false, false, 0.1f));
            Push();
        }

        private void Push()
        {
            _target = null;
            _owner = null;

            if (rigid != null)
            {
                rigid.linearVelocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
            }

            if (_myPool != null)
                _myPool.Push(this);
            else
                gameObject.SetActive(false);
        }
    }
}
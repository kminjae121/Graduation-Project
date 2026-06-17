using Code.Core.Events.Bus;
using Code.UnitSystem.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UnitSystem.Enemies
{
    public class BossGimmickObject : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float destroyDelay;
        [SerializeField] private UnityEvent onDamaged;
        [SerializeField] private UnityEvent onDestroyed;
        [SerializeField] private UnityEvent onFailed;

        private BossGimmickSpawner _spawner;
        private Unit _unit;
        private UnitHealth _health;
        private bool _finished;
        private bool _unitUnregistered;

        public bool IsFinished => _finished;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            _finished = false;
            _unitUnregistered = false;
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnDestroy()
        {
            if (!_finished)
                _spawner?.ReleaseGimmick(this);

            UnregisterUnit();
        }

        public void Initialize(BossGimmickSpawner spawner)
        {
            _spawner = spawner;
            CacheComponents();

            if (_unit is BossGimmickUnit gimmickUnit)
                gimmickUnit.ResetCountdown();
        }

        public void ClearWithoutComplete()
        {
            _spawner?.ReleaseGimmick(this);
            _finished = true;
            UnregisterUnit();
            Destroy(gameObject);
        }

        public void FailByTimeout()
        {
            if (_finished)
                return;

            _finished = true;
            onFailed?.Invoke();
            _spawner?.FailGimmick(this);
            UnregisterUnit();
            Destroy(gameObject);
        }

        private void CompleteByDestruction()
        {
            if (_finished)
                return;

            _finished = true;
            _unitUnregistered = true;
            onDestroyed?.Invoke();
            _spawner?.CompleteGimmick(this);

            if (destroyDelay > 0f)
                Destroy(gameObject, destroyDelay);
            else
                Destroy(gameObject);
        }

        private void CacheComponents()
        {
            _unit = GetComponent<Unit>();
            _health = GetComponent<UnitHealth>();
        }

        private void SubscribeEvents()
        {
            CacheComponents();

            if (_unit != null)
            {
                _unit.OnDeathEvent -= CompleteByDestruction;
                _unit.OnDeathEvent += CompleteByDestruction;
            }

            if (_health != null)
            {
                _health.OnHealthChangedEvent -= HandleHealthChanged;
                _health.OnHealthChangedEvent += HandleHealthChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_unit != null)
                _unit.OnDeathEvent -= CompleteByDestruction;

            if (_health != null)
                _health.OnHealthChangedEvent -= HandleHealthChanged;
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (!_finished)
                onDamaged?.Invoke();
        }

        private void UnregisterUnit()
        {
            if (_unitUnregistered || _unit == null)
                return;

            _unitUnregistered = true;
            Bus<UnitDeadEvent>.Raise(new UnitDeadEvent(_unit));
        }
    }
}

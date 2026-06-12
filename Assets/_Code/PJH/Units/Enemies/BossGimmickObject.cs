using Code.UnitSystem.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UnitSystem.Enemies
{
    public class BossGimmickObject : MonoBehaviour, IDamageable, ITargetHealthInfo
    {
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField] private Sprite icon;
        [SerializeField, Min(0f)] private float destroyDelay;
        [SerializeField] private UnityEvent onDamaged;
        [SerializeField] private UnityEvent onDestroyed;

        private BossGimmickSpawner _spawner;
        private bool _destroyed;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public Sprite Icon => icon;

        private void OnEnable()
        {
            ResetHealth();
        }

        private void OnDestroy()
        {
            if (!_destroyed)
                _spawner?.ReleaseGimmick(this);
        }

        public void Initialize(BossGimmickSpawner spawner)
        {
            _spawner = spawner;
            ResetHealth();
        }

        public void ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitNormal,
            Unit dealer, bool isCritical, bool isPenetrate)
        {
            if (_destroyed)
                return;

            int damage = Mathf.Max(0, damageData.damage);

            if (damage <= 0)
                return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            onDamaged?.Invoke();

            if (CurrentHealth <= 0f)
                Break();
        }

        public void ClearWithoutComplete()
        {
            _spawner?.ReleaseGimmick(this);
            _destroyed = true;
            Destroy(gameObject);
        }

        private void ResetHealth()
        {
            _destroyed = false;
            CurrentHealth = maxHealth;
        }

        private void Break()
        {
            if (_destroyed)
                return;

            _destroyed = true;
            onDestroyed?.Invoke();
            _spawner?.CompleteGimmick(this);

            if (destroyDelay > 0f)
                Destroy(gameObject, destroyDelay);
            else
                Destroy(gameObject);
        }
    }
}

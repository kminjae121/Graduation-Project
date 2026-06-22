using Code.Core.Events.Bus;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UnitSystem.Enemies
{
    public class BossGimmickUnit : Unit
    {
        [SerializeField, Min(1)] private int maxCountdown = 2;
        [SerializeField, Min(0f)] private float firstTurnGauge = 100f;
        [SerializeField, Min(0f)] private float destroyDelay;
        [SerializeField] private UnityEvent onDamaged;
        [SerializeField] private UnityEvent onDestroyed;
        [SerializeField] private UnityEvent onFailed;

        private BossGimmickSpawner _spawner;
        private bool _finished;
        private bool _unitUnregistered;
        private bool _endingTurn;

        public int RemainingTurns { get; private set; }
        public bool IsFinished => _finished;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _finished = false;
            _unitUnregistered = false;
            SubscribeEvents();
            ResetCountdown();
        }

        protected override void OnDisable()
        {
            UnsubscribeEvents();
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            if (!_finished)
                _spawner?.ReleaseGimmick(this);

            UnregisterUnit();
            base.OnDestroy();
        }

        public void Initialize(BossGimmickSpawner spawner)
        {
            _spawner = spawner;
            _finished = false;
            _unitUnregistered = false;
            ResetCountdown();
        }

        public void ResetCountdown()
        {
            RemainingTurns = Mathf.Max(1, maxCountdown);
            TurnGauge = Mathf.Max(0f, firstTurnGauge);
            _endingTurn = false;
        }

        public void ClearWithoutComplete()
        {
            if (_finished)
                return;

            _finished = true;
            _spawner?.ReleaseGimmick(this);
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

        public override void OnTurnStart()
        {
            base.OnTurnStart();
            TickCountdown();
            OnTurnEnd();
        }

        public override void OnTurnEnd()
        {
            if (_endingTurn || !isMyTurn)
                return;

            _endingTurn = true;
            base.OnTurnEnd();
            _endingTurn = false;
        }

        private void TickCountdown()
        {
            if (_finished)
                return;

            --RemainingTurns;

            if (RemainingTurns <= 0)
                FailByTimeout();
        }

        private void CompleteByDestruction()
        {
            if (_finished)
                return;

            _finished = true;
            UnregisterUnit();
            onDestroyed?.Invoke();
            _spawner?.CompleteGimmick(this);

            if (destroyDelay > 0f)
                Destroy(gameObject, destroyDelay);
            else
                Destroy(gameObject);
        }

        private void SubscribeEvents()
        {
            OnDeathEvent -= CompleteByDestruction;
            OnDeathEvent += CompleteByDestruction;

            if (HealthCompo == null)
                return;

            HealthCompo.OnHealthChangedEvent -= HandleHealthChanged;
            HealthCompo.OnHealthChangedEvent += HandleHealthChanged;
        }

        private void UnsubscribeEvents()
        {
            OnDeathEvent -= CompleteByDestruction;

            if (HealthCompo != null)
                HealthCompo.OnHealthChangedEvent -= HandleHealthChanged;
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (!_finished)
                onDamaged?.Invoke();
        }

        protected override void Dead()
        {
            UnregisterUnit();
        }

        private void UnregisterUnit()
        {
            if (_unitUnregistered)
                return;

            _unitUnregistered = true;
            Bus<UnitDeadEvent>.Raise(new UnitDeadEvent(this));
        }
    }
}

using UnityEngine;

namespace Code.UnitSystem.Enemies
{
    public class BossGimmickUnit : Unit
    {
        [SerializeField, Min(1)] private int maxCountdown = 2;
        [SerializeField, Min(0f)] private float firstTurnGauge = 100f;

        private BossGimmickObject _gimmick;
        private bool _endingTurn;

        public int RemainingTurns { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            CacheGimmick();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CacheGimmick();
            ResetCountdown();
        }

        public void ResetCountdown()
        {
            RemainingTurns = Mathf.Max(1, maxCountdown);
            TurnGauge = Mathf.Max(0f, firstTurnGauge);
            _endingTurn = false;
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
            CacheGimmick();

            if (_gimmick == null || _gimmick.IsFinished)
                return;

            --RemainingTurns;

            if (RemainingTurns <= 0)
                _gimmick.FailByTimeout();
        }

        private void CacheGimmick()
        {
            if (_gimmick == null)
                _gimmick = GetComponent<BossGimmickObject>();
        }
    }
}

using Code.Core.Events.Bus;
using Code.Core.Managers;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class TurnOrderRoundSlotUI : MonoBehaviour, IPoolable
    {
        [SerializeField] private PoolingItemSO poolingType;
        [SerializeField] private TurnManager turnManager;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI roundText;

        private Pool _pool;

        public PoolingItemSO PoolingType => poolingType;
        public GameObject GameObject => gameObject;

        private void Awake()
        {
            if (turnManager == null)
                turnManager = UnityEngine.Object.FindFirstObjectByType<TurnManager>();
        }

        private void OnEnable()
        {
            Bus<TurnOrderUpdateEvent>.Subscribe(HandleTurnOrderUpdate);
            RefreshCurrentRound();
        }

        private void OnDisable()
        {
            Bus<TurnOrderUpdateEvent>.Unsubscribe(HandleTurnOrderUpdate);
        }

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
        }

        public void ResetItem()
        {
            if (roundText != null)
            {
                roundText.text = string.Empty;
            }
        }

        public void ReturnToPool()
        {
            if (_pool != null)
            {
                _pool.Push(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Setup(RoundTracker roundTracker)
        {
            if (roundTracker != null)
            {
                SetRound(roundTracker.NextRound);
            }
        }

        public void SetRound(int round)
        {
            if (roundText != null)
            {
                roundText.text = round.ToString();
            }
        }

        private void HandleTurnOrderUpdate(TurnOrderUpdateEvent evt)
        {
            RefreshCurrentRound();
        }

        private void RefreshCurrentRound()
        {
            if (turnManager == null || turnManager.CurrentRound <= 0)
                return;

            SetRound(turnManager.CurrentRound);
        }
    }
}

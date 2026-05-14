using Code.Managers;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class TurnOrderRoundSlotUI : MonoBehaviour, IPoolable
    {
        [SerializeField] private PoolingItemSO poolingType;
        
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI roundText;
        
        private Pool _pool;

        public PoolingItemSO PoolingType => poolingType;
        public GameObject GameObject => gameObject;

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
            if (roundText != null && roundTracker != null)
            {
                roundText.text = roundTracker.NextRound.ToString();
            }
        }
    }
}
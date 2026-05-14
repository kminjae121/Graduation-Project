using TMPro;
using UnityEngine;
using GondrLib.ObjectPool.Runtime;

namespace Code.UI
{
    public class ArtifactStatSlotUI : MonoBehaviour, IPoolable
    {
        [Header("Pool Settings")]
        [SerializeField] private PoolingItemSO poolingType;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI statInfoText;
        [SerializeField] private TextMeshProUGUI statValueText;

        private GondrLib.ObjectPool.Runtime.Pool _pool;

        public PoolingItemSO PoolingType => poolingType;
        public GameObject GameObject => gameObject;

        public void SetUpPool(GondrLib.ObjectPool.Runtime.Pool pool) => _pool = pool;

        public void ResetItem()
        {
            statInfoText.text = "";
            statValueText.text = "";
            statValueText.color = Color.white;
        }

        public void ReturnToPool()
        {
            if (_pool != null) _pool.Push(this);
            else Destroy(gameObject);
        }

        public void SetStat(string infoStr, float value)
        {
            statInfoText.text = infoStr;

            if (value > 0)
            {
                statValueText.text = $"+{value}";
                if (ColorUtility.TryParseHtmlString("#28FF3B", out Color color))
                    statValueText.color = color;
            }
            else if (value < 0)
            {
                statValueText.text = $"{value}";
                if (ColorUtility.TryParseHtmlString("#EE0000", out Color color))
                    statValueText.color = color;
            }
            else
            {
                statValueText.text = "0";
                statValueText.color = Color.white;
            }
        }
    }
}
using System;
using Code.UnitSystem.Upgrade;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public class UpgradeNodeButton : MonoBehaviour, IPoolable, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Pooling Settings")]
        [SerializeField] private PoolingItemSO poolingType;

        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private Button button;

        private UpgradeNodeSO _nodeData;
        private Action<UpgradeNodeSO> _onClickCallback;
        private GondrLib.ObjectPool.Runtime.Pool _pool;

        private readonly Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        private readonly Color _unlockedColor = Color.white;

        public PoolingItemSO PoolingType => poolingType;
        public GameObject GameObject => gameObject;

        public void SetUpPool(GondrLib.ObjectPool.Runtime.Pool pool) => _pool = pool;

        public void ResetItem()
        {
            _nodeData = null;
            _onClickCallback = null;
            button.onClick.RemoveAllListeners();
        }

        public void ReturnToPool()
        {
            if (_pool != null) _pool.Push(this);
            else Destroy(gameObject);
        }

        public void SetData(UpgradeNodeSO data, Action<UpgradeNodeSO> onClick)
        {
            _nodeData = data;
            _onClickCallback = onClick;
            
            if (iconImage != null)
            {
                iconImage.sprite = data.icon;
                iconImage.color = data.isUnlocked ? _unlockedColor : _lockedColor;
            }
            
            if (lockOverlay != null) 
            {
                lockOverlay.SetActive(!data.isUnlocked);
            }

            button.onClick.AddListener(() => _onClickCallback?.Invoke(_nodeData));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_nodeData != null && iconImage != null)
            {
                iconImage.color = _unlockedColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_nodeData != null && iconImage != null)
            {
                iconImage.color = _nodeData.isUnlocked ? _unlockedColor : _lockedColor;
            }
        }
    }
}
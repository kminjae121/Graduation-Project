using Code.Core.Events.Bus;
using Code.Items;
using Code.UnitSystem.ArtifactSystem;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public class ArtifactButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPoolable
    {
        [Header("Pooling Settings")]
        [SerializeField] private PoolingItemSO poolingType;

        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject hoverImage; 
        [SerializeField] private Image rarityImage;

        [Header("Rarity Sprites")]
        [SerializeField] private Sprite commonSprite;
        [SerializeField] private Sprite uncommonSprite;
        [SerializeField] private Sprite rareSprite;
        [SerializeField] private Sprite epicSprite;
        [SerializeField] private Sprite legendarySprite;

        [Header("Normal Popup Settings")]
        [SerializeField] private Vector2 popupOffset;

        [Header("Equipped Popup Settings")]
        [SerializeField] private Vector2 equippedPopupOffset;

        [Header("Behavior Settings")]
        [SerializeField] private bool openPopupOnHover = true;

        private EquipmentItemSO _equipmentItem;
        private bool _isEquipped;
        private bool _isSelected;
        private GondrLib.ObjectPool.Runtime.Pool _pool;
        private RectTransform _rectTransform;

        public PoolingItemSO PoolingType => poolingType;
        public GameObject GameObject => gameObject;
        public Vector2 EquippedPopupOffset => equippedPopupOffset;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Bus<ArtifactPopupEvent>.Subscribe(HandlePopupEvent);
        }

        private void OnDestroy()
        {
            Bus<ArtifactPopupEvent>.Unsubscribe(HandlePopupEvent);
        }

        public RectTransform GetPivot() => _rectTransform;
        
        public Vector2 GetOffset() => popupOffset;

        public void SetUpPool(GondrLib.ObjectPool.Runtime.Pool pool) => _pool = pool;

        public void ResetItem()
        {
            _equipmentItem = null;
            _isEquipped = false;
            _isSelected = false;
            
            if (hoverImage != null) hoverImage.SetActive(false);
            if (rarityImage != null) rarityImage.gameObject.SetActive(false);
        }

        public void ReturnToPool()
        {
            if (_pool != null) _pool.Push(this);
            else Destroy(gameObject);
        }

        public void SetArtifact(EquipmentItemSO equipmentItem, bool isEquipped)
        {
            _equipmentItem = equipmentItem;
            iconImage.sprite = equipmentItem.itemIcon;
            iconImage.color = Color.white; 
            _isEquipped = isEquipped;

            if (hoverImage != null) hoverImage.SetActive(_isEquipped);
            ApplyRaritySprite(equipmentItem.rarity);
        }

        public Sprite GetRaritySprite(ArtifactRarity rarity)
        {
            switch (rarity)
            {
                case ArtifactRarity.Legendary: return legendarySprite;
                case ArtifactRarity.Epic: return epicSprite;
                case ArtifactRarity.Rare: return rareSprite;
                case ArtifactRarity.Uncommon: return uncommonSprite;
                case ArtifactRarity.Common: default: return commonSprite;
            }
        }

        private void ApplyRaritySprite(ArtifactRarity rarity)
        {
            if (rarityImage == null) return;
            
            rarityImage.gameObject.SetActive(true);
            rarityImage.sprite = GetRaritySprite(rarity);
        }

        private void HandlePopupEvent(ArtifactPopupEvent evt)
        {
            if (_equipmentItem != null && evt.EquipmentItem == _equipmentItem)
            {
                _isSelected = true;
                if (hoverImage != null && !_isEquipped) hoverImage.SetActive(true);
            }
            else
            {
                _isSelected = false;
                if (hoverImage != null && !_isEquipped) hoverImage.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_equipmentItem != null && hoverImage != null && !_isEquipped) hoverImage.SetActive(true);
            
            if (openPopupOnHover && _equipmentItem != null)
            {
                Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(_equipmentItem, _isEquipped, GetPivot(), GetOffset(), false));
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_equipmentItem != null && hoverImage != null && !_isSelected && !_isEquipped) hoverImage.SetActive(false);

            if (openPopupOnHover && _equipmentItem != null)
            {
                Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_equipmentItem == null) return;
            
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(_equipmentItem, _isEquipped, GetPivot(), GetOffset(), false));
            }
        }
    }
}
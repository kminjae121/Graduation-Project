using Code.Core.Events.Bus;
using Code.SkillSystem;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public class CharacterSkillButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPoolable
    {
        [Header("Pooling Settings")]
        [SerializeField] private PoolingItemSO poolingType;

        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject hoverImage;

        [Header("Hover Settings")]
        [SerializeField] private GameObject customHoverArea;

        [Header("Normal Popup Settings")]
        [SerializeField] private Vector2 popupOffset;

        [Header("Equipped Popup Settings")]
        [SerializeField] private Vector2 equippedPopupOffset;

        [Header("Combat Popup Settings")]
        [SerializeField] private Vector2 combatPopupOffset;

        public bool IsCombatMode { get; set; } = false;

        private SkillSO _skill;
        private bool _isEquipped;
        private bool _isTooltipSuppressed;
        private bool _isHovering;
        private GondrLib.ObjectPool.Runtime.Pool _pool;
        private HoverDetector _hoverDetector;
        private RectTransform _rectTransform;

        public PoolingItemSO PoolingType => poolingType;
        public GameObject GameObject => gameObject;
        
        public Vector2 EquippedPopupOffset => equippedPopupOffset;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (customHoverArea != null)
            {
                _hoverDetector = customHoverArea.GetComponent<HoverDetector>();
                if (_hoverDetector == null)
                    _hoverDetector = customHoverArea.AddComponent<HoverDetector>();

                _hoverDetector.OnEnter += HandleHoverEnter;
                _hoverDetector.OnExit += HandleHoverExit;
            }
        }

        private void OnDestroy()
        {
            if (_hoverDetector != null)
            {
                _hoverDetector.OnEnter -= HandleHoverEnter;
                _hoverDetector.OnExit -= HandleHoverExit;
            }
        }

        public RectTransform GetPivot() => _rectTransform;

        public Vector2 GetOffset()
        {
            if (IsCombatMode) return combatPopupOffset;
            return _isEquipped ? equippedPopupOffset : popupOffset;
        }

        public void SetUpPool(GondrLib.ObjectPool.Runtime.Pool pool) => _pool = pool;

        public void ResetItem()
        {
            _skill = null;
            _isEquipped = false;
            _isTooltipSuppressed = false;
            _isHovering = false;
            IsCombatMode = false;
            UpdateHoverState();
        }

        public void ReturnToPool()
        {
            if (_pool != null) _pool.Push(this);
            else Destroy(gameObject);
        }

        public void SetEmptySlot(Sprite emptySprite)
        {
            ResetItem();
            if (iconImage != null)
            {
                iconImage.sprite = emptySprite;
                iconImage.color = Color.white;
            }
        }

        private void OnDisable()
        {
            _isHovering = false;
            _isTooltipSuppressed = false;
            UpdateHoverState();
            
            if (_skill != null) 
            {
                if (IsCombatMode) Bus<CombatSkillHoverEvent>.Raise(new CombatSkillHoverEvent(null, null));
                else Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            }
        }

        public void SetSkill(SkillSO skill, bool isEquipped)
        {
            _skill = skill;
            if (iconImage != null)
            {
                iconImage.sprite = skill.skillUIImage;
                iconImage.color = Color.white;
            }
            _isEquipped = isEquipped;
            UpdateHoverState();
        }

        private void UpdateHoverState()
        {
            if (hoverImage != null) hoverImage.SetActive(_isEquipped || _isHovering);
        }

        private void HandleHoverEnter()
        {
            if (_skill != null) 
            {
                _isHovering = true;
                UpdateHoverState();
                
                if (!_isTooltipSuppressed)
                {
                    if (IsCombatMode) Bus<CombatSkillHoverEvent>.Raise(new CombatSkillHoverEvent(_skill, GetPivot(), GetOffset()));
                    else Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(_skill, GetPivot(), GetOffset()));
                }
            }
        }

        private void HandleHoverExit()
        {
            if (_skill != null) 
            {
                _isHovering = false;
                UpdateHoverState();
                
                _isTooltipSuppressed = false;
                
                if (IsCombatMode) Bus<CombatSkillHoverEvent>.Raise(new CombatSkillHoverEvent(null, null));
                else Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (customHoverArea == null) HandleHoverEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (customHoverArea == null) HandleHoverExit();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_skill == null || IsCombatMode) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
                Bus<SkillEquipPopupEvent>.Raise(new SkillEquipPopupEvent(_skill, _isEquipped, GetPivot(), GetOffset()));
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (!_isTooltipSuppressed)
                {
                    _isTooltipSuppressed = true;
                    Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
                }
                else
                {
                    Bus<SkillEquipPopupEvent>.Raise(new SkillEquipPopupEvent(_skill, _isEquipped, GetPivot(), GetOffset()));
                }
            }
        }
    }
}
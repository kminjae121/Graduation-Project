using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public class SlotHoverClickTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Action<RectTransform, Vector2> OnClick;
        public Action<RectTransform, Vector2> OnLeftClick;
        public Action<RectTransform, Vector2> OnRightClick;
        public Action<RectTransform, Vector2> OnHoverEnter;
        public Action OnHoverExit;

        [Header("Hover Effect")]
        public GameObject hoverImage; 
        public bool useHoverVisuals = true;

        [Header("Popup Settings (Normal)")]
        [SerializeField] private Vector2 popupOffset;

        [Header("Popup Settings (Equipped)")]
        [SerializeField] private bool isEquippedSlot = false;
        [SerializeField] private Vector2 equippedPopupOffset;

        private Image _image;
        private Color _normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        private Color _hoverColor = Color.white;
        private bool _isInteractable = true;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            if (hoverImage != null) hoverImage.SetActive(false);
        }

        public RectTransform GetPivot() => _rectTransform;
        public Vector2 GetOffset() => isEquippedSlot ? equippedPopupOffset : popupOffset;

        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;
            
            if (_image != null)
            {
                if (useHoverVisuals && hoverImage == null)
                    _image.color = interactable ? _normalColor : Color.white;
                else
                    _image.color = Color.white;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isInteractable) return;
            
            if (useHoverVisuals)
            {
                if (hoverImage != null) hoverImage.SetActive(true);
                else if (_image != null) _image.color = _hoverColor;
            }
            
            OnHoverEnter?.Invoke(GetPivot(), GetOffset());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isInteractable) return;
            
            if (useHoverVisuals)
            {
                if (hoverImage != null) hoverImage.SetActive(false);
                else if (_image != null) _image.color = _normalColor;
            }
            
            OnHoverExit?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isInteractable) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnClick?.Invoke(GetPivot(), GetOffset());
                OnLeftClick?.Invoke(GetPivot(), GetOffset());
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick?.Invoke(GetPivot(), GetOffset());
            }
        }
    }
}
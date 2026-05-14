using Code.Core.Events.Bus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public class PartyCharacterSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image slotImage;
        [SerializeField] private Button slotButton;
        
        [Header("Data")]
        [SerializeField] private UnitSO characterInfo;

        private bool _isSelected;

        private void Awake()
        {
            if (slotButton != null)
                slotButton.onClick.AddListener(HandleSlotButton);
                
            Bus<PartyCharacterSelectEvent>.Subscribe(HandleCharacterSelected);
            Bus<PartyCharacterDeselectEvent>.Subscribe(HandleCharacterDeselected);

            if (characterInfo != null && slotImage != null)
            {
                slotImage.sprite = characterInfo.UnitImage;
            }
        }

        private void OnDestroy()
        {
            if (slotButton != null)
                slotButton.onClick.RemoveListener(HandleSlotButton);
            
            Bus<PartyCharacterSelectEvent>.Unsubscribe(HandleCharacterSelected);
            Bus<PartyCharacterDeselectEvent>.Unsubscribe(HandleCharacterDeselected);
        }

        private void HandleSlotButton()
        {
            if (characterInfo == null) return;
            
            if (_isSelected) return;
            Bus<PartyCharacterSelectEvent>.Raise(new PartyCharacterSelectEvent(characterInfo));
        }

        private void HandleCharacterSelected(PartyCharacterSelectEvent evt)
        {
            if (evt.Unit == characterInfo) _isSelected = true;
        }

        private void HandleCharacterDeselected(PartyCharacterDeselectEvent evt)
        {
            if (evt.Unit == characterInfo) _isSelected = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (characterInfo != null)
            {
                Bus<PartyCharacterHoverEvent>.Raise(new PartyCharacterHoverEvent(characterInfo));
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Bus<PartyCharacterHoverEvent>.Raise(new PartyCharacterHoverEvent(null));
        }
    }
}
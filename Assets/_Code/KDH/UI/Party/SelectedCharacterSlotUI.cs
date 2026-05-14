using Code.Core.Events.Bus;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class SelectedCharacterSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image slotImage;
        [SerializeField] private Button slotButton;
        [SerializeField] private Sprite defaultSprite;

        private UnitSO _unitData;

        private void Awake()
        {
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(OnSlotButtonClicked);
            }
        }

        private void Start()
        {
            UpdateSlot(null);
        }

        private void OnDestroy()
        {
            if (slotButton != null)
            {
                slotButton.onClick.RemoveListener(OnSlotButtonClicked);
            }
        }

        public void UpdateSlot(UnitSO unit)
        {
            _unitData = unit;

            if (_unitData != null)
            {
                slotImage.sprite = _unitData.UnitImage;
                slotImage.gameObject.SetActive(true);
            }
            else
            {
                slotImage.sprite = defaultSprite;
                slotImage.gameObject.SetActive(defaultSprite != null);
            }
        }

        private void OnSlotButtonClicked()
        {
            if (_unitData == null) return;
            Bus<PartyCharacterDeselectEvent>.Raise(new PartyCharacterDeselectEvent(_unitData));
        }
    }
}
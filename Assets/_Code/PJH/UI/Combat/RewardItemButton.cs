using Code.Core.Debugs;
using Code.Items;
using Code.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class RewardItemButton : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button itemButton;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private Image itemImage;

        private ItemSO _item;
        
        private void Awake()
        {
            itemButton.onClick.AddListener(HandleItemButton);
        }

        private void OnDestroy()
        {
            itemButton.onClick.RemoveListener(HandleItemButton);
        }
        
        public void SetItem(ItemSO newItem)
        {
            _item = newItem;

            itemNameText.text = _item.itemName;
            itemImage.sprite = _item.itemIcon;
        }

        private void HandleItemButton()
        {
            if (_item == null)
                return;

            switch (_item)
            {
                case CurrencyItemSO currency:
                    HandleCurrency(currency);
                    break;
                case EquipmentItemSO equipment:
                    HandleEquipment(equipment);
                    break;
            }
            
            gameObject.SetActive(false);
        }

        private void HandleCurrency(CurrencyItemSO currency)
        {
            PlayerManager.Instance.AddGold(currency.amount);
            UnityLogger.Log($"골드 추가 : {currency.amount}");
        }

        private void HandleEquipment(EquipmentItemSO equipment)
        {
            PlayerManager.Instance.equipmentInventory.Add(equipment);
            UnityLogger.Log("인벤토리 장비 추가");
        }
    }
}
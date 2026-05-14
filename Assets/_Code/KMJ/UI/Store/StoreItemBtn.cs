using Code.Core.Events.Bus;
using Code.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class StoreItemBtn : MonoBehaviour
    {
        [SerializeField] private Button itemButton;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private Image itemImage;
        
        [SerializeField] private TextMeshProUGUI goldTxt;
        
        private ItemSO item;

        private void Awake()
        {
            itemButton.onClick.AddListener(HandleItemButton);
        }

        private void OnDisable()
        {
            itemButton.onClick.RemoveListener(HandleItemButton);
        }

        public void SetItem(ItemSO newItem, TextMeshProUGUI goldTxt)
        {
            item = newItem;

            itemNameText.text = item.itemName;
            itemImage.sprite = item.itemIcon;
            this.goldTxt = goldTxt;
        }

        private void HandleItemButton()
        {
            Bus<SetChoiceUIEvent>.Raise(new SetChoiceUIEvent(null,item,goldTxt,this.gameObject));
        }
        
    }
}
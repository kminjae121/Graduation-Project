using Code.Core.Managers;
using Code.Core.Events.Bus;
using Code.Items;
using Code.Managers;
using Code.SkillSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class SkillChoiceUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI buyTxt;
        [SerializeField] private Button selectBtn;
        [SerializeField] private Button cancelBtn;
        [SerializeField] private GameObject choiceObj;
        
        private SkillSO _skil;
        private ItemSO _item;
        private TextMeshProUGUI _goldTxt;
        private GameObject _storePanel;

        private void Awake()
        {
            Bus<SetChoiceUIEvent>.Subscribe(SetText);
            selectBtn.onClick.AddListener(SelectSkill);
            cancelBtn.onClick.AddListener(CancelSkill);
        }

        private void OnDisable()
        {
            Bus<SetChoiceUIEvent>.Unsubscribe(SetText);
            selectBtn.onClick.RemoveListener(SelectSkill);
            cancelBtn.onClick.RemoveListener(CancelSkill);
        }

        public void SetText(SetChoiceUIEvent setEvent)
        {
            if (setEvent.itemSO != null)
            {
                _item = setEvent.itemSO;
                buyTxt.text = $"{_item.itemName}를 구매하시겠습니까?";
            }
            else if(setEvent.skillSO != null)
            {
                _skil = setEvent.skillSO;
                buyTxt.text = $"{_skil.skillName}를 구매하시겠습니까?";
            }
            
            _goldTxt = setEvent.goldTxt;
            _storePanel = setEvent.storePanel;
            
            choiceObj.SetActive(true);
        }
        private void HandleCurrency(CurrencyItemSO currency)
        {
            PlayerManager.Instance.AddGold(currency.amount);
        }

        private void HandleEquipment(EquipmentItemSO equipment)
        {
            PlayerManager.Instance.equipmentInventory.Add(equipment);
        }

        public void SelectSkill()
        {
            if (_skil != null)
            {
                if (_skil.skillPrice > PlayerManager.Instance.Gold)
                    return;
            
                PlayerManager.Instance.RemoveGold(_skil.skillPrice);
            
                GoodsManager.Instance.GetSkill(_skil);
            
                _goldTxt.text = $"골드 : {PlayerManager.Instance.Gold.ToString()}";

                _skil = null;
                choiceObj.SetActive(false);
                _storePanel.SetActive(false);
            }
            else if(_item != null)
            {
                switch (_item)
                {
                    case CurrencyItemSO currency:
                        HandleCurrency(currency);
                        break;
                    case EquipmentItemSO equipment:
                        HandleEquipment(equipment);
                        break;
                }
            
                _goldTxt.text = $"골드 : {PlayerManager.Instance.Gold.ToString()}";

                _item = null;
                choiceObj.SetActive(false);
                _storePanel.SetActive(false);
            }
        }
        

        public void CancelSkill() => choiceObj.SetActive(false);
    }
}

using System;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using GameEventChannel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UnitUpgradeUI : MonoBehaviour
    {
        private UnitInGameSO unitInfoSO;

        [SerializeField] private Image unitImage;
        [SerializeField] private Button unitHealthUpgradeButton;
        [SerializeField] private Button unitDamageUpgradeButton;
        [SerializeField] private Button unitSkillDamageUpgradeBtn;


        [SerializeField] private TextMeshProUGUI unitHealthTxt;
        [SerializeField] private TextMeshProUGUI unitDamageTxt;
        [SerializeField] private TextMeshProUGUI unitSkillTxt;

        private void Awake()
        {
            Bus<SendUnitInfoEvent>.Subscribe(SetUnitSO);
        }

        private void OnDestroy()
        {
            Bus<SendUnitInfoEvent>.Unsubscribe(SetUnitSO);
        }

        public void SetUnitSO(SendUnitInfoEvent unit)
        {
            unitInfoSO = unit.unitState.Data.unitInGame;

            unitImage.sprite = unit.unitState.Data.UnitImage;
            
            SetUnitBtns();

            SetTexts();
        }



        public void MaxHealthUpgrade()
        {
            
            if (unitDamageUpgradeButton == null)
                return;

            if (unitInfoSO == null)
                return;

            unitInfoSO.Maxhealth += 10;
            SetTexts();
        }

        private void SkillDamageUpgrade()
        {
            if (unitSkillDamageUpgradeBtn == null)
                return;

            if (unitInfoSO == null)
                return;
            
            unitInfoSO.SkillDamage += 10;
            SetTexts();
        }

        private void DamageUpgrade()
        {
            if (unitDamageUpgradeButton == null)
                return;

            if (unitInfoSO == null)
                return;


            unitInfoSO.AtkDamage += 10;
            SetTexts();
        }
        
        private void SetUnitBtns()
        {
            unitHealthUpgradeButton.onClick.RemoveAllListeners();
            unitDamageUpgradeButton.onClick.RemoveAllListeners();
            unitSkillDamageUpgradeBtn.onClick.RemoveAllListeners();
            
            unitHealthUpgradeButton.onClick.AddListener(MaxHealthUpgrade);
            unitDamageUpgradeButton.onClick.AddListener(DamageUpgrade);
            unitSkillDamageUpgradeBtn.onClick.AddListener(SkillDamageUpgrade);
        }
        
        private void SetTexts()
        {
            unitHealthTxt.text = $"체력 : {unitInfoSO.Maxhealth.ToString()}";
            unitDamageTxt.text = $"공격력 {unitInfoSO.AtkDamage.ToString()}";
            unitSkillTxt.text = $"스킬 공격력 : {unitInfoSO.SkillDamage.ToString()}";
        }
    }
}

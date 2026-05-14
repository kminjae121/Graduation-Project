using Code.Items;
using Code.SkillSystem;
using TMPro;
using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct SetChoiceUIEvent : IEvent
    {
        public SkillSO skillSO;
        public ItemSO itemSO;
        public TextMeshProUGUI goldTxt;
        public GameObject storePanel;

        public SetChoiceUIEvent(SkillSO skillSo, ItemSO itemSo, TextMeshProUGUI goldTxt,GameObject storePanel)
        {
            this.goldTxt = goldTxt;
            this.itemSO = itemSo;
            this.skillSO = skillSo;
            this.storePanel = storePanel;
        }
    }
}
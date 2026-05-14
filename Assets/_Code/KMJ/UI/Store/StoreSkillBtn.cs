using Code.Core.Events.Bus;
using Code.SkillSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class StoreSkillBtn : MonoBehaviour
    {
        [SerializeField] private Image skillImg;
        [SerializeField] private TextMeshProUGUI skillDec;
        [SerializeField] private TextMeshProUGUI skillUnit;

        [SerializeField] private Button skillBtn;

        private TextMeshProUGUI goldTxt;
        private SkillSO skillso = null;


        private void Awake()
        {
            skillBtn.onClick.AddListener(HandleSkillPressed);   
        }

        private void OnDisable()
        {
            skillBtn.onClick.RemoveListener(HandleSkillPressed);    
        }


        public void SetSkill(SkillSO skill, TextMeshProUGUI goldTxt)
        {
            skillImg.sprite = skill.skillUIImage;
            skillUnit.text = skill.unitType.ToString();
            skillDec.text = skill.SkillDescription;

            this.goldTxt = goldTxt;
            skillso = skill;
        }


        private void HandleSkillPressed()
        {
            Bus<SetChoiceUIEvent>.Raise(new SetChoiceUIEvent(skillso,null,goldTxt,this.gameObject));
        }
    }
}
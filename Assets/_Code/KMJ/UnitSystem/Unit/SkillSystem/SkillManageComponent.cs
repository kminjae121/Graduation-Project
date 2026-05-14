using System;
using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.SkillSystem
{
    public class SkillManageComponent : MonoBehaviour
    {
        private BasicUnitSkill usingSkill;

        private void OnEnable()
        {
            Bus<SendSkillEvent>.Subscribe(SetSkillSO);
        }

        private void OnDestroy()
        {
            Bus<SendSkillEvent>.Unsubscribe(SetSkillSO);
        }
        
        public void SetSkillSO(SendSkillEvent skillSo)
        {
            usingSkill = skillSo.skill;
        }

        public BasicUnitSkill GetSkillInfo()
        {
            return usingSkill;
        }
    }
}
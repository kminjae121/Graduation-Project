using System.Collections.Generic;
using _Code.KMJ.UnitSystem.involveUnitSO;
using Code.Core;
using Code.Core.Managers;
using Code.Items;
using Code.SkillSystem;

namespace Code.Core.Managers
{
    public class GoodsManager : MonoSingleton<GoodsManager>
    {
        public HavingSkillSO havingSkillSO;
        public List<SkillSO> skills;
        
        public void AddSkill()
        {
            skills.ForEach(skill =>
            {
                havingSkillSO.HaveSkills.Add(skill);
                SkillSendManager.Instance.AddSkillList(skill);
            });

            skills.Clear();
        }

        public void GetSkill(SkillSO skill)
        {
            skills.Add(skill);
        }
    }
}
using System.Collections.Generic;
using Code.SkillSystem;
using UnityEngine;

namespace _Code.KMJ.UnitSystem.involveUnitSO
{
    [CreateAssetMenu(fileName = "HavingSkillSO", menuName = "SO/UnitSO/Skill/HavingSkill", order = 0)]
    public class HavingSkillSO : ScriptableObject
    {
        public List<SkillSO> HaveSkills = new List<SkillSO>();
    }
}
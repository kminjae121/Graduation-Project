using UnityEngine;

namespace Code.SkillSystem
{
    [CreateAssetMenu(fileName = "SkillSO", menuName = "SO/UnitSO/Skill/UnitSkill", order = 0)]
    public class SkillSO : ScriptableObject
    {
        [Header("Basic Info")]
        public UnitType unitType = UnitType.None;
        public string skillName;
        public string skillAnimationKey;
        
        [TextArea(3, 15)]
        public string SkillDescription;
        public int SkillValue;
        public int SkillCost;
        public Sprite skillUIImage;
        public string className;
        public int skillPrice;

        [Header("Detail Info")]
        public int SkillDamage;
        public int SkillRange;
        public int MinRange;
        public bool IsOwnSkill;
        public SkillType SkillType;
    }
}
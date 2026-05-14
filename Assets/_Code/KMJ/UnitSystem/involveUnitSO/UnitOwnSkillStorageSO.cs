using System.Collections.Generic;
using Code.SkillSystem;
using UnityEngine;

namespace _Code.KMJ.UnitSystem.involveUnitSO
{
    [CreateAssetMenu(fileName = "UnitSO", menuName = "SO/UnitSO/Skill/SkillOwnStorage")]
    public class UnitOwnSkillStorageSO : ScriptableObject
    {
        public UnitType uniType = UnitType.None;
        public List<SkillSO> skills = null;
    }
}
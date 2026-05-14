using System;
using System.Collections.Generic;
using System.Linq;
using Code.SkillSystem;
using UnityEngine;

namespace _Code.KMJ.UnitSystem.involveUnitSO
{
    [CreateAssetMenu(fileName = "UnitSO", menuName = "SO/UnitSO/Skill/UnitSKillStorage")]
    public class UnitSkillStorageSO : ScriptableObject
    {
        public UnitType uniType = UnitType.None;
        public List<SkillSO> skills = null;
    }
}
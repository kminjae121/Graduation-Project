using System.Collections.Generic;
using UnityEngine;

namespace Code.Combat.StatusEffect
{
    [CreateAssetMenu(fileName = "StatusEffectStorage", menuName = "SO/StatusEffect/Storage", order = 0)]
    public class StatusEffectStorageSO : ScriptableObject
    {
        public List<StatusEffectSO> statusEffects;
    }
}
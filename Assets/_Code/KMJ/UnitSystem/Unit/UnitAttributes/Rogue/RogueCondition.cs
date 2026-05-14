using System.Collections.Generic;
using Code.UI;
using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public class RogueCondition : MonoBehaviour, IUnitCondition
    {
        [SerializeField] private RogueShadowSpawn rogueShadowCompo;
        public void Initialize(Unit unit)
        {
        }

        public bool CheckCondition(GameObject target)
        {
            if (rogueShadowCompo.GetMaxShadowCnt() <= rogueShadowCompo.GetShadowCnt())
            {
                return true;
            }
            return false;
        }
    }
}
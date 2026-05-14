using System.Collections.Generic;
using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public class ArcherCondition : MonoBehaviour, IUnitCondition
    {
        [SerializeField] private int currentValue = 1;
        [SerializeField] private int endValue = 8;

        public void Initialize(Unit unit)
        {
        }

        public bool CheckCondition(GameObject target)
        {
            if (target == null) return false;

            currentValue += 1;

            if (currentValue >= endValue)
            {
                return true;
            }
            
            return false;
        }
    }
}
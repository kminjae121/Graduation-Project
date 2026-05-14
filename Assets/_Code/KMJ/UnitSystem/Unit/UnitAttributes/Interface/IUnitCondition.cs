using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public interface IUnitCondition
    {
        public void Initialize(Unit unit);
        public bool CheckCondition(GameObject unit);
    }
}
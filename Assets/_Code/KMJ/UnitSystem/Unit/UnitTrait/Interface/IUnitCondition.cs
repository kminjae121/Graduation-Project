using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public interface IUnitCondition
    {
        public void Initialize(Unit unit);
        public bool CheckCondition(GameObject unit);
    }
}
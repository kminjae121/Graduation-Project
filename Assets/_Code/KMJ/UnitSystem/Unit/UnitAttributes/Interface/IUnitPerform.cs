using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public interface IUnitPerform
    {
        public void Initialize(Unit unit);
        public void Perform(GameObject target);
    }
}
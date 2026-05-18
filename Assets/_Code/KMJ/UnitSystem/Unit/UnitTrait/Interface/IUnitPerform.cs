using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public interface IUnitPerform
    {
        public void Initialize(Unit unit);
        public void Perform(GameObject target);
    }
}
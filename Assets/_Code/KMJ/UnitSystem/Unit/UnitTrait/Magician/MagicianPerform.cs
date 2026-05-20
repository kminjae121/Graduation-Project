using Code.Core.Events.Bus;
using Code.UnitSystem.Combat;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class MagicianPerform : MonoBehaviour, IUnitPerform
    {
        private Unit _unit;
        [SerializeField] private MagicianCondition condition;
        
        public void Initialize(Unit unit)
        {
            _unit = unit;
        }

        public void Perform(GameObject target)
        {
            
        }
    }
}
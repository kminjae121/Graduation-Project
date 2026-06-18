using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class KnightCondition : MonoBehaviour, IUnitCondition
    {
        private Unit _unit;
        private int _stack = 0;
        [SerializeField] private int _maxStack = 4;

        public void Initialize(Unit unit)
        {
            _unit = unit;
        }
        
        private void SetStack()
        {
            _stack += 1;
        }

        public void ResetStack()
        {
            _stack = 0;
        }

        public bool CheckCondition(GameObject unit)
        {
            SetStack();
            
            Bus<KnightSpecEvent>.Raise(new KnightSpecEvent(_stack));
            
            if (_stack >= _maxStack)
            {
                return true;
            }
            
            return false;
        }
    }
}
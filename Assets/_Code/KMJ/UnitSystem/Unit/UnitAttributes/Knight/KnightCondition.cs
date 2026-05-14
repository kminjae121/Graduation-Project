using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public class KnightCondition : MonoBehaviour, IUnitCondition
    {
        private Unit _unit;
        private int _stack = 0;
        [SerializeField] private int _maxStack = 8;

        public void Initialize(Unit unit)
        {
            _unit = unit;
        }
        
        private void SetStack()
        {
            _stack += 1;
        }

        public bool CheckCondition(GameObject unit)
        {
            SetStack();

            Bus<KnightGimicBarEvent>.Raise(new KnightGimicBarEvent(_stack));
            
            if (_stack >= _maxStack)
            {
                _stack = 0;
                Bus<KnightGimicBarEvent>.Raise(new KnightGimicBarEvent(0));
                return true;
            }
            
            return false;
        }
    }
}
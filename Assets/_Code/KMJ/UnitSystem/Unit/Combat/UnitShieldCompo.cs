using Code.UnitSystem;
using UnityEngine;
using UnityEngine.Events;

namespace _Code.Combat
{
    public class UnitShieldCompo : MonoBehaviour, IUnitComponent
    {
        private Unit _unit;
        private UnitSO _unitInfoSO;
        
        private int _maxShieldValue = 0;
        private int _currentShieldValue = 0;

        public UnityEvent BreakShieldEvent;
        public UnityEvent RecoverShieldEvent;
        
        public void Initialize(Unit owner)
        {
            _unit = owner;

            _unitInfoSO = _unit.unitSO;

            _maxShieldValue = _unitInfoSO.ShieldValue = _currentShieldValue;
        }

        public int GetShieldValue()
            => _currentShieldValue;

        public void BreakShield(int value)
        {
            if(_currentShieldValue > 0)
                _currentShieldValue -= value;

            if (_currentShieldValue < 0)
                _currentShieldValue = 0;
            
            BreakShieldEvent?.Invoke();
        }

        public void RecoverShield(int value)
        {
            _currentShieldValue += value;
            
            if(_currentShieldValue > _maxShieldValue)
                _currentShieldValue = _maxShieldValue;
            
            RecoverShieldEvent?.Invoke();
        }
    }
}
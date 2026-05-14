using UnityEngine;


namespace Code.UnitSystem.UnitAttributes
{
    public enum MagicianType
    {
        Attack,
        Heal,
    }
    
    public class MagicianCondition: MonoBehaviour, IUnitCondition
    {
        private Unit _unit;

        private int _atkGauge;
        private int _healGauge;

        private int _maxGauge = 5;

        public MagicianType MagicianType { get; private set; }
        
        public void Initialize(Unit unit)
        {
            _unit = unit;
            _atkGauge = 0;
            _healGauge = 0;
        }

        public bool CheckCondition(GameObject unit)
        {
            Unit unitType = unit.GetComponentInParent<Unit>();
            
            if (unitType as CharacterUnit)
            {
                _healGauge += 1;
                if (_healGauge >= _maxGauge)
                {
                    _healGauge = 0;
                    MagicianType = MagicianType.Heal;
                    return true;
                }
            }
            else
            {
                _atkGauge += 1;
                if (_atkGauge >= _maxGauge)
                {
                    _atkGauge = 0;
                    MagicianType = MagicianType.Attack;    
                    return true;
                }
            }
            
            return false;
        }
    }
}
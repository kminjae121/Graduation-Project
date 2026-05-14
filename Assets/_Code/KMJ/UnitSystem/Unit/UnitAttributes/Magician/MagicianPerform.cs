using Code.Core.Events.Bus;
using Code.UnitSystem.Combat;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
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
            if (condition.MagicianType == MagicianType.Heal)
            {
                CharacterUnit[] units = FindObjectsOfType<CharacterUnit>();

                foreach (var unit in units)
                {
                    unit.GetUnitCompo<UnitHealth>().HealHp(10);
                }
            }
            else if(condition.MagicianType == MagicianType.Attack)
            {
                AbstractEnemyUnit[] units = FindObjectsOfType<AbstractEnemyUnit>();

                foreach (var unit in units)
                {
                    DamageData data = new DamageData();

                    data.damage = 10;
            
                    Bus<DamageEvent>.Raise(new DamageEvent(data,unit.gameObject,0, _unit,false,false,0.3f));
                }
            }
        }
    }
}
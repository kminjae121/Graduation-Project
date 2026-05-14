using Code.UnitSystem.Combat;
using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public class KnightPerform : MonoBehaviour , IUnitPerform
    {
        [SerializeField] private ParticleSystem circleParticle;
        [SerializeField] private KnightDefenseRange defenseCompo;
        private Unit _unit;
        private UnitHealth _healthCompo;
        
        public void Initialize(Unit unit)
        {
            _unit = unit;
            _healthCompo = _unit.GetUnitCompo<UnitHealth>();
        }

        public void Perform(GameObject target)
        {
            foreach (var unit in defenseCompo.Targets)
            {
                unit.GetUnitCompo<InvincibilityCompo>().SetUnitInvincibility(2);
                unit.GetUnitCompo<UnitHealth>().HealHp(20);
            }
            
            _unit.GetUnitCompo<InvincibilityCompo>().SetUnitInvincibility(1);

            _healthCompo.SetMaxHp(_unit.unitSO.Maxhealth);
            circleParticle.Play();
        }
    }
}
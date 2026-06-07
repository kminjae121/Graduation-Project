using Code.Core.Events.Bus;
using Code.UnitSystem.Combat;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class KnightPerform : MonoBehaviour , IUnitPerform
    {
        [SerializeField] private KnightCondition condition;
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
            condition.ResetStack();
            Bus<KnightGimicBarEvent>.Raise(new KnightGimicBarEvent(0));
            
            foreach (var unit in defenseCompo.Targets)
            {
                unit.GetUnitCompo<InvincibilityCompo>().SetUnitInvincibility(2);
                unit.GetUnitCompo<UnitHealth>().HealHp(20);
            }
            
            _unit.GetUnitCompo<InvincibilityCompo>().SetUnitInvincibility(4);

            _healthCompo.ResetMaxHp();
                 
            circleParticle.Play();
        }
    }
}
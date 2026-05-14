using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using UnityEngine;

namespace _Code.Passive
{
    public class DamageIncreasePassive : MyTurnPassive
    {
        private CharacterUnit _character; 
        private void Start()
        {
            _character = _unit as CharacterUnit;
        }


        public override void StartPassive()
        {
            AttackApplyCompo.Instance.AttackStartEvent += ReduceDamage;
        }

        public override void StopPassive()
        {
            AttackApplyCompo.Instance.AttackStartEvent -= ReduceDamage;
        }

        private void ReduceDamage(ref DamageEvent evt, ref bool isCritical, ref bool isPenetrate)
        {
            UnitHealth health = evt.target.GetComponent<UnitHealth>();
            
            if (health.CurrentHealth <= health.MaxHealth * 0.3f)
            {
                evt.DamageData.damage = (int)(evt.DamageData.damage * 1.5f);
            }
        }
    }
}
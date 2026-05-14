using System;
using _Code.Combat;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using UnityEngine;

namespace _Code.Passive
{
    public class DamageReducePassive : MyTurnPassive
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
            //if (_character.HealthCompo.CurrentHealth >= _character.HealthCompo.MaxHealth * 0.8f)
            //{
            //    int damage = evt.DamageData.damage;
            //
            //    int reduceDamage = (int)(damage - (damage * 0.2f));
            //
            //    evt.DamageData.damage = reduceDamage;
            //}
        }
    }
}
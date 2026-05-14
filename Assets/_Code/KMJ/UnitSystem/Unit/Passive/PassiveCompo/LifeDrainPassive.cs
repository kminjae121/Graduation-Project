using System;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace _Code.Passive
{
    public class LifeDrainPassive : MyTurnPassive
    {
        private CharacterUnit _character; 
        private void Start()
        {
            _character = _unit as CharacterUnit;
        }

        public override void StartPassive()
        {
            AttackApplyCompo.Instance.AttackEndEvent.AddListener(LifeDrain);
        }

        public override void StopPassive()
        {
            AttackApplyCompo.Instance.AttackEndEvent.RemoveListener(LifeDrain);
        }

        private void LifeDrain(Vector3 pos)
        {
            float lostHp = _character.HealthCompo.MaxHealth - _character.HealthCompo.CurrentHealth;

            int healHp = Mathf.FloorToInt(lostHp * 0.15f);
            _character.HealthCompo.HealHp(healHp);
        }
    }
}
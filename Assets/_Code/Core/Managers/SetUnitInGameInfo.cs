using System;
using System.Collections.Generic;
using Code.UnitSystem;
using UnityEngine;

namespace Code.Core.Managers
{
    public class SetUnitInGameInfo : MonoBehaviour
    {
        [SerializeField] private List<UnitSO> _units;

        private void Start()
        {
            _units.ForEach(unit =>
            {
                unit.unitInGame.AtkDamage = unit.AttackDamage;
                unit.unitInGame.DefensivePower = unit.DefensivePower;
                unit.unitInGame.Maxhealth =  unit.Maxhealth;
                unit.unitInGame.SkillDamage = unit.AttackDamage; 
            });
        }
    }
}
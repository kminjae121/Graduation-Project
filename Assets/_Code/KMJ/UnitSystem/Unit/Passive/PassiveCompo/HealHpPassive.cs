using Code.Managers;
using Code.UnitSystem;
using UnityEngine;

namespace _Code.Passive
{
    public class HealHpPassive : AlwaysTurnPassive
    {
        private CharacterUnit _character; 
        
        protected override void Start()
        {
            base.Start();
            _character = _unit as CharacterUnit;
        }
        
        public override void StartPassive()
        {
            _turnManager.OnTurnStart += HealHp;
        }

        public override void StopPassive()
        {
            _turnManager.OnTurnStart -= HealHp;
        }

        private void HealHp()
        {
            float lostHp = _character.HealthCompo.MaxHealth - _character.HealthCompo.CurrentHealth;

            int healHp = Mathf.FloorToInt(lostHp * 0.1f);
            _character.HealthCompo.HealHp(healHp);
        }
    }
}
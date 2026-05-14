using Code.UnitSystem;
using UnityEngine;

namespace Code.UnitSystem.Combat
{
    public class ActionData : MonoBehaviour, IUnitComponent
    {
        public Vector3 HitPoint { get; set; }
        public Vector3 HitNormal { get; set; }
        public bool HitByPowerAttack { get; set; }
        public DamageData LastDamageData { get; set; }

        private Unit _entity;

        public void Initialize(Unit owner)
        {
            _entity = owner;    
        }
    }
}
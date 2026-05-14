using Code.UnitSystem;
using Code.UnitSystem.Combat;
using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct DamageEvent : IEvent
    {
        public DamageData DamageData;
        public GameObject target;
        public float addDamage;

        public Unit Owner;

        public bool IsCritical;

        public bool IsPenetrate;

        public float ShakeValue;
        public DamageEvent(DamageData data, GameObject target, float addDamage, Unit Owenr,  bool IsCritical, bool IsPenetrate,float shakeValue)
        {
            DamageData = data;
            this.target = target;
            this.addDamage = addDamage;
            this.Owner = Owenr;
            this.IsCritical = IsCritical;
            this.IsPenetrate = IsPenetrate;
            this.ShakeValue =  shakeValue;
        }
    }
}
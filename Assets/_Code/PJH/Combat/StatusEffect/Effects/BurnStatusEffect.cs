using Code.UnitSystem.Combat;
using UnityEngine;

namespace Code.Combat.StatusEffect
{
    public class BurnStatusEffect : StatusEffect
    {
        public override void ApplyEffect()
        {
            if (!(Value > 0))
                return;
            
            UnitHealth healthCompo = _target.GetUnitCompo<UnitHealth>();

            if (healthCompo == null)
                return;
            
            var damageData = new DamageData
            {
                damage = (int)Value,
                damageType = DamageType.MAGIC,
                isCritical = false
            };

            healthCompo.ApplyDamage(damageData, _target.transform.position,
                Vector3.zero, null, false,
                false);
        }

        public override void EndUpdateEffect()
        {
            base.EndUpdateEffect();
            
            ApplyEffect();
        }

        public override void Merge(StatusEffectApplyData data)
        {
            // 합
            int? mergedDuration = Duration + data.Duration;

            if (Value == null || data.Value == null)
                return;
            
            // Max
            int? mergedValue = Mathf.Max((int)Value, (int)data.Value);
            
            SetEffect(_target, new StatusEffectApplyData(mergedDuration, mergedValue));
        }
    }
}
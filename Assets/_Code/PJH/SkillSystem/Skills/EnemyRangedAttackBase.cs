using Code.Combat.StatusEffect;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyRangedAttackBase : EnemyAttackBaseSkill
    {
        protected override void Attack(GameObject target)
        {
            if (target == null)
                return;

            Owner.VFXCompo.PlayVFX("Fireball", Owner.transform.position, Quaternion.identity);
            SkillFeedbackEvent?.Invoke();
            Bus<DamageEvent>.Raise(new DamageEvent(DamageData, target, AddDamage, null, false, false, 0.1f));
            Bus<ApplyStatusEffectEvent>.Raise(new ApplyStatusEffectEvent(target.GetComponent<Unit>(), EffectType.Burn, new StatusEffectApplyData(3, 5)));
        }
    }
}

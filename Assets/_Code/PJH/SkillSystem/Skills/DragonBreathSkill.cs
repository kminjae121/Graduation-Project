using Code.Combat.StatusEffect;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using UnityEngine;

namespace Code.SkillSystem
{
    public class DragonBreathSkill : EnemyLineBaseSkill
    {
        [SerializeField] private int burnDuration = 2;
        [SerializeField] private int burnDamage = 5;

        protected override void Attack(GameObject target)
        {
            if (target == null)
                return;

            foreach (var hitTarget in GetLineTargets(target))
            {
                Bus<DamageEvent>.Raise(new DamageEvent(DamageData, hitTarget, AddDamage,
                    null, false, false, 0.1f));

                ApplyBurn(hitTarget);
            }
        }

        private void ApplyBurn(GameObject target)
        {
            if (burnDuration <= 0 || burnDamage <= 0)
                return;

            if (!target.TryGetComponent(out Unit targetUnit))
                return;

            Bus<ApplyStatusEffectEvent>.Raise(new ApplyStatusEffectEvent(targetUnit, EffectType.Burn,
                new StatusEffectApplyData(burnDuration, burnDamage)));
        }
    }
}
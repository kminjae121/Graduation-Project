using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyMeleeAttackBase : EnemyAttackBaseSkill
    {
        protected override void Attack(GameObject target)
        {
            if (target == null)
                return;

            Bus<DamageEvent>.Raise(new DamageEvent(DamageData, target, AddDamage, null, false, false, 0.1f));
        }
    }
}
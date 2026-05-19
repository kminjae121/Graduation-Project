using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyRangedAttack : EnemyBaseSkill
    {
        protected override void OnAttack(GameObject target)
        {
            if (target == null)
                return;

            SkillFeedbackEvent?.Invoke();
            Bus<DamageEvent>.Raise(new DamageEvent(DamageData, target, AddDamage, null, false, false, 0.1f));
        }
    }
}
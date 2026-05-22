using UnityEngine;

namespace Code.SkillSystem
{
    public abstract class EnemyAttackBaseSkill : EnemyBaseSkill
    {
        protected override bool UseAttackEvent => true;

        protected override void OnAttack(GameObject target)
        {
            Attack(target);
        }

        protected abstract void Attack(GameObject target);
    }
}
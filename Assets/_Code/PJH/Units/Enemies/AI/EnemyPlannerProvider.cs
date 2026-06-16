using Code.SkillSystem;
using Code.UnitSystem;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public abstract class EnemyPlannerProvider : MonoBehaviour
    {
        public abstract EnemyPlannerBase Planner { get; }

        public virtual void OnSkillStarted(SkillSO skillSO, Unit target)
        {
        }

        public virtual void OnSkillFinished(SkillSO skillSO, Unit target)
        {
        }
    }
}

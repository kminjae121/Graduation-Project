using Code.SkillSystem;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public sealed class EnemyPlan
    {
        public Unit Target { get; private set; }
        public SkillSO SelectedSkill { get; private set; }
        public Vector2Int MoveTile { get; private set; }
        public bool HasMoveTile { get; private set; }

        public bool CanAttackImmediately => Target != null && SelectedSkill != null;

        public void SetTarget(Unit target)
            => Target = target;

        public void SetSkill(SkillSO skillSO)
            => SelectedSkill = skillSO;

        public void ClearCombatDecision()
        {
            Target = null;
            SelectedSkill = null;
        }

        public void SetCombatDecision(Unit target, SkillSO skillSO)
        {
            Target = target;
            SelectedSkill = skillSO;
        }

        public void SetMoveTile(Vector2Int moveTile)
        {
            MoveTile = moveTile;
            HasMoveTile = true;
        }

        public void ClearMoveTile()
        {
            MoveTile = default;
            HasMoveTile = false;
        }

        public void Clear()
        {
            ClearCombatDecision();
            ClearMoveTile();
        }
    }
}

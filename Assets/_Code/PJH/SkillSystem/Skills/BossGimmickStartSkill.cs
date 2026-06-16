using Code.UnitSystem.Enemies.AI;
using UnityEngine;

namespace Code.SkillSystem
{
    public class BossGimmickStartSkill : EnemyActiveBaseSkill
    {
        public override bool CanUseAt(Vector2Int from, GameObject target)
            => target != null && SkillSO != null;

        public override float ScoreAt(Vector2Int from, GameObject target, EnemyAIProfileSO ai)
        {
            if (!CanUseAt(from, target))
                return float.MinValue;

            if (ai == null)
                return AIPriority * 10f;

            return AIPriority * ai.PrioWeight;
        }

        public override float PosScore(Vector2Int from, GameObject target)
            => 0f;

        protected override void OnSkillStarted()
        {
            SkillFeedbackEvent?.Invoke();
        }
    }
}

using Code.Core.Events.Bus;
using Code.UnitSystem.Enemies.AI;
using UnityEngine;

namespace Code.SkillSystem
{
    public class GolemPunishAllAttackSkill : EnemyAttackBaseSkill
    {
        [SerializeField] private float shakeValue = 0.4f;
        [SerializeField] private string vfxName;

        public override bool CanUseAt(Vector2Int from, GameObject target)
            => target != null && SkillSO != null;

        public override float ScoreAt(Vector2Int from, GameObject target, EnemyAIProfileSO ai)
        {
            if (!CanUseAt(from, target))
                return float.MinValue;

            float power = SkillSO.SkillDamage;

            if (ai == null)
                return power + AIPriority * 10f;

            return power * ai.DmgWeight + AIPriority * ai.PrioWeight;
        }

        public override float PosScore(Vector2Int from, GameObject target)
            => 0f;

        protected override void Attack(GameObject target)
        {
            if (UnitManager == null)
                return;

            SkillFeedbackEvent?.Invoke();

            if (!string.IsNullOrWhiteSpace(vfxName) && Owner?.VFXCompo != null)
                Owner.VFXCompo.PlayVFX(vfxName);

            foreach (var unit in UnitManager.GetPlayerUnits())
            {
                if (unit == null || !unit.gameObject.activeInHierarchy)
                    continue;

                Bus<DamageEvent>.Raise(new DamageEvent(DamageData, unit.gameObject, AddDamage,
                    Owner, false, false, shakeValue));
            }
        }
    }
}

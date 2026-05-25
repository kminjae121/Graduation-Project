using Code.Core.Debugs;
using Code.Core.Events.Bus;
using Code.Map;
using Code.UnitSystem;
using Code.UnitSystem.TraitSystem;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyShieldSkill : EnemyActiveBaseSkill
    {
        [SerializeField] private int guardTurns = 2;
        [SerializeField, Range(0f, 1f)] private float frontDamageRate;
        [SerializeField] private float frontAngle = 120f;
        [SerializeField] private int adjacentRange = 1;

        protected override void OnSkillStarted()
        {
            ApplyFrontGuard();
            DamageAdjacentTargets();
            SkillFeedbackEvent?.Invoke();
            //Owner.VFXCompo.PlayVFX();
        }

        private void ApplyFrontGuard()
        {
            if (Owner == null)
                return;

            var invincibility = Owner.GetUnitCompo<InvincibilityCompo>();

            if (invincibility == null)
            {
                UnityLogger.LogWarning($"[{nameof(EnemyShieldSkill)}] InvincibilityCompo is missing.");
                return;
            }

            invincibility.SetFrontGuard(guardTurns, Owner.transform, frontDamageRate, frontAngle);
        }

        private void DamageAdjacentTargets()
        {
            if (Owner == null || UnitManager == null)
                return;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
            {
                UnityLogger.LogError($"[{nameof(EnemyShieldSkill)}] GridMap is missing.");
                return;
            }

            Vector2Int origin = gridMap.WorldToGridPos(Owner.transform.position);
            int range = Mathf.Max(1, adjacentRange);

            foreach (var unit in UnitManager.GetAllUnits())
            {
                if (unit == null || unit == Owner || unit.IsPlayerUnit == Owner.IsPlayerUnit)
                    continue;

                Vector2Int targetPos = gridMap.WorldToGridPos(unit.transform.position);
                int distance = Mathf.Abs(targetPos.x - origin.x) + Mathf.Abs(targetPos.y - origin.y);

                if (distance <= 0 || distance > range)
                    continue;

                Bus<DamageEvent>.Raise(new DamageEvent(DamageData, unit.gameObject, AddDamage,
                    Owner, false, false, 0.1f));
            }
        }
    }
}

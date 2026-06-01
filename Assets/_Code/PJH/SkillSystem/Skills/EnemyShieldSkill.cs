using Code.Core.Debugs;
using Code.Core.Events.Bus;
using Code.Map;
using Code.UnitSystem;
using Code.UnitSystem.Enemies.AI;
using Code.UnitSystem.TraitSystem;
using Code.Utils;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyShieldSkill : EnemyActiveBaseSkill
    {
        [SerializeField] private int guardTurns = 2;
        [SerializeField, Range(0f, 1f)] private float frontDamageRate;
        [SerializeField] private float frontAngle = 120f;
        [SerializeField] private float guardBaseScore = 20f;
        [SerializeField] private float frontThreatScore = 40f;
        [SerializeField] private float lowHealthScore = 100f;

        public override bool CanUseAt(Vector2Int from, GameObject target)
        {
            if (target == null || SkillSO == null || GridMap.Instance == null)
                return false;

            if (HasFrontGuard() || !IsCurrentTile(from))
                return false;

            return true;
        }

        public override float ScoreAt(Vector2Int from, GameObject target, EnemyAIProfileSO ai)
        {
            if (!CanUseAt(from, target))
                return float.MinValue;

            int frontThreatCount = CountFrontThreats(from);

            float score = guardBaseScore;
            score += frontThreatCount * frontThreatScore;
            score += MissingHealthRatio() * lowHealthScore;

            if (ai == null)
                return score + AIPriority * 10f;

            return score + AIPriority * ai.PrioWeight;
        }

        public override float PosScore(Vector2Int from, GameObject target)
            => 0f;

        protected override void OnSkillStarted()
        {
            ApplyFrontGuard();
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

            invincibility.SetFrontGuard(guardTurns, Owner.transform, frontDamageRate, frontAngle,
                CounterAttack);
        }

        private bool HasFrontGuard()
        {
            if (Owner == null)
                return false;

            var invincibility = Owner.GetUnitCompo<InvincibilityCompo>();
            return invincibility != null && invincibility.IsFrontGuard;
        }

        private void CounterAttack(Unit attacker)
        {
            if (Owner == null || UnitManager == null || attacker == null)
                return;

            var gridMap = GridMap.Instance;

            if (gridMap == null)
            {
                UnityLogger.LogError($"[{nameof(EnemyShieldSkill)}] GridMap is missing.");
                return;
            }

            Vector2Int originPos = gridMap.WorldToGridPos(Owner.transform.position);
            Vector2Int attackerPos = gridMap.WorldToGridPos(attacker.transform.position);
            Vector2Int hitPos = originPos + GetCounterDirection(originPos, attackerPos);

            foreach (var unit in UnitManager.GetAllUnits())
            {
                if (unit == null || unit == Owner || unit.IsPlayerUnit == Owner.IsPlayerUnit)
                    continue;

                Vector2Int targetPos = gridMap.WorldToGridPos(unit.transform.position);

                if (targetPos != hitPos)
                    continue;

                Bus<DamageEvent>.Raise(new DamageEvent(DamageData, unit.gameObject, AddDamage,
                    Owner, false, false, 0.1f));
            }
        }

        private bool IsCurrentTile(Vector2Int from)
        {
            if (Owner == null || GridMap.Instance == null)
                return false;

            return GridMap.Instance.WorldToGridPos(Owner.transform.position) == from;
        }

        private static Vector2Int GetCounterDirection(Vector2Int origin, Vector2Int attackerPos)
        {
            Vector2Int delta = attackerPos - origin;

            if (delta == Vector2Int.zero)
                return Vector2Int.up;

            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                return delta.x >= 0 ? Vector2Int.right : Vector2Int.left;

            return delta.y >= 0 ? Vector2Int.up : Vector2Int.down;
        }

        private int CountFrontThreats(Vector2Int from)
        {
            if (Owner == null || UnitManager == null || GridMap.Instance == null)
                return 0;

            int count = 0;
            Vector3 origin = GridMap.Instance.GridToWorldPos(from.x, from.y);
            Vector3 forward = Owner.transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude <= 0.001f)
                forward = Vector3.forward;

            forward.Normalize();
            float frontDot = Mathf.Cos(Mathf.Clamp(frontAngle, 0f, 180f) * 0.5f * Mathf.Deg2Rad);

            foreach (var unit in UnitManager.GetAllUnits())
            {
                if (!IsEnemyUnit(unit))
                    continue;

                Vector3 toTarget = unit.transform.position - origin;
                toTarget.y = 0f;

                if (toTarget.sqrMagnitude <= 0.001f)
                    continue;

                if (Vector3.Dot(forward, toTarget.normalized) >= frontDot)
                    ++count;
            }

            return count;
        }

        private float MissingHealthRatio()
        {
            if (Owner?.HealthCompo == null || Owner.HealthCompo.MaxHealth <= 0f)
                return 0f;

            return 1f - Mathf.Clamp01(Owner.HealthCompo.CurrentHealth / Owner.HealthCompo.MaxHealth);
        }

        private bool IsEnemyUnit(Unit unit)
            => unit != null && unit != Owner && unit.IsPlayerUnit != Owner.IsPlayerUnit;
    }
}

using Code.UnitSystem.Enemies;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyRangedSkill : EnemyAttackBaseSkill
    {
        [SerializeField] private PoolingItemSO projectilePrefab;
        [SerializeField] private Transform fireTrm;
        
        private PoolManagerMono _poolManager;
        
        protected override void Attack(GameObject target)
        {
            if (_poolManager == null)
                _poolManager = FindFirstObjectByType<PoolManagerMono>();
            
            var projectile = _poolManager.Pop<EnemyProjectile>(projectilePrefab);
            
            if (projectile == null)
                return;
            
            Vector3 firePos = fireTrm != null ? fireTrm.position : Owner.transform.position;
            Vector3 dir = target.transform.position - firePos;
            dir.y = 0f;
            
            if (dir.sqrMagnitude <= 0.001f)
                dir = Owner.transform.forward;
            
            projectile.transform.position = firePos;
            projectile.Initialize(Owner, target, DamageData, AddDamage);
            projectile.Launch(dir.normalized);
            
            SkillFeedbackEvent?.Invoke();
        }
    }
}
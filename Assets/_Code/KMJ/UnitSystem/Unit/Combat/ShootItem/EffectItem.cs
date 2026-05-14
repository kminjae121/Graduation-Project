using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.UnitSystem.Combat
{
    public class EffectItem : ShootItem
    {
        [Header("BombParticle")]
        [SerializeField] private ParticleSystem bombEffect;

        [Header("CastSetting")] 
        [SerializeField] private GameObject arrowPrfab;
        [SerializeField] private LayerMask whatIsEnemy;
        [SerializeField] private Transform bombTrm;
        [SerializeField] private Vector3 castSize;
        [SerializeField] private int damage;
        
        private DamageData _damageData;
        
        private void Start()
        {
            _damageData.damage = damage;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bombTrm.position, castSize);
            Gizmos.color = Color.red;
        }

        public override void AttackEnd()
        {
            Bus<DamageEvent>.Raise(new DamageEvent(_shootItemManager.DamageData,_target,0,_shootItemManager.Unit
                , false,false,0.2f));
            
            ParticleSystem particle = Instantiate(bombEffect, transform.position, Quaternion.identity);
            particle.Play();
            
            Collider[] cols = Physics.OverlapBox(bombTrm.position, castSize, Quaternion.identity, whatIsEnemy);

            foreach (var col in cols)
            {
                Bus<DamageEvent>.Raise(new DamageEvent(_damageData, col.gameObject,0 ,null, false,false,0.2f));
            }
            
            gameObject.SetActive(false); 
        }
    }
}
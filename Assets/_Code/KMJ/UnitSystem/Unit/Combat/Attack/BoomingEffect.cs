using System.Collections;
using Code.Core.Events.Bus;
using Code.UnitSystem.Combat;
using UnityEngine;

namespace Code.UnitSystem
{
    public class BoomingEffect : MonoBehaviour
    {
        [SerializeField] private LayerMask _whatIsEnemy;

        private DamageData _damageData;

        [SerializeField] private AttackDataSO atkData;

        private float _addDamage;
        private GameObject _target;

        [SerializeField] private ParticleSystem _particleSystem;

        private void Awake()
        {
            _damageData.damage = 4;
        }
        public void StartParticleEffect(Vector3 trm)
        {
            transform.position = trm;
            StartCoroutine(StartEffect());
        }

        public void SetDamageData(DamageData damageData,float addDamage,GameObject target)
        {
            _damageData = damageData;
            _addDamage = addDamage;
            _target =  target;
        }

        private IEnumerator StartEffect()
        {            
            _particleSystem.Play();
            yield return new WaitForSeconds(1.3f);
            
            Bus<DamageEvent>.Raise(new DamageEvent(_damageData,_target,_addDamage, null,false,false,0.3f));
        }
    }
}
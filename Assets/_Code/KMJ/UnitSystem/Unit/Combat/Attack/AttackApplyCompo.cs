using System;
using Code.Core;
using Code.Core.Events.Bus;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Code.UnitSystem.Combat
{
    public class AttackApplyCompo : MonoSingleton<AttackApplyCompo>
    {
        public delegate void AttackHandler(ref DamageEvent evt, ref bool isCritical, ref bool isPenetrate);
        public event AttackHandler AttackStartEvent;

        public UnityEvent<Vector3> AttackEndEvent;

        protected override void Awake()
        {
            isDontDestroyOnLoad = false;
            base.Awake();
        }

        private void Start()
        {
            Bus<DamageEvent>.Subscribe(GetApplyDamage);
            
            AttackStartEvent += CalculateCritical;
        }

        private void OnDestroy()
        {
            Bus<DamageEvent>.Unsubscribe(GetApplyDamage);
            AttackStartEvent -= CalculateCritical;
        }

        public void GetApplyDamage(DamageEvent evt)
        {
            Bus<TurnEndUIEvent>.Raise(new TurnEndUIEvent(false));

            if (evt.target != null && evt.target.TryGetComponent(out IDamageable damageable))
            {
                bool isCritical = evt.IsCritical;
                bool isPenetrate = evt.IsPenetrate;
                
                AttackStartEvent?.Invoke(ref evt, ref isCritical, ref isPenetrate);
                
                Bus<CamShakeEvent>.Raise(new CamShakeEvent(evt.ShakeValue));
                
                damageable.ApplyDamage(evt.DamageData, evt.target.transform.position, evt.target.transform.position
                    , evt.Owner, isCritical, isPenetrate);

                var anim = evt.target.GetComponentInChildren<UnitAnimation>();
                if (anim != null)
                {
                    Vector3 targetPos = anim.transform.position;
                    targetPos.y += 1f;
                    AttackEndEvent?.Invoke(targetPos);
                }
            }
        }
        
        private void CalculateCritical(ref DamageEvent evt, ref bool isCritical, ref bool isPenetrate)
        {
            if (isCritical)
                return;
            
            float damage = evt.DamageData.damage;
            float criticalProbilityValue = Random.Range(0f, 100f);

            if (evt.Owner != null && criticalProbilityValue <= evt.Owner.unitSO.CriticalProbability)
            {
                isCritical = true;
                damage *= (evt.Owner.unitSO.CriticalDamageIncrease / 100);
                evt.DamageData.damage = (int)damage;
            }
        }
    }
}
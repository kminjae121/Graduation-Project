using System;
using Code.Core;
using Code.Core.Events.Bus;
using Code.UnitSystem.TraitSystem;
using EnemySystem;
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

                EnemyMarking(evt);
                TargetPos(evt);
            }
        }

        private void TargetPos(DamageEvent evt)
        {
            var anim = evt.target.GetComponentInChildren<UnitAnimation>();
            if (anim != null)
            {
                Vector3 targetPos = anim.transform.position;
                targetPos.y += 1f;
                AttackEndEvent?.Invoke(targetPos);
            }
        }

        private static void EnemyMarking(DamageEvent evt)
        {
            if (evt.Owner as CharacterUnit)
            {
                EnemyMark markCompo = evt.target.GetComponentInChildren<EnemyMark>();
                    
                if (markCompo != null)
                {
                    if (evt.Owner.unitSO.UnitType == UnitType.Archer && markCompo.GetCurrentMark() == 0)
                    {   
                        Bus<UseSpecEvent>.Raise(new UseSpecEvent(UnitType.Archer, evt.target));   
                    }
                    else if (evt.Owner.unitSO.UnitType != UnitType.Archer && markCompo.GetCurrentMark() > 0)
                    {
                        Bus<UseSpecEvent>.Raise(new UseSpecEvent(UnitType.Archer, evt.target));
                    }
                    else
                        return;
                }
            }
        }

        private void CalculateCritical(ref DamageEvent evt, ref bool isCritical, ref bool isPenetrate)
        {
            float damage = evt.DamageData.damage;
            
            if (isCritical)
            {
                damage *= evt.Owner.unitSO.CriticalDamageIncrease;
                evt.DamageData.damage = (int)damage;
                Debug.Log("크리티컬");
                isCritical = true;
                return;
            }
            
            float criticalProbilityValue = Random.Range(0f, 100f);

            if (evt.Owner != null && criticalProbilityValue <= evt.Owner.unitSO.CriticalProbability)
            {
                isCritical = true;
                damage *= evt.Owner.unitSO.CriticalDamageIncrease;
                evt.DamageData.damage = (int)damage;
            }
        }
    }
}
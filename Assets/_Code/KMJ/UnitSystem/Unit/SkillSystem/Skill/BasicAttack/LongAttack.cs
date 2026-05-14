using System.Collections;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.SkillSystem;
using UnityEngine;
using UnityEngine.AI;

public class LongAttack : BasicUnitSkill
    {
        [SerializeField] private float atkMoveSpeed;
        [SerializeField] private Animator animator;
        [SerializeField] private BoomingEffect effectPrefab;
        
        private UnitAnimation _animationCompo;

        
        private GameObject _target = null;
        
        public bool isRunningAttack = false;
        
        private Vector3 _ownTrm;

        protected void Start()
        {
            SkillEvent.AddListener(AttackAction);
            _animationCompo = _characterUnit.GetUnitCompo<UnitAnimation>();
        }

        protected override void StartEvent()
        {
            base.StartEvent();
            triggerCompo.OnAttackTrigger += ShootLongRangeAttack;
            triggerCompo.OnAnimationEndTrigger += SkillEnd;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SkillEvent.RemoveListener(AttackAction);
        }

        public void AttackAction(GameObject target)
        {
            _ownTrm = transform.position;
            
            StartCoroutine(MeleeAttackAction(target));
        }

        private IEnumerator MeleeAttackAction(GameObject target)
        {
            yield return new WaitForSeconds(0.2f);
            
             _target = target;
             
             _animationCompo.PlaySelectAnimation("ATTACK");
        }


        private void ShootLongRangeAttack()
        {
            Vector3 dir = _target.transform.position;
            dir.y += 1.3f;

            dir.x += 0.4f;
            dir.z += 0.4f;
            
            effectPrefab.SetDamageData(DamageData,AddDamage, _target);
            effectPrefab.StartParticleEffect(dir);
            
            if (_characterUnit.unitSO.UnitType == UnitType.Magician)
                Bus<UseGimicEvent>.Raise(new UseGimicEvent(UnitType.Magician, _target));
            
            effectPrefab.gameObject.SetActive(true);
        }


        protected override void SkillEnd()
        {
            base.SkillEnd();
            triggerCompo.OnAttackTrigger -= ShootLongRangeAttack;
            triggerCompo.OnAnimationEndTrigger -= SkillEnd;
            _animationCompo.PlaySelectAnimation("IDLE");
            SkillEndEvent?.Invoke();
        }
    }
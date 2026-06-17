using System.Collections;
using Code.Core;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using Code.SkillSystem;
using UnityEngine;
using UnityEngine.AI;

public class ShooterAttack : BasicUnitSkill
{
        [SerializeField] private Transform shootTrm;
        [SerializeField] private float atkMoveSpeed;
        [SerializeField] private Animator animator;
        
         private UnitAnimation _animationCompo;

        private ShootItemAttackManager _shootItemManager;
        
        private GameObject _target = null;

        protected void Start()
        {
            SkillEvent.AddListener(AttackAction);
            _shootItemManager = _characterUnit.GetUnitCompo<ShootItemAttackManager>();
            _animationCompo = _characterUnit.GetUnitCompo<UnitAnimation>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SkillEvent.RemoveListener(AttackAction);
        }

        protected override void StartEvent()
        {
            base.StartEvent();
            triggerCompo.OnAttackTrigger += Shoot;
            triggerCompo.OnAnimationEndTrigger += SkillEnd;
        }
        

        public void AttackAction(GameObject target)
        {
            StartCoroutine(ShootAttackSet(target));
        }

        private IEnumerator ShootAttackSet(GameObject target)
        {
            yield return new WaitForSeconds(0.4f);
            
            _target = null;
            _target = target;
            SkillFeedbackEvent?.Invoke();
            _animationCompo.PlaySelectAnimation("ATTACK");
        }

        private void Shoot()
        {
            Vector3 pos = shootTrm.transform.position;
            
            Vector3 slashRot = _characterUnit.transform.rotation.eulerAngles;
            
            _shootItemManager.SetTarget(_target);
            _shootItemManager.SetDamageData(DamageData,AddDamage);
            _shootItemManager.CreateShootItem("ShootItem",pos, slashRot);   
            SoundManager.Instance.PlayClip("BowSound");
            Bus<CamShakeEvent>.Raise(new CamShakeEvent(0.15f));
        }

        protected override void SkillEnd()
        {
            base.SkillEnd();
            triggerCompo.OnAttackTrigger -= Shoot;
            triggerCompo.OnAnimationEndTrigger -= SkillEnd;
            _animationCompo.PlaySelectAnimation("IDLE");
            SkillEndEvent.Invoke();
        }
    }
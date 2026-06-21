using System.Collections;
using Code.Combat.StatusEffect;
using Code.Core;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using Code.SkillSystem;
using UnityEngine;

public class FireArrow : BasicUnitSkill
    {
        [SerializeField] private Transform shootTrm;
        private UnitAnimation animtionCompo;

        private GameObject _target;
        
        private ShootItemAttackManager  _shootItemManager;

        [SerializeField] private int burnDuration = 2;
        [SerializeField] private int burnDamage = 5;
        
        protected  void Start()
        {
            SkillEvent.AddListener(AttackAction);
            animtionCompo = _characterUnit.GetUnitCompo<UnitAnimation>();
            _shootItemManager = _characterUnit.GetUnitCompo<ShootItemAttackManager>();
        }

        protected override void StartEvent()
        {
            base.StartEvent();
            triggerCompo.OnAttackTrigger += MakeArrow;
            triggerCompo.OnAnimationEndTrigger += SkillEnd;
        }

        protected override void OnDestroy()
        {
            SkillEvent.RemoveListener(AttackAction);
            base.OnDestroy();
        }

        public void AttackAction(GameObject target)
        {
            _target = null;
            StartCoroutine(FireArrowAction());
            _target = target;
            
        }
        
        protected override void SkillEnd()
        {
            base.SkillEnd();
            triggerCompo.OnAttackTrigger -= MakeArrow;
            triggerCompo.OnAnimationEndTrigger -= SkillEnd;
            SkillEndEvent?.Invoke();
            animtionCompo.PlaySelectAnimation("IDLE");
        }
        
        private IEnumerator FireArrowAction()
        {
            SoundManager.Instance.PlayClip("BowReloadSound");
            yield return new WaitForSeconds(0.3f);
            SoundManager.Instance.PlayClip("BlinkSound");
            yield return new WaitForSeconds(0.1f);
            SkillFeedbackEvent?.Invoke();
            animtionCompo.PlaySelectAnimation("FIRE");
        }
        
        public void MakeArrow()
        {
            Vector3 pos = shootTrm.position;
            
            Vector3 slashRot = transform.rotation.eulerAngles;
            
            _shootItemManager.SetTarget(_target);
            _shootItemManager.SetDamageData(DamageData,AddDamage);
            _shootItemManager.CreateShootItem("FireArrow",pos, slashRot);

            
            SoundManager.Instance.PlayClip("BowSound");
            
            Unit targetUnit = _target != null ? _target.GetComponent<Unit>() : null;

            if (targetUnit != null)
            {
                Bus<ApplyStatusEffectEvent>.Raise(new ApplyStatusEffectEvent(targetUnit, EffectType.Burn,
                    new StatusEffectApplyData(burnDuration, burnDamage)));
            }
        }
    }

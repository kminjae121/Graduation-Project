using System.Collections;
using Code.Combat.StatusEffect;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using Code.SkillSystem;
using UnityEngine;

public class FireArrow : BasicUnitSkill
    {
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
            yield return new WaitForSeconds(0.4f);
            SkillFeedbackEvent?.Invoke();
            animtionCompo.PlaySelectAnimation("FIRE");
        }
        
        public void MakeArrow()
        {
            Vector3 pos = _characterUnit.GetComponentInChildren<UnitAnimation>().transform.position;
            pos.y += 0.3f;
            Vector3 slashRot = transform.rotation.eulerAngles;
            
            _shootItemManager.SetTarget(_target);
            _shootItemManager.SetDamageData(DamageData,AddDamage);
            _shootItemManager.CreateShootItem("FireArrow",pos, slashRot);
            
            Bus<ApplyStatusEffectEvent>.Raise(new ApplyStatusEffectEvent(_target.GetComponent<Unit>(), EffectType.Burn,
                new StatusEffectApplyData(burnDuration, burnDamage)));
        }
    }
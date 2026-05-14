using System.Collections;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using Code.SkillSystem;
using UnityEngine;
using UnityEngine.UIElements;


public class HealSkill : BasicUnitSkill
    {
        private UnitAnimation animtionCompo;
        private GameObject _target;

        protected  void Start()
        {
            SkillEvent.AddListener(HealAction);
            animtionCompo = _characterUnit.GetUnitCompo<UnitAnimation>();
        }

        protected override void StartEvent()
        {
            base.StartEvent();
            triggerCompo.OnAttackTrigger += Heal;
            triggerCompo.OnAnimationEndTrigger += SkillEnd;
        }

        protected override void OnDestroy()
        { 
            SkillEvent.RemoveListener(HealAction);
            base.OnDestroy();
            
        }
        
        public void HealAction(GameObject target)
        {
            StartCoroutine(FireBall());
            _target = target;
        }
        
        private IEnumerator FireBall()
        {
            Bus<UnitSetMoveEvent>.Raise(new UnitSetMoveEvent(false));
            yield return new WaitForSeconds(0.4f);
            animtionCompo.PlaySelectAnimation("HEAL");
            
            SkillFeedbackEvent?.Invoke();
        }

        protected override void SkillEnd()
        {
            base.SkillEnd();
            SkillEndEvent?.Invoke();
            Bus<TurnEndUIEvent>.Raise(new TurnEndUIEvent(false)); 
            animtionCompo.PlaySelectAnimation("IDLE");
            triggerCompo.OnAttackTrigger-= Heal;
            triggerCompo.OnAnimationEndTrigger -= SkillEnd;
        }

        public void Heal()
        {
            Bus<UseGimicEvent>.Raise(new UseGimicEvent(UnitType.Magician, _target));
            UnitHealth health = _characterUnit.GetUnitCompo<UnitHealth>();

            health.HealHp(20);
        }
    }
using System.Collections;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.SkillSystem;
using UnityEngine;

    public class AddAvoideProbablity : BasicUnitSkill
    {
        private UnitAnimation animtionCompo;

        private int skillCnt = 0;

        protected void Start()
        {
            SkillEvent.AddListener(AddAP);
            animtionCompo = _characterUnit.GetUnitCompo<UnitAnimation>();
        }

        protected override void StartEvent()
        {
            base.StartEvent();
            triggerCompo.OnAttackTrigger += PlusAvoideProbablity;
            triggerCompo.OnAnimationEndTrigger += HandleSkillEnd;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SkillEvent.RemoveListener(AddAP);
        }

        private void AddAP(GameObject obj)
        {   
            StartCoroutine(AddAvoid());
        }

        private IEnumerator AddAvoid()
        {
            Bus<UnitSetMoveEvent>.Raise(new UnitSetMoveEvent(false));
            yield return new WaitForSeconds(0.4f);
            SkillFeedbackEvent?.Invoke();   
            animtionCompo.PlaySelectAnimation("HEAL");
        }
    
        private void PlusAvoideProbablity()
        {
            if (skillCnt >= 3)
            {
                _characterUnit.InitializeAvoidProbability();
                return;
            }

            skillCnt += 1;
            
            _characterUnit.AddAvoidProbability += 10;
            _characterUnit.unitSO.AvoidProbability += 10;
        }

        private void HandleSkillEnd()
        {
            SkillEnd();
        }
        
        protected override void SkillEnd()
        { 
            base.SkillEnd();
            animtionCompo.PlaySelectAnimation("IDLE");
            triggerCompo.OnAnimationEndTrigger -= SkillEnd; 
            triggerCompo.OnAttackTrigger -= PlusAvoideProbablity;
            Bus<TurnEndUIEvent>.Raise(new TurnEndUIEvent(false));
            SkillEndEvent?.Invoke();
        }
    }
using System.Collections;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.SkillSystem;
using UnityEngine;

    public class AddAvoideProbablity : BasicUnitSkill
    {
        private UnitAnimation _animtionCompo;

        private int _skillCnt = 0;

        protected void Start()
        {
            SkillEvent.AddListener(AddAP);
            _animtionCompo = _characterUnit.GetUnitCompo<UnitAnimation>();
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
            _animtionCompo.PlaySelectAnimation("HEAL");
        }
    
        private void PlusAvoideProbablity()
        {
            if (_skillCnt >= 3)
            {
                _characterUnit.InitializeAvoidProbability();
                return;
            }

            _skillCnt += 1;
            
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
            _animtionCompo.PlaySelectAnimation("IDLE");
            triggerCompo.OnAnimationEndTrigger -= SkillEnd; 
            triggerCompo.OnAttackTrigger -= PlusAvoideProbablity;
            Bus<TurnEndUIEvent>.Raise(new TurnEndUIEvent(false));
            SkillEndEvent?.Invoke();
        }
    }
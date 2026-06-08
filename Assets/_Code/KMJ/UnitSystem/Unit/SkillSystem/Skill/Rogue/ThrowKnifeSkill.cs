using System.Collections;
using System.Collections.Generic;
using Code.UnitSystem;
using Code.SkillSystem;
using Code.UnitSystem.TraitSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

    public class ThrowKnifeSkill : BasicUnitSkill
    {
        private UnitAnimation _animtionCompo;

        private RogueShadowSpawn _shadowSpawn;
        
        protected void Start()
        {
            SkillEvent.AddListener(AttackAction);
            _animtionCompo = _characterUnit.GetUnitCompo<UnitAnimation>();
            _shadowSpawn = _characterUnit.GetUnitCompo<RogueShadowSpawn>();
        }

        protected override void StartEvent()
        {
            base.StartEvent();
            triggerCompo.OnAttackTrigger += MoveShadow;
            triggerCompo.OnAnimationEndTrigger += SkillEnd;
        }

        protected override void OnDestroy()
        {
            SkillEvent.RemoveListener(AttackAction);
            base.OnDestroy();
        }

        public void AttackAction(GameObject target)
        {
            StartCoroutine(SlashFlag());
        }
        
        private IEnumerator SlashFlag()
        {
            yield return new WaitForSeconds(0.4f);
            SkillFeedbackEvent?.Invoke();   
            _animtionCompo.PlaySelectAnimation("THROW");
        }
        
        public void MoveShadow()
        {
            if (_shadowSpawn.GetCurrentShadow() == null)
                return;
             
            _shadowSpawn.GetShadowMapTile().SetTileUnit(_characterUnit);
            _characterUnit.transform.position = _shadowSpawn.GetCurrentShadow().transform.position;
            _shadowSpawn.SetShadowInfo(_shadowSpawn.GetCurrentShadow(), false);
            
        }
        
        protected override void SkillEnd()
        {
            base.SkillEnd();
            
            triggerCompo.OnAttackTrigger -= MoveShadow;
            triggerCompo.OnAnimationEndTrigger -= SkillEnd;
            SkillEndEvent?.Invoke();
            _animtionCompo.PlaySelectAnimation("IDLE");
        }
    }
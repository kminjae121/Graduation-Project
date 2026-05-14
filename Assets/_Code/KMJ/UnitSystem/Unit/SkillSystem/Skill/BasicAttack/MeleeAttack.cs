using System.Collections;
using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using Code.SkillSystem;
using UnityEngine;
using UnityEngine.AI;

public class MeleeAttack : BasicUnitSkill
{ 
    [SerializeField] private Animator animator;
    [SerializeField] private float atkMoveSpeed;
    [SerializeField] private AttackDataSO atkData;
    
    private UnitAnimation _animationCompo;
    
    public bool isRunningAttack = false;
    
    private Vector3 _ownTrm;
    
    private GameObject _target = null;
    
    protected void Start()
    {
        SkillEvent.AddListener(AttackAction);
        _animationCompo = _characterUnit.GetUnitCompo<UnitAnimation>();
    }

    protected override void StartEvent()
    {
        base.StartEvent();
        triggerCompo.OnAttackTrigger += TakeDamage;
        triggerCompo.OnAnimationEndTrigger += AttackEnd;
    }

    protected override void OnDestroy()
    {
        SkillEvent.RemoveListener(AttackAction);
    }
    
    public void AttackAction(GameObject target)
    {
        _ownTrm = _characterUnit.transform.position;
        _target = target;
        
        StartCoroutine(MeleeAttackAction(target));
    }
    
    private IEnumerator MeleeAttackAction(GameObject target)
    {
        yield return new WaitForSeconds(0.4f);
        
         SkillFeedbackEvent?.Invoke();
         _animationCompo.PlaySelectAnimation("ATTACK");
    }
    
    public void AttackEnd()
    {
        ReturnOwnPos();
    }
    
    private void ReturnOwnPos()
    {
        _animationCompo.PlaySelectAnimation("IDLE");
        SkillEnd();
    }
    
    protected override void SkillEnd()
    {
        base.SkillEnd();
        triggerCompo.OnAttackTrigger -= TakeDamage;
        triggerCompo.OnAnimationEndTrigger -= AttackEnd;
        SkillEndEvent?.Invoke();
    }
    
    public void TakeDamage()
    {
        if (_characterUnit.unitSO.UnitType == UnitType.Bandlt)
            Bus<UseGimicEvent>.Raise(new UseGimicEvent(UnitType.Bandlt, _target));
        
        Bus<DamageEvent>.Raise(new DamageEvent(DamageData,_target,AddDamage, _characterUnit,false,false,0.3f));
    }
}
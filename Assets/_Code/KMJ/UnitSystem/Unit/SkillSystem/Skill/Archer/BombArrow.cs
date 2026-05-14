using System.Collections;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using Code.SkillSystem;
using UnityEngine;

public class BombArrow : BasicUnitSkill
{
    private UnitAnimation animtionCompo;

    private GameObject _target;

    private ShootItemAttackManager _shootItemManager;
    
    protected void Start()
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
        StartCoroutine(FireArrowAction());
        _target = target;
    }
    
    private IEnumerator FireArrowAction()
    {
        yield return new WaitForSeconds(0.3f);
        yield return new WaitForSeconds(0.1f);
        SkillFeedbackEvent?.Invoke();
        animtionCompo.PlaySelectAnimation("AIM");
    }

    protected override void SkillEnd()
    {
        base.SkillEnd();
        triggerCompo.OnAttackTrigger -= MakeArrow;
        triggerCompo.OnAnimationEndTrigger -= SkillEnd;
        SkillEndEvent?.Invoke();
        animtionCompo.PlaySelectAnimation("IDLE");
    }
    
    public void MakeArrow()
    {
        Bus<CamShakeEvent>.Raise(new CamShakeEvent(0.4f));
        Vector3 pos = _characterUnit.GetComponentInChildren<UnitAnimation>().transform.position;
        pos.y += 0.3f;
        
        Vector3 slashRot = transform.rotation.eulerAngles;
        
        _shootItemManager.SetTarget(_target);
        _shootItemManager.SetDamageData(DamageData,AddDamage);
        _shootItemManager.CreateShootItem("BombArrow",pos, slashRot);
    
        _target = null;
    }
}
using System.Collections;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.SkillSystem;
using UnityEngine;

public class BasicAttackSkill : BasicUnitSkill
{
    [SerializeField] private Animator animator;
    private UnitAnimation animtionCompo;

    [SerializeField] private float atkMoveSpeed;

    private GameObject _target;
        
    private Vector3 _ownTrm;
    
    protected void Start()
    {
        SkillEvent.AddListener(AttackAction);
        animtionCompo = _characterUnit.GetUnitCompo<UnitAnimation>();
    }

    protected override void StartEvent()
    {
        base.StartEvent();
        triggerCompo.OnAnimationEndTrigger += AttackEnd;
        triggerCompo.OnAttackTrigger += TakeDamage;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        SkillEvent.RemoveListener(AttackAction);
    }
    


    public void AttackAction(GameObject target)
    {
        _ownTrm = _characterUnit.transform.position;
        _target = null;
        _target = target;
        StartCoroutine(MeleeAttackAction(_targetEnemy)); ;
    }

    private IEnumerator MeleeAttackAction(GameObject target)
    {
        yield return new WaitForSeconds(0.2f);
        SkillFeedbackEvent?.Invoke();
        yield return new WaitForSeconds(0.2f);
        
        animtionCompo.PlaySelectAnimation("BAS");
    }
    
    public void TakeDamage()
    {
        Bus<DamageEvent>.Raise(new DamageEvent(DamageData,_target,AddDamage,_characterUnit,false,false,0.15f));
    }

    public void AttackEnd()
    {
        ReturnOwnPos();
    }

    private void ReturnOwnPos()
    {
        SkillEnd();
        animtionCompo.PlaySelectAnimation("IDLE");
    }

    protected override void SkillEnd()
    {
        base.SkillEnd();
        triggerCompo.OnAnimationEndTrigger -= AttackEnd;
        triggerCompo.OnAttackTrigger -= TakeDamage;
        Bus<TurnEndUIEvent>.Raise(new TurnEndUIEvent(false));
        SkillEndEvent?.Invoke();
    }
}

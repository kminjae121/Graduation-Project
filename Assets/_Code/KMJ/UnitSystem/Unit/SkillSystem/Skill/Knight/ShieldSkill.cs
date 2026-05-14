using System.Collections;
using Code.Core.Events.Bus;
using Code.Managers;
using Code.UnitSystem;
using Code.SkillSystem;
using Code.SkillSystem.Skill.Knight;
using UnityEngine;


public class ShieldSkill : BasicUnitSkill
{
    private KnightShieldCompo _shieldCompo;
    private UnitAnimation animtionCompo;

    private int _inGameDefensePower = 0;

    protected void Start()
    {
        SkillEvent.AddListener(AddAP);
        animtionCompo = _characterUnit.GetUnitCompo<UnitAnimation>();
        _shieldCompo = _characterUnit.GetComponentInChildren<KnightShieldCompo>();
    }

    protected override void StartEvent()
    {
        triggerCompo.OnAnimationEndTrigger += SkillEnd;
        base.StartEvent();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _characterUnit.unitSO.DefensivePower -= _inGameDefensePower;
            
        SkillEvent.RemoveListener(AddAP);

    }

    private void AddAP(GameObject obj)
    {
        StartCoroutine(Shield());
    }

    public void SetShield()
    {
        _inGameDefensePower += 10;
        _characterUnit.unitSO.DefensivePower += 10;
    }
    

    private IEnumerator Shield()
    {
        yield return new WaitForSeconds(0.4f);
        SetShield();
        SkillFeedbackEvent?.Invoke();
        animtionCompo.PlaySelectAnimation("SHELD");
    }
    
    protected override void SkillEnd()
    {
        base.SkillEnd();
        triggerCompo.OnAnimationEndTrigger -= SkillEnd;
        Bus<TurnEndUIEvent>.Raise(new TurnEndUIEvent(false));
        SkillEndEvent?.Invoke();
        animtionCompo.PlaySelectAnimation("IDLE");
    }
}
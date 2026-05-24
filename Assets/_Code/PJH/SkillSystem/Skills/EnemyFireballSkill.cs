using Code.Combat.StatusEffect;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using UnityEngine;

namespace Code.SkillSystem
{
    public class EnemyFireballSkill : EnemyRangedSkill
    {
        protected override void Attack(GameObject target)
        {
            base.Attack(target);
            
            Bus<ApplyStatusEffectEvent>.Raise(new ApplyStatusEffectEvent(target.GetComponent<Unit>(), EffectType.Burn, new StatusEffectApplyData(3, 5)));
        }
    }
}
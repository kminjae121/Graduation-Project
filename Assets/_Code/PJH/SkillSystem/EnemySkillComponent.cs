using Code.Core.Events.Bus;

namespace Code.SkillSystem
{
    public class EnemySkillComponent : SkillComponent
    {
        protected override void StartSkill(BaseSkill skill, SkillSO skillSO)
        {
            skill.ConfigureSkillRange(skillSO);
            skill.ShowSkillRange();
            Bus<UsingSkillEvent>.Raise(new UsingSkillEvent(false));
        }

        protected override void CancelSkill(BaseSkill skill)
        {
            skill.SkillFinished(false);
            skill.BooleanSkillUse(false);
            Bus<UsingSkillEvent>.Raise(new UsingSkillEvent(true));
        }
    }
}
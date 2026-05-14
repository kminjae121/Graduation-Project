using Code.SkillSystem;

namespace Code.UnitSystem.Enemies.AI
{
    public readonly struct EnemySkillPick
    {
        public readonly Unit Target;
        public readonly SkillSO SkillSO;
        public readonly EnemyBaseSkill Skill;
        public readonly float Score;

        public EnemySkillPick(Unit target, SkillSO skillSO, EnemyBaseSkill skill, float score)
        {
            Target = target;
            SkillSO = skillSO;
            Skill = skill;
            Score = score;
        }

        public bool IsValid => Target != null && SkillSO != null && Skill != null;
    }
}
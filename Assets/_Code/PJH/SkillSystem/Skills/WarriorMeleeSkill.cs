using Code.Core;
using UnityEngine;

namespace Code.SkillSystem
{
    public class WarriorMeleeSkill : EnemyMeleeSkill
    {
        [SerializeField] private string[] effectNames;

        private int _attackIndex;

        protected override void StartEvent()
        {
            base.StartEvent();
            triggerCompo.OnSoundPlayTrigger += SoundPlay;
        }

        private void SoundPlay()
        {
            SoundManager.Instance.PlayClip("SlashSound");
        }

        protected override void Attack(GameObject target)
        {
            PlayAttackEffect();
            ++_attackIndex;
            
            base.Attack(target);
        }

        protected override void FinishSkill()
        {
            StopAttackEffects();
            _attackIndex = 0;
            
            base.FinishSkill();
            triggerCompo.OnSoundPlayTrigger -= SoundPlay;
        }

        private void PlayAttackEffect()
        {
            int index = Mathf.Clamp(_attackIndex, 0, effectNames.Length - 1);
            Owner.VFXCompo.PlayVFX(effectNames[index], Owner.transform.position, Owner.transform.rotation);
        }

        private void StopAttackEffects()
        {
            if (Owner?.VFXCompo == null || effectNames == null)
                return;

            foreach (var effect in effectNames)
                if (!string.IsNullOrWhiteSpace(effect))
                    Owner.VFXCompo.StopVFX(effect);
        }
    }
}
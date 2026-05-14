using Code.UnitSystem;
using Code.UnitSystem.Combat;

namespace _Code.Passive
{
    public class ThornmailPassive : AlwaysTurnPassive
    {
        private CharacterUnit _character;

        private void Awake()
        {
            _character = _unit as CharacterUnit;
        }

        public override void StartPassive()
        {
            _character.HealthCompo.OnInteractionEvent.AddListener(Thornmail);
        }

        public override void StopPassive()
        {
            if (_character == null)
                return;
            if (_character.HealthCompo == null)
                return;
            _character.HealthCompo.OnInteractionEvent.RemoveListener(Thornmail);
        }

        private void Thornmail(Unit target, int value)
        {
            DamageData damageData = new DamageData();
            damageData.damage = (int)(value * 0.3f);

            if (target != null)
            {
                if (target.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.ApplyDamage(damageData, target.transform.position,
                        transform.transform.position, null, _character, false);
                }   
            }
        }
    }
}
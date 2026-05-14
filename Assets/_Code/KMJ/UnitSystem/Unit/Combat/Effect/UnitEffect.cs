using UnityEngine;

namespace _Code.UnitSystem
{
    public class UnitEffect : MonoBehaviour
    {
        [field: SerializeField] public string EffectName { get; private set; }

        [SerializeField] private ParticleSystem particleSystem;


        private void OnValidate()
        {
            gameObject.name = EffectName;
        }

        public void PlayEffect()
        {
            particleSystem.Play(true);
        }

        public void StopEffect()
        {
            particleSystem.Stop(true);
        }
    }
}
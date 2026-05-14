using System;
using UnityEngine;

namespace Code.Effects
{
    public class PlayParticleVFX : MonoBehaviour, IPlayableVFX
    {
        [field:SerializeField ] public string VfxName { get; private set; }
        [SerializeField] private bool isOnPosition;
        [SerializeField] private ParticleSystem particle;
        
        public void PlayVfx(Vector3 position, Quaternion rotation)
        {
            if(isOnPosition == false)
                transform.SetPositionAndRotation(position, rotation);
            
            particle.Play(true); //true는 생략해도 된다.
        }

        public void StopVfx()
        {
            particle.Stop(true);
        }

        private void OnValidate()
        {
            if(string.IsNullOrEmpty(VfxName) == false)
                gameObject.name = VfxName;
        }
    }
}
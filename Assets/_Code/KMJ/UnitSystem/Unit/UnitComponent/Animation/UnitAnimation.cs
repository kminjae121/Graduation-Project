using UnityEngine;

namespace Code.UnitSystem
{
    public class UnitAnimation : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private Animator _animator;
        
        public void Initialize(Unit owner)
        {
            AnimationAllStop();
        }

        public void PlaySelectAnimation(string animationName)
        {
            if (_animator == null)
                return;
            
            AnimationAllStop();
            
            int animHash = Animator.StringToHash(animationName);
            _animator.SetBool(animHash, true);
        }

        public void ReturnIdleAnimation()
        {
            PlaySelectAnimation("IDLE");
        }

        public void AnimationAllStop()
        {
            if (_animator == null) return;

            foreach (var param in _animator.parameters)
                if (param.type == AnimatorControllerParameterType.Bool)
                    _animator.SetBool(param.nameHash, false);
        }
        
        public void RestartFromEntry()
        {
            if (_animator == null) return;
            
            AnimationAllStop();
            ResetAllTriggers();
            
            _animator.Rebind();
            _animator.Update(0f);
        }
        
        private void ResetAllTriggers()
        {
            if (_animator == null) return;

            foreach (var param in _animator.parameters)
                if (param.type == AnimatorControllerParameterType.Trigger)
                    _animator.ResetTrigger(param.nameHash);
        }
    }
}
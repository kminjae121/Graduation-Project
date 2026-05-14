using Code.Effects;
using Code.UnitSystem.Combat;
using DG.Tweening;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.Feedbacks
{
    public class ImplosionFeedback : Feedback
    {
        [SerializeField] private PoolingItemSO implosionPool;
        [SerializeField] private float playDuration = 0.5f;
        [SerializeField] private ActionData actionData;
        
        [Inject] private PoolManagerMono _poolManager;

        private void Awake()
        {
            Injector.InjectInto(this);
        }

        public override void CreateFeedback()
        {
            if (_poolManager == null)
            {
                Debug.LogError("풀 매니저가 주입되지 않아 이펙트를 생성할 수 없습니다.");
                return;
            }

            PoolingEffect effect = _poolManager.Pop<PoolingEffect>(implosionPool);
            
            Quaternion rotation = Quaternion.LookRotation(actionData.HitNormal * -1);
            effect.PlayVFX(actionData.HitPoint, rotation);

            DOVirtual.DelayedCall(playDuration, ()=>
            {
                _poolManager.Push(effect);
            });
        }

        public override void StopFeedback()
        {
        }
    }
}
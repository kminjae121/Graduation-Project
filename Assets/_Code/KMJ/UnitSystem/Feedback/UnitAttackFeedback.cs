using Code.Effects;
using DG.Tweening;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.Feedbacks
{
    public class UnitAttackFeedback : Feedback
    {
        [SerializeField] private PoolingItemSO atkEffectPool;
        [SerializeField] private float playDuration = 1f;
        [SerializeField] private Vector3 HitPoint;
        
        [Inject] private PoolManagerMono _poolManager;

        private void Awake()
        {
            Injector.InjectInto(this);
        }

        public void StartFeedback(Vector3 hitPoint)
        {
            HitPoint = hitPoint;
            CreateFeedback();
        }

        public override void CreateFeedback()
        {
            if (_poolManager == null)
            {
                Debug.LogError("풀 매니저가 주입되지 않아 이펙트를 생성할 수 없습니다.");
                return;
            }

            PoolingEffect effect = _poolManager.Pop<PoolingEffect>(atkEffectPool);
            
            Quaternion rotation = Quaternion.LookRotation(HitPoint * -1);
            effect.PlayVFX(HitPoint, rotation);

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
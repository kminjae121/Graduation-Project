using Code.Effects;
using Code.UnitSystem.Combat;
using DG.Tweening;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.Feedbacks
{
    public class HitImpactFeedback : Feedback
    {
        [SerializeField] private PoolingItemSO hitImpactItem;
        [SerializeField] private float playDuration = 0.5f;
        [SerializeField] private ActionData actionData;
        [SerializeField] private DamageType allowedDamageType; //이펙트가 재생될 데미지 타입
        
        [Inject] private PoolManagerMono _poolManager;
        
        public override void CreateFeedback()
        {
            PoolingEffect effect = _poolManager.Pop<PoolingEffect>(hitImpactItem);
            
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
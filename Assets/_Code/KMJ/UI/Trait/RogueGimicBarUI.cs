using _Code.Core.EventBus.Events.Trait;
using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class RogueGimicBarUI : GimicUI
    {
        [SerializeField] private Image gaugeImg;
        
        private void Start()
        {
            Bus<RogueGimicBarEvent>.Subscribe(SetKnightGimicBar);
        }

        private void OnDestroy()
        {
            Bus<RogueGimicBarEvent>.Unsubscribe(SetKnightGimicBar);
        }
        private void SetKnightGimicBar(RogueGimicBarEvent evt)
        {
            gaugeImg.DOFillAmount(evt.value / 3,0.5f);
        }

        public override void OperationUI()
        {
            
        }
    }
}
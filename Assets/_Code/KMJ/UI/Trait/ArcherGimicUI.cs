using _Code.Core.EventBus.Events.Trait;
using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class ArcherGimicUI : GimicUI
    {
        [SerializeField] private Image gaugeImg;
        
        private void Start()
        {
            Bus<ArcherGimicEvent>.Subscribe(SetKnightGimicBar);
        }

        private void OnDestroy()
        {
            Bus<ArcherGimicEvent>.Unsubscribe(SetKnightGimicBar);
        }
        private void SetKnightGimicBar(ArcherGimicEvent evt)
        {
            gaugeImg.DOFillAmount(evt.value / 8,0.5f);
        }

        public override void OperationUI()
        {
            
        }
    }
}
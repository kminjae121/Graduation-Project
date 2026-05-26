using _Code.Core.EventBus.Events.Trait;
using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class MagicianGimicBarUI : GimicUI
    {
        [SerializeField] private Image gaugeImg;
        
        private void Start()
        {
            Bus<MaigicianBarEvent>.Subscribe(SetKnightGimicBar);
        }

        private void OnDestroy()
        {
            Bus<MaigicianBarEvent>.Unsubscribe(SetKnightGimicBar);
        }
        private void SetKnightGimicBar(MaigicianBarEvent evt)
        {
            gaugeImg.DOFillAmount(evt.value / 4,0.5f);
        }

        public override void OperationUI()
        {
            
        }
    }
}
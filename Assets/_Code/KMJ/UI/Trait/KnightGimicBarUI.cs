using System;
using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class KnightGimicBarUI : GimicUI
    {
        [SerializeField] private Image gaugeImg;
        
        private void Start()
        {
            Bus<KnightGimicBarEvent>.Subscribe(SetKnightGimicBar);
        }

        private void OnDestroy()
        {
            Bus<KnightGimicBarEvent>.Unsubscribe(SetKnightGimicBar);
        }
        private void SetKnightGimicBar(KnightGimicBarEvent evt)
        {
            gaugeImg.DOFillAmount(evt.value / 7,0.5f);
        }

        public override void OperationUI()
        {
            
        }
    }
}
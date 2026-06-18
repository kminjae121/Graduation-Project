using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class KnightSpecUI : SpecUI
    {
        [SerializeField] private Image gaugeImg;

        private void Start()
        {
            Bus<KnightSpecEvent>.Subscribe(SetKnightSpecBar);
        }

        private void OnDestroy()
        {
            Bus<KnightSpecEvent>.Unsubscribe(SetKnightSpecBar);
        }

        private void SetKnightSpecBar(KnightSpecEvent evt)
        {
            gaugeImg.DOFillAmount(evt.value / 7, 0.5f);
        }

        public override void OperationUI()
        {
        }
    }
}

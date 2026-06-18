using _Code.Core.EventBus.Events.Trait;
using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class MagicianSpecUI : SpecUI
    {
        [SerializeField] private Image gaugeImg;

        private void Start()
        {
            Bus<MagicianSpecEvent>.Subscribe(SetMagicianSpecBar);
        }

        private void OnDestroy()
        {
            Bus<MagicianSpecEvent>.Unsubscribe(SetMagicianSpecBar);
        }

        private void SetMagicianSpecBar(MagicianSpecEvent evt)
        {
            gaugeImg.DOFillAmount(evt.value / 4, 0.5f);
        }

        public override void OperationUI()
        {
        }
    }
}

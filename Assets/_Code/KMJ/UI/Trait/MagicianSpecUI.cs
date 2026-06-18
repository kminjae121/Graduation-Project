using _Code.Core.EventBus.Events.Trait;
using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class MagicianSpecUI : SpecUI
    {
        [Header("Gauge Image")]
        [SerializeField] private Image gaugeImg;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float fillDuration = 1.1f;
        [SerializeField] private Ease fillEase = Ease.OutCubic;

        private const float MaxGaugeValue = 4f;
        private Tween _fillTween;

        private void Start()
        {
            Bus<MagicianSpecEvent>.Subscribe(SetMagicianSpecBar);
        }

        private void OnDestroy()
        {
            Bus<MagicianSpecEvent>.Unsubscribe(SetMagicianSpecBar);
            _fillTween?.Kill();
        }

        private void SetMagicianSpecBar(MagicianSpecEvent evt)
        {
            if (gaugeImg == null)
                return;

            float targetFillAmount = Mathf.Clamp01(evt.value / MaxGaugeValue);

            _fillTween?.Kill();
            _fillTween = gaugeImg
                .DOFillAmount(targetFillAmount, fillDuration)
                .SetEase(fillEase);
        }

        public override void OperationUI()
        {
        }
    }
}

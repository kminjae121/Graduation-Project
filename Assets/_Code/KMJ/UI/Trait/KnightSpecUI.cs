using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class KnightSpecUI : SpecUI
    {
        [Header("Gauge Image")]
        [SerializeField] private Image gaugeImg;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float fillDuration = 1.1f;
        [SerializeField] private Ease fillEase = Ease.OutCubic;

        private const float MaxGaugeValue = 7f;
        private Tween _fillTween;

        private void Start()
        {
            Bus<KnightSpecEvent>.Subscribe(SetKnightSpecBar);
        }

        private void OnDestroy()
        {
            Bus<KnightSpecEvent>.Unsubscribe(SetKnightSpecBar);
            _fillTween?.Kill();
        }

        private void SetKnightSpecBar(KnightSpecEvent evt)
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

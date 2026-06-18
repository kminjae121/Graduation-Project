using System.Collections.Generic;
using _Code.Core.EventBus.Events.Trait;
using Code.Core.Events.Bus;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class RogueSpecUI : SpecUI
    {
        [Header("Gauge Images")]
        [SerializeField] private List<Image> gaugeImages = new List<Image>();

        [Header("Animation")]
        [SerializeField, Min(0f)] private float fillDuration = 0.5f;

        private const int MaxShadowCount = 3;
        private List<Image> _resolvedGaugeImages = new List<Image>();

        private void Awake()
        {
            _resolvedGaugeImages = ResolveGaugeImages(transform, gaugeImages);
            SetCountGaugeImages(_resolvedGaugeImages, 0, MaxShadowCount, fillDuration, true);
        }

        private void Start()
        {
            Bus<RogueSpecEvent>.Subscribe(SetRogueSpecBar);
        }

        private void OnDestroy()
        {
            Bus<RogueSpecEvent>.Unsubscribe(SetRogueSpecBar);
            KillGaugeImageTweens(_resolvedGaugeImages);
        }

        private void SetRogueSpecBar(RogueSpecEvent evt)
        {
            int shadowCount = Mathf.FloorToInt(evt.value);
            SetCountGaugeImages(_resolvedGaugeImages, shadowCount, MaxShadowCount, fillDuration, false);
        }

        public override void OperationUI()
        {
        }
    }
}

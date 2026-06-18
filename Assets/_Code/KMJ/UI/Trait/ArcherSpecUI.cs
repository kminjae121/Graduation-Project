using System.Collections.Generic;
using _Code.Core.EventBus.Events.Trait;
using Code.Core.Events.Bus;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class ArcherSpecUI : SpecUI
    {
        [Header("Gauge Images")]
        [SerializeField] private List<Image> gaugeImages = new List<Image>();

        [Header("Animation")]
        [SerializeField, Min(0f)] private float fillDuration = 0.5f;

        private List<Image> _resolvedGaugeImages = new List<Image>();

        private void Awake()
        {
            _resolvedGaugeImages = ResolveGaugeImages(transform, gaugeImages);
            SetCountGaugeImages(_resolvedGaugeImages, 0, GetMaxMarkCount(), fillDuration, true);
        }

        private void Start()
        {
            Bus<ArcherSpecEvent>.Subscribe(SetArcherSpecBar);
        }

        private void OnDestroy()
        {
            Bus<ArcherSpecEvent>.Unsubscribe(SetArcherSpecBar);
            KillGaugeImageTweens(_resolvedGaugeImages);
        }

        private void SetArcherSpecBar(ArcherSpecEvent evt)
        {
            int markCount = Mathf.FloorToInt(evt.value);
            SetCountGaugeImages(_resolvedGaugeImages, markCount, GetMaxMarkCount(), fillDuration, false);
        }

        private int GetMaxMarkCount()
        {
            return Mathf.Max(1, _resolvedGaugeImages.Count);
        }

        public override void OperationUI()
        {
        }
    }
}

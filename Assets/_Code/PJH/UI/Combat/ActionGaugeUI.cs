using Code.Core.Events.Bus;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class ActionGaugeUI : MonoBehaviour
    {
        [SerializeField] private Image actionGaugeImage;
        [SerializeField] private TextMeshProUGUI actionGaugeText;
        [SerializeField] private float tweenTime = 0.3f;

        private Tween _gaugeTween;
        
        private void Awake()
        {
            Bus<ActionGaugeEvent>.Subscribe(HandleValueChange);
        }

        private void OnDestroy()
        {
            Bus<ActionGaugeEvent>.Unsubscribe(HandleValueChange);
        }

        private void HandleValueChange(ActionGaugeEvent evt)
        {
            actionGaugeText.text = $"{evt.Value * 100}";
            
            _gaugeTween?.Kill();
            _gaugeTween = actionGaugeImage
                .DOFillAmount(evt.Value, tweenTime)
                .SetEase(Ease.OutCubic);
        }
    }
}
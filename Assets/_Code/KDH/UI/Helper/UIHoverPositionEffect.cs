using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIHoverPositionEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Vector2 hoverOffset = new Vector2(0f, 10f);
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private Ease easeType = Ease.OutCubic;
        
        [SerializeField] private CanvasGroup hoverCanvasGroup;

        private RectTransform _rectTransform;
        private Vector2 _originalPosition;
        private Tween _moveTween;
        private Tween _fadeTween;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                Debug.LogError("RectTransform 컴포넌트를 찾을 수 없습니다.");
                return;
            }
            _originalPosition = _rectTransform.anchoredPosition;

            if (hoverCanvasGroup != null)
            {
                hoverCanvasGroup.alpha = 0f;
            }
        }

        private void OnDisable()
        {
            _moveTween?.Kill();
            _fadeTween?.Kill();
            
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _originalPosition;
            }

            if (hoverCanvasGroup != null)
            {
                hoverCanvasGroup.alpha = 0f;
            }
        }

        private void OnDestroy()
        {
            _moveTween?.Kill();
            _fadeTween?.Kill();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _moveTween?.Kill();
            _moveTween = _rectTransform.DOAnchorPos(_originalPosition + hoverOffset, duration)
                .SetEase(easeType)
                .SetUpdate(true);

            if (hoverCanvasGroup != null)
            {
                _fadeTween?.Kill();
                _fadeTween = hoverCanvasGroup.DOFade(1f, duration)
                    .SetEase(easeType)
                    .SetUpdate(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _moveTween?.Kill();
            _moveTween = _rectTransform.DOAnchorPos(_originalPosition, duration)
                .SetEase(easeType)
                .SetUpdate(true);

            if (hoverCanvasGroup != null)
            {
                _fadeTween?.Kill();
                _fadeTween = hoverCanvasGroup.DOFade(0f, duration)
                    .SetEase(easeType)
                    .SetUpdate(true);
            }
        }
    }
}
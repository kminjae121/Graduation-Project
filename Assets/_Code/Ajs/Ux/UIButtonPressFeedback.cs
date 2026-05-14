using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Ajs.Ux
{
    public class UIButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler, IPointerEnterHandler
    {
        [SerializeField, Range(0.7f, 1f)] private float pressedScale = 0.9f;
        [SerializeField, Min(0.01f)] private float pressDuration = 0.06f;
        [SerializeField, Min(0.01f)] private float releaseDuration = 0.08f;
        [Header("Hover Scale")]
        [SerializeField] private bool enableHoverScale = true;
        [SerializeField, Range(1f, 1.3f)] private float hoverScale = 1.08f;
        [Header("Click Side Fade")]
        [SerializeField] private bool enableClickSideFade;
        [SerializeField] private bool moveToRightOnClick = true;
        [SerializeField, Min(1f)] private float clickMoveDistance = 22f;
        [SerializeField, Min(0.01f)] private float clickFadeOutDuration = 0.08f;
        [SerializeField, Min(0.01f)] private float clickFadeInDuration = 0.1f;

        private RectTransform _rect;
        private Coroutine _scaleRoutine;
        private Coroutine _clickFxRoutine;
        private Vector3 _normalScale = Vector3.one;
        private Vector2 _normalAnchoredPos;
        private CanvasGroup _canvasGroup;
        private bool _isHovering;

        private void Awake()
        {
            _rect = transform as RectTransform;
            if (_rect != null)
            {
                _normalScale = _rect.localScale;
                _normalAnchoredPos = _rect.anchoredPosition;
            }

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (_rect == null)
                _rect = transform as RectTransform;

            if (_rect != null)
            {
                _normalScale = _rect.localScale;
                _normalAnchoredPos = _rect.anchoredPosition;
            }

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            StartScale(Vector3.one * pressedScale, pressDuration);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StartScale(GetRestScale(), releaseDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            StartScale(_normalScale, releaseDuration);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            if (enableHoverScale)
                StartScale(GetRestScale(), releaseDuration);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!enableClickSideFade || _rect == null || _canvasGroup == null)
                return;

            if (_clickFxRoutine != null)
                StopCoroutine(_clickFxRoutine);

            _clickFxRoutine = StartCoroutine(ClickSideFadeRoutine());
        }

        private void StartScale(Vector3 targetScale, float duration)
        {
            if (_rect == null)
                return;

            if (_scaleRoutine != null)
                StopCoroutine(_scaleRoutine);

            _scaleRoutine = StartCoroutine(ScaleRoutine(targetScale, duration));
        }

        private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
        {
            Vector3 start = _rect.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                _rect.localScale = Vector3.LerpUnclamped(start, targetScale, eased);
                yield return null;
            }

            _rect.localScale = targetScale;
            _scaleRoutine = null;
        }

        private IEnumerator ClickSideFadeRoutine()
        {
            _rect.anchoredPosition = _normalAnchoredPos;
            _canvasGroup.alpha = 1f;

            float dir = moveToRightOnClick ? 1f : -1f;
            Vector2 offset = new Vector2(clickMoveDistance * dir, 0f);

            float elapsed = 0f;
            while (elapsed < clickFadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / clickFadeOutDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                _rect.anchoredPosition = Vector2.LerpUnclamped(_normalAnchoredPos, _normalAnchoredPos + offset, eased);
                _canvasGroup.alpha = Mathf.LerpUnclamped(1f, 0f, eased);
                yield return null;
            }

            _rect.anchoredPosition = _normalAnchoredPos + offset;
            _canvasGroup.alpha = 0f;

            elapsed = 0f;
            while (elapsed < clickFadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / clickFadeInDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                _rect.anchoredPosition = Vector2.LerpUnclamped(_normalAnchoredPos + offset, _normalAnchoredPos, eased);
                _canvasGroup.alpha = Mathf.LerpUnclamped(0f, 1f, eased);
                yield return null;
            }

            _rect.anchoredPosition = _normalAnchoredPos;
            _canvasGroup.alpha = 1f;
            _clickFxRoutine = null;
        }

        public void SetClickSideFadeEnabled(bool enabled)
        {
            enableClickSideFade = enabled;
        }

        public void ConfigureHoverScale(bool enabled, float scale)
        {
            enableHoverScale = enabled;
            hoverScale = Mathf.Max(1f, scale);
        }

        private Vector3 GetRestScale()
        {
            if (_isHovering && enableHoverScale)
                return _normalScale * hoverScale;

            return _normalScale;
        }
    }
}

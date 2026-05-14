using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace Code.Ajs.Ux
{
    [RequireComponent(typeof(Button))]
    public class UIButtonClickSfx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickSfx;
        [SerializeField] private AudioClip cancelClickSfx;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [Header("Toggle Click Sfx")]
        [SerializeField] private bool enableToggleCancelSfx;
        [SerializeField] private bool startAsSelected;
        [Header("Hover/Press UX")]
        [SerializeField] private bool enableHoverScale = true;
        [SerializeField, Range(1f, 1.3f)] private float hoverScale = 1.08f;
        [SerializeField, Range(0.7f, 1f)] private float pressedScale = 0.9f;
        [SerializeField, Min(0.01f)] private float scaleDuration = 0.08f;

        private Button _button;
        private RectTransform _rect;
        private Vector3 _baseScale = Vector3.one;
        private Coroutine _scaleRoutine;
        private bool _isHovered;
        private bool _isSelected;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rect = transform as RectTransform;
            if (_rect != null)
                _baseScale = _rect.localScale;

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            _isSelected = startAsSelected;
        }

        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(PlayClickSfx);
        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(PlayClickSfx);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            if (enableHoverScale)
                AnimateScale(_baseScale * hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            AnimateScale(_baseScale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateScale(_baseScale * pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Vector3 target = (_isHovered && enableHoverScale) ? _baseScale * hoverScale : _baseScale;
            AnimateScale(target);
        }

        private void PlayClickSfx()
        {
            AudioClip targetClip = clickSfx;

            if (enableToggleCancelSfx)
            {
                targetClip = _isSelected ? cancelClickSfx : clickSfx;
                _isSelected = !_isSelected;
            }

            if (targetClip == null)
                return;

            if (audioSource != null)
            {
                audioSource.PlayOneShot(targetClip, volume);
                return;
            }

            AudioSource.PlayClipAtPoint(targetClip, transform.position, volume);
        }

        public void SetSelectedState(bool isSelected)
        {
            _isSelected = isSelected;
        }

        private void AnimateScale(Vector3 target)
        {
            if (_rect == null)
                return;

            if (_scaleRoutine != null)
                StopCoroutine(_scaleRoutine);

            _scaleRoutine = StartCoroutine(ScaleRoutine(target));
        }

        private IEnumerator ScaleRoutine(Vector3 target)
        {
            Vector3 start = _rect.localScale;
            float elapsed = 0f;

            while (elapsed < scaleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / scaleDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                _rect.localScale = Vector3.LerpUnclamped(start, target, eased);
                yield return null;
            }

            _rect.localScale = target;
            _scaleRoutine = null;
        }
    }
}

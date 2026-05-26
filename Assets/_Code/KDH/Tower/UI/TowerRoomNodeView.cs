using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Tower.UI
{
    [DisallowMultipleComponent]
    public sealed class TowerRoomNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [Header("Bindings")]
        [SerializeField] private Image roomIconImage;
        [SerializeField] private Image selectedRoomIcon;
        [SerializeField] private Button button;

        [Header("Hover")]
        [SerializeField, Min(1f)] private float hoverScaleMultiplier = 1.12f;
        [SerializeField, Min(0.01f)] private float hoverScaleSpeed = 14f;

        [Header("Color")]
        [SerializeField] private Color idleIconColor = new(0.58f, 0.58f, 0.58f, 1f);
        [SerializeField] private Color hoverIconColor = Color.white;
        [SerializeField] private Color selectedIconColor = Color.black;
        [SerializeField, Min(0.01f)] private float colorChangeSpeed = 14f;
        [SerializeField, Min(0.01f)] private float selectedFillSpeed = 14f;
        [SerializeField, Min(0f)] private float selectionCommitDelay = 0.22f;

        public event Action Clicked;
        public event Action<PointerEventData> PointerEntered;
        public event Action<PointerEventData> PointerMoved;
        public event Action<PointerEventData> PointerExited;

        private Vector3 _baseScale = Vector3.one;
        private bool _isHovered;
        private bool _isSelected;
        private Color _normalIconColor;
        private Coroutine _selectionRoutine;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _normalIconColor = idleIconColor;
            ResolveBindings();
            SetSelectedFillImmediate(_isSelected ? 1f : 0f);
        }

        private void OnEnable()
        {
            if (_baseScale == Vector3.zero)
                _baseScale = transform.localScale;

            _isHovered = false;
            transform.localScale = _baseScale;
            ResolveBindings();
            SetSelectedFillImmediate(_isSelected ? 1f : 0f);

            if (button != null)
                button.onClick.AddListener(HandleClick);
        }

        private void Update()
        {
            Vector3 targetScale = _isHovered ? _baseScale * hoverScaleMultiplier : _baseScale;
            float scaleT = GetSmoothStep(hoverScaleSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleT);

            if (roomIconImage != null)
            {
                Color targetColor = GetTargetIconColor();
                float colorT = GetSmoothStep(colorChangeSpeed);
                roomIconImage.color = Color.Lerp(roomIconImage.color, targetColor, colorT);
            }

            if (selectedRoomIcon != null)
            {
                float targetFillAmount = _isSelected ? 1f : 0f;
                float fillT = GetSmoothStep(selectedFillSpeed);
                selectedRoomIcon.fillAmount = Mathf.Lerp(selectedRoomIcon.fillAmount, targetFillAmount, fillT);
            }
        }

        private void OnDisable()
        {
            if (_selectionRoutine != null)
            {
                StopCoroutine(_selectionRoutine);
                _selectionRoutine = null;
            }

            _isHovered = false;
            transform.localScale = _baseScale;

            if (button != null)
                button.onClick.RemoveListener(HandleClick);
        }

        public void Apply(
            Sprite roomSprite,
            Sprite unknownSprite,
            bool isRevealed,
            bool isInteractable,
            bool isSelected)
        {
            ResolveBindings();

            Sprite iconSprite = isRevealed ? roomSprite : unknownSprite;
            if (roomIconImage != null)
            {
                roomIconImage.sprite = iconSprite;
                roomIconImage.enabled = iconSprite != null;
                roomIconImage.raycastTarget = button != null;
            }

            _normalIconColor = idleIconColor;
            _isSelected = isSelected;

            if (roomIconImage != null)
                roomIconImage.color = GetTargetIconColor();

            if (button != null)
            {
                button.transition = Selectable.Transition.None;
                button.interactable = isInteractable;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            transform.SetAsLastSibling();
            PointerEntered?.Invoke(eventData);
        }

        public void OnPointerMove(PointerEventData eventData)
            => PointerMoved?.Invoke(eventData);

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            PointerExited?.Invoke(eventData);
        }

        public void PlaySelected()
        {
            _isSelected = true;
        }

        private void HandleClick()
        {
            if (_selectionRoutine != null)
                return;

            PlaySelected();

            if (selectionCommitDelay <= 0f)
            {
                Clicked?.Invoke();
                return;
            }

            _selectionRoutine = StartCoroutine(CommitSelectionAfterDelay());
        }

        private IEnumerator CommitSelectionAfterDelay()
        {
            yield return new WaitForSecondsRealtime(selectionCommitDelay);
            _selectionRoutine = null;
            Clicked?.Invoke();
        }

        private void ResolveBindings()
        {
            if (button == null)
                button = GetComponentInChildren<Button>(true);

            if (roomIconImage == null)
                roomIconImage = FindChildImageByName(transform, "RoomIcon") ?? GetComponentInChildren<Image>(true);

            if (selectedRoomIcon == null)
                selectedRoomIcon = FindChildImageByName(transform, "RoomIcon_Selected");

            if (button != null)
            {
                button.transition = Selectable.Transition.None;

                if (roomIconImage != null)
                    button.targetGraphic = roomIconImage;
            }

            if (selectedRoomIcon != null)
            {
                selectedRoomIcon.raycastTarget = false;
                selectedRoomIcon.gameObject.SetActive(true);
            }
        }

        private Color GetTargetIconColor()
        {
            if (_isSelected)
                return selectedIconColor;

            return _isHovered ? hoverIconColor : _normalIconColor;
        }

        private static float GetSmoothStep(float speed)
        {
            return 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
        }

        private void SetSelectedFillImmediate(float fillAmount)
        {
            if (selectedRoomIcon != null)
                selectedRoomIcon.fillAmount = fillAmount;
        }

        private static Image FindChildImageByName(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
                return null;

            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child.GetComponent<Image>();

                Image found = FindChildImageByName(child, childName);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}

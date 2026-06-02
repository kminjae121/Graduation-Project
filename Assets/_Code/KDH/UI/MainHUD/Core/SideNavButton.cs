using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    [RequireComponent(typeof(Button))]
    public class SideNavButton : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] private string targetPanelId;

        [Header("Animation Targets")]
        [SerializeField] private Transform scaleTarget;
        [SerializeField] private GameObject hoverArea;

        [Header("Selection")]
        [SerializeField] private Image selectionImage;
        [SerializeField] private Color selectedColor = new(1f, 0.86f, 0.15f, 1f);

        [Header("Hover Animation Settings")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Vector3 hoverScale = new(1.1f, 1.1f, 1.1f);
        [SerializeField] private Ease animationEase = Ease.OutCubic;

        private static readonly List<SideNavButton> Instances = new();

        private Button _navButton;
        private Vector3 _originalScale;
        private Color _originalSelectionColor = Color.white;
        private Tween _scaleTween;
        private HoverDetector _detector;
        private bool _isSelected;

        private void Awake()
        {
            _navButton = GetComponent<Button>();
            Instances.Add(this);

            if (scaleTarget == null)
                scaleTarget = transform;

            _originalScale = scaleTarget.localScale;

            ResolveSelectionImage();
            CacheOriginalSelectionColor();

            _navButton.onClick.AddListener(HandleNavButtonClick);

            GameObject targetHoverObj = hoverArea != null ? hoverArea : gameObject;
            _detector = targetHoverObj.GetComponent<HoverDetector>();

            if (_detector == null)
                _detector = targetHoverObj.AddComponent<HoverDetector>();

            _detector.OnEnter += PlayHoverEnter;
            _detector.OnExit += PlayHoverExit;
        }

        private void OnDestroy()
        {
            SetSelected(false);
            Instances.Remove(this);

            _navButton.onClick.RemoveListener(HandleNavButtonClick);

            if (_detector != null)
            {
                _detector.OnEnter -= PlayHoverEnter;
                _detector.OnExit -= PlayHoverExit;
            }

            _scaleTween?.Kill();
        }

        private void LateUpdate()
        {
            if (_isSelected && selectionImage != null)
                selectionImage.color = GetSelectedColor();
        }

        private void PlayHoverEnter()
        {
            _scaleTween?.Kill();
            _scaleTween = scaleTarget.DOScale(hoverScale, animationDuration).SetEase(animationEase);
        }

        private void PlayHoverExit()
        {
            _scaleTween?.Kill();
            _scaleTween = scaleTarget.DOScale(_originalScale, animationDuration).SetEase(animationEase);
        }

        private void HandleNavButtonClick()
        {
            if (string.IsNullOrEmpty(targetPanelId))
            {
                Debug.LogError("대상 패널 ID가 설정되지 않았습니다.");
                return;
            }

            if (TryOpenPanelOrTab(targetPanelId))
                SetSelectedPanel(targetPanelId);
        }

        private static bool TryOpenPanelOrTab(string id)
        {
            return MainPanel.TryOpenTab(id) || PanelManager.TryOpen(id);
        }

        public static void SetSelectedPanel(string panelId)
        {
            foreach (SideNavButton button in Instances)
            {
                if (button == null)
                    continue;

                bool shouldSelect = string.Equals(button.targetPanelId, panelId, System.StringComparison.OrdinalIgnoreCase);
                button.SetSelected(shouldSelect);
            }
        }

        public static void ClearSelection()
        {
            foreach (SideNavButton button in Instances)
                if (button != null)
                    button.SetSelected(false);
        }

        private void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;

            if (selectionImage == null)
                return;

            selectionImage.color = isSelected ? GetSelectedColor() : _originalSelectionColor;
        }

        private void ResolveSelectionImage()
        {
            if (selectionImage != null)
                return;

            if (_navButton != null && _navButton.targetGraphic is Image targetImage)
            {
                selectionImage = targetImage;
                return;
            }

            selectionImage = GetComponentInChildren<Image>(true);
        }

        private void CacheOriginalSelectionColor()
        {
            if (selectionImage != null)
                _originalSelectionColor = selectionImage.color;
        }

        private Color GetSelectedColor()
        {
            return selectedColor.a > 0f ? selectedColor : new Color(1f, 0.86f, 0.15f, 1f);
        }
    }
}

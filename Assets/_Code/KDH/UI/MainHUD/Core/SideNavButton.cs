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
        [SerializeField] private Color selectedColor = new(1f, 0.7411765f, 0.1490196f, 1f);

        [Header("Hover Animation Settings")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Vector3 hoverScale = new(1.1f, 1.1f, 1.1f);
        [SerializeField] private Ease animationEase = Ease.OutCubic;

        private Button _navButton;
        private Vector3 _originalScale;
        private Color _originalSelectionColor = Color.white;
        private Tween _scaleTween;
        private HoverDetector _detector;

        private void Awake()
        {
            _navButton = GetComponent<Button>();

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
            RefreshSelectionColor();
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

            TryOpenPanelOrTab(targetPanelId);
        }

        private static bool TryOpenPanelOrTab(string id)
        {
            return MainPanel.TryOpenTab(id) || PanelManager.TryOpen(id);
        }

        private void RefreshSelectionColor()
        {
            if (selectionImage == null)
                return;

            selectionImage.color = IsTargetPanelVisible() ? GetSelectedColor() : _originalSelectionColor;
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
            return selectedColor.a > 0f ? selectedColor : new Color(1f, 0.7411765f, 0.1490196f, 1f);
        }

        private bool IsTargetPanelVisible()
        {
            return MainPanel.IsTabVisible(targetPanelId) || PanelManager.IsOpen(targetPanelId);
        }
    }
}

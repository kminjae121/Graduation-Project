using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    [RequireComponent(typeof(Button))]
    public class SideNavButton : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] private string targetPanelId;
        [SerializeField] private List<string> panelsToClose;

        [Header("Animation Targets")]
        [SerializeField] private Transform scaleTarget;
        [SerializeField] private CanvasGroup panelNameCanvasGroup;
        [SerializeField] private GameObject hoverArea;

        [Header("Hover Animation Settings")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
        [SerializeField] private Ease animationEase = Ease.OutCubic;
        
        private Button _navButton;
        private Vector3 _originalScale;
        private Tween _scaleTween;
        private Tween _fadeTween;
        private HoverDetector _detector;

        private void Awake()
        {
            _navButton = GetComponent<Button>();
            
            if (scaleTarget == null)
                scaleTarget = transform;
                
            _originalScale = scaleTarget.localScale;

            if (panelNameCanvasGroup != null)
            {
                panelNameCanvasGroup.alpha = 0f;
                Canvas textCanvas = panelNameCanvasGroup.GetComponent<Canvas>();
                if (textCanvas == null)
                {
                    textCanvas = panelNameCanvasGroup.gameObject.AddComponent<Canvas>();
                }
                textCanvas.overrideSorting = true;
                textCanvas.sortingOrder = 100;
            }

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
            _fadeTween?.Kill();
        }

        private void PlayHoverEnter()
        {
            _scaleTween?.Kill();
            _scaleTween = scaleTarget.DOScale(hoverScale, animationDuration).SetEase(animationEase);

            if (panelNameCanvasGroup != null)
            {
                _fadeTween?.Kill();
                _fadeTween = panelNameCanvasGroup.DOFade(1f, animationDuration).SetEase(animationEase);
            }
        }

        private void PlayHoverExit()
        {
            _scaleTween?.Kill();
            _scaleTween = scaleTarget.DOScale(_originalScale, animationDuration).SetEase(animationEase);

            if (panelNameCanvasGroup != null)
            {
                _fadeTween?.Kill();
                _fadeTween = panelNameCanvasGroup.DOFade(0f, animationDuration).SetEase(animationEase);
            }
        }

        private void HandleNavButtonClick()
        {
            if (string.IsNullOrEmpty(targetPanelId))
            {
                Debug.LogError("대상 패널 ID가 설정되지 않았습니다.");
                return;
            }

            if (panelsToClose != null)
            {
                foreach (var id in panelsToClose)
                {
                    if (string.IsNullOrEmpty(id) == false)
                        PanelManager.Close(id);
                }
            }

            PanelManager.Open(targetPanelId);
        }
    }
}
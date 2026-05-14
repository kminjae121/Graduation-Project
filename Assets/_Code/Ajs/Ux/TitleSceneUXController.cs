using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Code.Ajs.Ux
{
    public class TitleSceneUXController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private GameObject playButPanel;
        [SerializeField] private Transform buttonSearchRoot;
        [SerializeField] private Button startButton;
        [SerializeField] private Button optionButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject optionPanel;
        
        [Header("Menu Motion")]
        [SerializeField] private bool animateMenuButtonsOnOpen = true;
        [SerializeField] private bool autoCollectMenuButtons = true;
        [SerializeField] private List<RectTransform> menuButtonRects = new List<RectTransform>();
        [SerializeField, Min(0f)] private float buttonStartOffsetX = 500f;
        [SerializeField, Min(0.01f)] private float buttonMoveDuration = 0.25f;
        [SerializeField, Min(0f)] private float buttonStaggerDelay = 0.06f;
        [SerializeField, Range(0.5f, 1.2f)] private float buttonStartScale = 0.95f;
        
        [Header("Per Button UX")]
        [SerializeField] private bool sideFadeOnlyOnStartButton = true;
        [SerializeField] private bool enableHoverScaleOnButtons = true;
        [SerializeField, Range(1f, 1.3f)] private float hoverScaleOnButtons = 1.08f;

        [Header("Scene")]
        [SerializeField] private string startSceneName;
        
        [Header("Start Transition Panel")]
        [SerializeField] private GameObject startTransitionPanel;
        [SerializeField] private bool useStartTransitionPanel = true;
        [SerializeField, Min(0.01f)] private float startTransitionDuration = 0.2f;
        [SerializeField, Min(0f)] private float startTransitionSlideX = 120f;

        [Header("Behavior")]
        [SerializeField] private bool hideMenuOnStart = true;

        [Header("Audio")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip defaultClickSfx;
        [SerializeField] private AudioClip menuOpenSfx;
        [SerializeField] private AudioClip startClickSfx;
        [SerializeField] private AudioClip optionClickSfx;
        [SerializeField] private AudioClip quitClickSfx;
        [SerializeField] private bool waitForClickSfxBeforeAction;
        [SerializeField, Min(0f)] private float maxWaitSeconds = 0.2f;

        private bool _menuOpened;
        private bool _isProcessingClick;
        private CanvasGroup _selfCanvasGroup;
        private bool _useSelfCanvasGroupHide;
        private readonly Dictionary<RectTransform, Vector2> _originalAnchoredPositions = new Dictionary<RectTransform, Vector2>();
        private CanvasGroup _startTransitionCanvasGroup;
        private RectTransform _startTransitionRect;
        private Vector2 _startTransitionOriginalPos;

        private void Awake()
        {
            SetupButtonUX();
            CacheButtonPositions();
            SetupStartTransitionPanel();

            if (hideMenuOnStart && menuRoot != null)
            {
                if (menuRoot == gameObject)
                {
                    // If we disable the same object this script is on, Update won't run.
                    _selfCanvasGroup = GetComponent<CanvasGroup>();
                    if (_selfCanvasGroup == null)
                        _selfCanvasGroup = gameObject.AddComponent<CanvasGroup>();

                    _selfCanvasGroup.alpha = 0f;
                    _selfCanvasGroup.interactable = false;
                    _selfCanvasGroup.blocksRaycasts = false;
                    _useSelfCanvasGroupHide = true;
                }
                else
                {
                    menuRoot.SetActive(false);
                }
            }
        }

        private void OnEnable()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnClickStart);

            if (optionButton != null)
                optionButton.onClick.AddListener(OnClickOption);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnClickQuit);
        }

        private void OnDisable()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(OnClickStart);

            if (optionButton != null)
                optionButton.onClick.RemoveListener(OnClickOption);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnClickQuit);
        }

        private void Update()
        {
            if (_menuOpened)
                return;

            if (IsAnyInputPressed())
                OpenMenu();
        }

        private void OpenMenu()
        {
            _menuOpened = true;
            PlayMenuOpenSfx();

            if (playButPanel != null)
                playButPanel.SetActive(false);

            if (menuRoot != null)
            {
                if (_useSelfCanvasGroupHide && menuRoot == gameObject && _selfCanvasGroup != null)
                {
                    _selfCanvasGroup.alpha = 1f;
                    _selfCanvasGroup.interactable = true;
                    _selfCanvasGroup.blocksRaycasts = true;
                }
                else
                {
                    menuRoot.SetActive(true);
                }
            }

            if (animateMenuButtonsOnOpen)
                StartCoroutine(AnimateMenuButtonsRoutine());
        }

        private bool IsAnyInputPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                return true;

            if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
                return true;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                return true;

            return false;
#else
            return Input.anyKeyDown;
#endif
        }

        private void OnClickStart()
        {
            if (_isProcessingClick)
                return;

            StartCoroutine(HandleStartClickRoutine());
        }

        private IEnumerator HandleStartClickRoutine()
        {
            _isProcessingClick = true;
            yield return PlayClickAndOptionallyWait(startClickSfx);
            yield return PlayStartTransitionPanelRoutine();

            if (string.IsNullOrWhiteSpace(startSceneName))
            {
                Debug.LogWarning("[TitleSceneUXController] Start Scene Name is empty.");
                _isProcessingClick = false;
                yield break;
            }

            SceneManager.LoadScene(startSceneName);
        }

        private void OnClickOption()
        {
            if (_isProcessingClick)
                return;

            StartCoroutine(HandleOptionClickRoutine());
        }

        private IEnumerator HandleOptionClickRoutine()
        {
            _isProcessingClick = true;
            yield return PlayClickAndOptionallyWait(optionClickSfx);

            if (optionPanel == null)
            {
                _isProcessingClick = false;
                yield break;
            }

            optionPanel.SetActive(!optionPanel.activeSelf);
            _isProcessingClick = false;
        }

        private void OnClickQuit()
        {
            if (_isProcessingClick)
                return;

            StartCoroutine(HandleQuitClickRoutine());
        }

        private IEnumerator HandleQuitClickRoutine()
        {
            _isProcessingClick = true;
            yield return PlayClickAndOptionallyWait(quitClickSfx);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator PlayClickAndOptionallyWait(AudioClip specificClip)
        {
            AudioClip clipToPlay = specificClip != null ? specificClip : defaultClickSfx;

            if (uiAudioSource == null)
                uiAudioSource = GetComponent<AudioSource>();

            if (uiAudioSource != null && clipToPlay != null)
            {
                uiAudioSource.PlayOneShot(clipToPlay);

                if (waitForClickSfxBeforeAction)
                    yield return new WaitForSecondsRealtime(Mathf.Min(clipToPlay.length, maxWaitSeconds));
            }
        }

        private void PlayMenuOpenSfx()
        {
            AudioClip clipToPlay = menuOpenSfx != null ? menuOpenSfx : defaultClickSfx;

            if (uiAudioSource == null)
                uiAudioSource = GetComponent<AudioSource>();

            if (uiAudioSource != null && clipToPlay != null)
                uiAudioSource.PlayOneShot(clipToPlay);
        }

        private void SetupButtonUX()
        {
            Transform searchRoot = buttonSearchRoot != null ? buttonSearchRoot : transform;
            Button[] buttons = searchRoot.GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                if (button == null)
                    continue;

                UIButtonPressFeedback feedback = button.GetComponent<UIButtonPressFeedback>();
                if (feedback == null)
                    feedback = button.gameObject.AddComponent<UIButtonPressFeedback>();

                if (feedback != null)
                {
                    bool enableSideFade = !sideFadeOnlyOnStartButton || button == startButton;
                    feedback.SetClickSideFadeEnabled(enableSideFade);
                    feedback.ConfigureHoverScale(enableHoverScaleOnButtons, hoverScaleOnButtons);
                }
            }
        }

        private void CacheButtonPositions()
        {
            _originalAnchoredPositions.Clear();

            if (autoCollectMenuButtons && menuRoot != null)
            {
                Button[] menuButtons = menuRoot.GetComponentsInChildren<Button>(true);
                foreach (Button button in menuButtons)
                {
                    if (button == null)
                        continue;

                    RectTransform rect = button.GetComponent<RectTransform>();
                    if (rect != null && !menuButtonRects.Contains(rect))
                        menuButtonRects.Add(rect);
                }
            }

            foreach (RectTransform rect in menuButtonRects)
            {
                if (rect == null)
                    continue;

                _originalAnchoredPositions[rect] = rect.anchoredPosition;
            }
        }

        private IEnumerator AnimateMenuButtonsRoutine()
        {
            if (menuButtonRects == null || menuButtonRects.Count == 0)
                yield break;

            foreach (RectTransform rect in menuButtonRects)
            {
                if (rect == null)
                    continue;

                if (!_originalAnchoredPositions.TryGetValue(rect, out Vector2 originalPos))
                {
                    originalPos = rect.anchoredPosition;
                    _originalAnchoredPositions[rect] = originalPos;
                }

                CanvasGroup group = rect.GetComponent<CanvasGroup>();
                if (group == null)
                    group = rect.gameObject.AddComponent<CanvasGroup>();

                rect.anchoredPosition = originalPos + Vector2.left * buttonStartOffsetX;
                rect.localScale = Vector3.one * buttonStartScale;
                group.alpha = 0f;
            }

            for (int i = 0; i < menuButtonRects.Count; i++)
            {
                RectTransform rect = menuButtonRects[i];
                if (rect == null)
                    continue;

                CanvasGroup group = rect.GetComponent<CanvasGroup>();
                if (group == null)
                    continue;

                Vector2 targetPos = _originalAnchoredPositions[rect];
                StartCoroutine(AnimateSingleButton(rect, group, targetPos, i * buttonStaggerDelay));
            }

            yield break;
        }

        private IEnumerator AnimateSingleButton(RectTransform rect, CanvasGroup group, Vector2 targetPos, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);

            Vector2 startPos = rect.anchoredPosition;
            Vector3 startScale = rect.localScale;

            float elapsed = 0f;
            while (elapsed < buttonMoveDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / buttonMoveDuration);
                float eased = EaseOutCubic(t);

                rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, eased);
                rect.localScale = Vector3.LerpUnclamped(startScale, Vector3.one, eased);
                group.alpha = Mathf.LerpUnclamped(0f, 1f, eased);

                yield return null;
            }

            rect.anchoredPosition = targetPos;
            rect.localScale = Vector3.one;
            group.alpha = 1f;
        }

        private static float EaseOutCubic(float t)
        {
            float inv = 1f - t;
            return 1f - inv * inv * inv;
        }

        private void SetupStartTransitionPanel()
        {
            if (startTransitionPanel == null)
                return;

            _startTransitionCanvasGroup = startTransitionPanel.GetComponent<CanvasGroup>();
            if (_startTransitionCanvasGroup == null)
                _startTransitionCanvasGroup = startTransitionPanel.AddComponent<CanvasGroup>();

            _startTransitionRect = startTransitionPanel.GetComponent<RectTransform>();
            if (_startTransitionRect != null)
                _startTransitionOriginalPos = _startTransitionRect.anchoredPosition;

            _startTransitionCanvasGroup.alpha = 0f;
            startTransitionPanel.SetActive(false);
        }

        private IEnumerator PlayStartTransitionPanelRoutine()
        {
            if (!useStartTransitionPanel || startTransitionPanel == null)
                yield break;

            if (_startTransitionCanvasGroup == null)
                SetupStartTransitionPanel();

            startTransitionPanel.SetActive(true);

            if (_startTransitionCanvasGroup == null)
                yield break;

            if (_startTransitionRect != null)
                _startTransitionRect.anchoredPosition = _startTransitionOriginalPos + new Vector2(startTransitionSlideX, 0f);

            _startTransitionCanvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < startTransitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / startTransitionDuration);
                float eased = EaseOutCubic(t);

                _startTransitionCanvasGroup.alpha = eased;

                if (_startTransitionRect != null)
                {
                    _startTransitionRect.anchoredPosition = Vector2.LerpUnclamped(
                        _startTransitionOriginalPos + new Vector2(startTransitionSlideX, 0f),
                        _startTransitionOriginalPos,
                        eased);
                }

                yield return null;
            }

            _startTransitionCanvasGroup.alpha = 1f;
            if (_startTransitionRect != null)
                _startTransitionRect.anchoredPosition = _startTransitionOriginalPos;
        }
    }
}

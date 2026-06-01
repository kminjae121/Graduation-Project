namespace PixeLadder.EasyTransition
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// A singleton manager that controls the entire scene transition process.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneTransitioner : MonoBehaviour
    {
        public static SceneTransitioner Instance;

        [Header("Configuration")]
        [Tooltip("The screen-covering Image prefab used for transitions.")]
        [SerializeField] private Image transitionImagePrefab;

        [Tooltip("The default transition effect to use if none is provided in the LoadScene call.")]
        [SerializeField] private TransitionEffect defaultTransition;

        [Header("Loading Screen")]
        [SerializeField] private GameObject loadingScreenPrefab;

        // --- Private State ---
        private Image transitionImageInstance;
        private GameObject loadingScreenInstance;
        private Slider loadingSlider;
        private bool isTransitioning = false;

        // Cache shader property ID for performance
        private static readonly int RectSizeID = Shader.PropertyToID("_RectSize");

        public static event System.Action OnSceneLoaded;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            // Create a dedicated, persistent canvas for the transition UI.
            GameObject canvasGO = new GameObject("TransitionCanvas");
            canvasGO.transform.SetParent(this.transform);

            var transitionCanvas = canvasGO.AddComponent<Canvas>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.sortingOrder = 999;

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            transitionImageInstance = Instantiate(transitionImagePrefab, canvasGO.transform);

            RectTransform rectT = transitionImageInstance.rectTransform;
            rectT.anchorMin = Vector2.zero;
            rectT.anchorMax = Vector2.one;
            rectT.sizeDelta = Vector2.zero;
            rectT.anchoredPosition = Vector2.zero;

            transitionImageInstance.gameObject.SetActive(false);

            if (loadingScreenPrefab != null)
            {
                loadingScreenInstance = Instantiate(loadingScreenPrefab, canvasGO.transform);
                loadingSlider = loadingScreenInstance.GetComponentInChildren<Slider>();
                loadingScreenInstance.SetActive(false);
            }
        }

        /// <summary>
        /// The main public method to start a scene transition.
        /// </summary>
        public void LoadScene(string sceneName, TransitionEffect effect = null)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("SceneTransitioner: Transition already in progress.");
                return;
            }

            var effectToUse = effect ?? defaultTransition;
            if (effectToUse == null)
            {
                Debug.LogError("SceneTransitioner: No transition effect specified and no default is set.", this);
                return;
            }

            StartCoroutine(TransitionRoutine(sceneName, effectToUse));
        }

        public void DoTransition(System.Action midTransitionAction, System.Action onCompleteAction = null, TransitionEffect effect = null)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("SceneTransitioner: Transition already in progress.");
                return;
            }

            var effectToUse = effect ?? defaultTransition;
            if (effectToUse == null)
            {
                Debug.LogError("SceneTransitioner: No transition effect specified and no default is set.", this);
                return;
            }

            StartCoroutine(ActionTransitionRoutine(midTransitionAction, onCompleteAction, effectToUse));
        }

        private IEnumerator ActionTransitionRoutine(System.Action midTransitionAction, System.Action onCompleteAction, TransitionEffect effect)
        {
            isTransitioning = true;
            transitionImageInstance.gameObject.SetActive(true);

            Material materialInstance = new Material(effect.transitionMaterial);
            Rect rect = transitionImageInstance.rectTransform.rect;
            materialInstance.SetVector(RectSizeID, new Vector4(rect.width, rect.height, 0, 0));
            effect.SetEffectProperties(materialInstance);
            transitionImageInstance.material = materialInstance;
            
            yield return effect.AnimateOut(transitionImageInstance);

            midTransitionAction?.Invoke();

            yield return effect.AnimateIn(transitionImageInstance);

            transitionImageInstance.gameObject.SetActive(false);
            Destroy(materialInstance);
            isTransitioning = false;

            onCompleteAction?.Invoke();
        }

        private IEnumerator TransitionRoutine(string sceneName, TransitionEffect effect)
        {
            isTransitioning = true;
            transitionImageInstance.gameObject.SetActive(true);

            // 1. Create a fresh instance of the material for this specific transition
            Material materialInstance = new Material(effect.transitionMaterial);

            // 2. CRITICAL FIX: Pass the RectSize (Aspect Ratio) to the shader immediately
            Rect rect = transitionImageInstance.rectTransform.rect;
            materialInstance.SetVector(RectSizeID, new Vector4(rect.width, rect.height, 0, 0));

            // 3. Apply custom effect properties
            effect.SetEffectProperties(materialInstance);
            transitionImageInstance.material = materialInstance;

            // 4. Run the fade-out animation (화면이 트랜지션으로 덮임)
            yield return effect.AnimateOut(transitionImageInstance);

            // 5. 로딩 스크린 활성화 및 비동기 씬 로드
            if (loadingScreenInstance != null)
            {
                loadingScreenInstance.SetActive(true);
                if (loadingSlider != null) loadingSlider.value = 0f;
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false; // 진행도가 0.9에서 멈추도록 설정 (씬 자동 전환 방지)

            while (!asyncLoad.isDone)
            {
                // progress는 0 ~ 0.9까지만 올라가므로 이를 0 ~ 1.0 비율로 변환.
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                
                if (loadingSlider != null)
                {
                    //loadingSlider.value = Mathf.Lerp(loadingSlider.value, progress, Time.deltaTime * 5f);
                    loadingSlider.value = progress;
                }

                // 로딩이 완료되었을 때 (progress == 0.9)
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            if (loadingScreenInstance != null)
            {
                loadingScreenInstance.SetActive(false);
            }

            // Fire event
            OnSceneLoaded?.Invoke();

            // 6. Run the fade-in animation
            yield return effect.AnimateIn(transitionImageInstance);

            // Cleanup
            transitionImageInstance.gameObject.SetActive(false);
            Destroy(materialInstance); // Clean up the material instance to prevent leaks
            isTransitioning = false;
        }
    }
}
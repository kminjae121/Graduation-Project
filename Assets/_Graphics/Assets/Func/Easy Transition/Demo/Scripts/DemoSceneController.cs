namespace PixeLadder.EasyTransition.Demo
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using TMPro;
    using PixeLadder.EasyTransition;

    /// <summary>
    /// Manages the logic for the self-contained demo scene.
    /// </summary>
    public class DemoSceneController : MonoBehaviour
    {
        #region Fields
        [Header("Scene References")]
        [SerializeField] private TMP_Text sceneNameText;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private Button transitionButton;

        [Header("Demo Configuration")]
        [SerializeField] private TransitionEffect[] transitionEffects;

        // Persist index across reloads
        private static int currentSceneIndex = 0;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            SceneTransitioner.OnSceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneTransitioner.OnSceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            InitializeUI();

            if (transitionButton != null)
                transitionButton.onClick.AddListener(OnTransitionButtonClicked);
        }

        private void OnDestroy()
        {
            if (transitionButton != null)
                transitionButton.onClick.RemoveListener(OnTransitionButtonClicked);
        }
        #endregion

        #region Logic
        private void OnTransitionButtonClicked()
        {
            if (transitionEffects == null || transitionEffects.Length == 0)
            {
                Debug.LogWarning("No transition effects assigned in inspector.");
                return;
            }

            string sceneToLoad = SceneManager.GetActiveScene().name;
            TransitionEffect effectToUse = transitionEffects[currentSceneIndex];

            SceneTransitioner.Instance.LoadScene(sceneToLoad, effectToUse);
        }

        private void HandleSceneLoaded()
        {
            if (transitionEffects != null && transitionEffects.Length > 0)
            {
                currentSceneIndex = (currentSceneIndex + 1) % transitionEffects.Length;
            }
        }

        private void InitializeUI()
        {
            if (transitionEffects == null || transitionEffects.Length == 0)
            {
                if (sceneNameText) sceneNameText.text = "Error";
                if (buttonText) buttonText.text = "No Effects";
                return;
            }

            // Current scene index logic
            int displayIndex = currentSceneIndex + 1;
            string effectName = transitionEffects[currentSceneIndex].name;

            if (sceneNameText)
                sceneNameText.text = $"Scene {displayIndex}";

            if (buttonText)
                buttonText.text = $"Load Next (Effect: {effectName})";

            if (transitionButton)
            {
                // Force layout rebuild to accommodate text width changes
                LayoutRebuilder.ForceRebuildLayoutImmediate(transitionButton.GetComponent<RectTransform>());
            }
        }
        #endregion
    }
}
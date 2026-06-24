using System.Collections;
using Code.Core.Managers;
using PixeLadder.EasyTransition;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.Ajs.Ux
{
    public class TitleSceneUXController : MonoBehaviour
    {
        [Header("Scene")]
        [FormerlySerializedAs("startSceneName")]
        [SerializeField] private string targetSceneName = "StartScene";

        [Header("Play Button")]
        [FormerlySerializedAs("startButton")]
        [SerializeField] private Button playButton;
        [SerializeField] private Transform playButtonScaleTarget;

        [Header("Hover")]
        [SerializeField, Min(1f)] private float hoverScale = 1.08f;
        [SerializeField, Min(0.01f)] private float hoverDuration = 0.18f;

        private Transform _scaleTarget;
        private Vector3 _originalScale;
        private Coroutine _scaleRoutine;
        private EventTrigger _playButtonEventTrigger;
        private EventTrigger.Entry _pointerEnterEntry;
        private EventTrigger.Entry _pointerExitEntry;
        private bool _isLoadingScene;

        private void Awake()
        {
            CacheScaleTarget();
        }

        private void OnEnable()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(LoadTargetScene);
                RegisterHoverEvents();
            }
        }

        private void OnDisable()
        {
            if (playButton != null)
                playButton.onClick.RemoveListener(LoadTargetScene);

            UnregisterHoverEvents();
            StopScaleRoutine();
            ResetScale();
        }

        private void CacheScaleTarget()
        {
            _scaleTarget = playButtonScaleTarget != null ? playButtonScaleTarget : playButton != null ? playButton.transform : null;

            if (_scaleTarget != null)
                _originalScale = _scaleTarget.localScale;
        }

        private void RegisterHoverEvents()
        {
            if (playButton == null)
                return;

            _playButtonEventTrigger = playButton.GetComponent<EventTrigger>();
            if (_playButtonEventTrigger == null)
                _playButtonEventTrigger = playButton.gameObject.AddComponent<EventTrigger>();

            _pointerEnterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            _pointerEnterEntry.callback.AddListener(OnPlayButtonPointerEnter);
            _playButtonEventTrigger.triggers.Add(_pointerEnterEntry);

            _pointerExitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            _pointerExitEntry.callback.AddListener(OnPlayButtonPointerExit);
            _playButtonEventTrigger.triggers.Add(_pointerExitEntry);
        }

        private void UnregisterHoverEvents()
        {
            if (_playButtonEventTrigger == null)
                return;

            if (_pointerEnterEntry != null)
                _playButtonEventTrigger.triggers.Remove(_pointerEnterEntry);

            if (_pointerExitEntry != null)
                _playButtonEventTrigger.triggers.Remove(_pointerExitEntry);

            _pointerEnterEntry = null;
            _pointerExitEntry = null;
            _playButtonEventTrigger = null;
        }

        private void OnPlayButtonPointerEnter(BaseEventData eventData)
        {
            if (_scaleTarget == null)
                CacheScaleTarget();

            if (_scaleTarget == null)
                return;

            StartScaleRoutine(_originalScale * hoverScale);
        }

        private void OnPlayButtonPointerExit(BaseEventData eventData)
        {
            if (_scaleTarget == null)
                return;

            StartScaleRoutine(_originalScale);
        }

        private void LoadTargetScene()
        {
            if (_isLoadingScene)
                return;

            if (string.IsNullOrWhiteSpace(targetSceneName))
            {
                Debug.LogWarning("이동할 씬 이름이 비어 있습니다.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                Debug.LogError($"씬을 로드할 수 없습니다: {targetSceneName}. Build Settings에 씬이 등록되어 있는지 확인해주세요.");
                return;
            }

            _isLoadingScene = true;

            SceneChangeManager sceneChangeManager = FindAnyObjectByType<SceneChangeManager>();
            if (sceneChangeManager != null && sceneChangeManager.TryChangeSelectScene(targetSceneName, false))
                return;

            if (SceneTransitioner.Instance != null)
            {
                SceneTransitioner.Instance.LoadScene(targetSceneName);
                return;
            }

            Debug.LogWarning("씬 전환 매니저가 없어 기본 씬 로드를 사용합니다.");
            SceneManager.LoadScene(targetSceneName);
        }

        private void StartScaleRoutine(Vector3 targetScale)
        {
            StopScaleRoutine();
            _scaleRoutine = StartCoroutine(ScaleRoutine(targetScale));
        }

        private void StopScaleRoutine()
        {
            if (_scaleRoutine == null)
                return;

            StopCoroutine(_scaleRoutine);
            _scaleRoutine = null;
        }

        private IEnumerator ScaleRoutine(Vector3 targetScale)
        {
            Vector3 startScale = _scaleTarget.localScale;
            float elapsed = 0f;

            while (elapsed < hoverDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / hoverDuration);
                float eased = EaseOutCubic(t);
                _scaleTarget.localScale = Vector3.LerpUnclamped(startScale, targetScale, eased);
                yield return null;
            }

            _scaleTarget.localScale = targetScale;
            _scaleRoutine = null;
        }

        private void ResetScale()
        {
            if (_scaleTarget != null)
                _scaleTarget.localScale = _originalScale;
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1f - value;
            return 1f - inverse * inverse * inverse;
        }
    }
}

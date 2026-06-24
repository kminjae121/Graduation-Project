    using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixeLadder.EasyTransition; 

namespace Code.Core.Managers
{
    public class SceneChangeManager : MonoSingleton<SceneChangeManager>
    {
        [SerializeField] private List<string> scenesName;
        
        [Header("Transition Effect")]
        [SerializeField] private TransitionEffect defaultTransitionEffect;

        private int _currentSceneIdx;

        public void ChangeSceneIdx(int idx)
        {
            if (idx < 0 || idx >= scenesName.Count)
            {
                Debug.LogError($"씬 인덱스 오류: {idx}는 유효하지 않습니다.");
                return;
            }

            LoadSceneWithTransition(scenesName[idx]);
            _currentSceneIdx = idx;
        }

        public void ChangeNextScene()
        {
            int nextIdx = _currentSceneIdx + 1;
            if (nextIdx >= scenesName.Count)
            {
                Debug.LogWarning("마지막 씬입니다.");
                return;
            }

            _currentSceneIdx = nextIdx;
            LoadSceneWithTransition(scenesName[_currentSceneIdx]);
        }

        public void ChangeSelectScene(string sceneName)
        {
            TryChangeSelectScene(sceneName);
        }

        public bool TryChangeSelectScene(string sceneName, bool logWhenMissing = true)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                if (logWhenMissing)
                    Debug.LogError("씬 이름이 비어 있습니다.");

                return false;
            }

            if (scenesName == null || !scenesName.Contains(sceneName))
            {
                if (logWhenMissing)
                    Debug.LogError($"씬 목록에 없는 씬입니다: {sceneName}");

                return false;
            }
            
            int sceneIdx = scenesName.IndexOf(sceneName);
            _currentSceneIdx = sceneIdx;
            
            LoadSceneWithTransition(sceneName);
            return true;
        }

        private void LoadSceneWithTransition(string sceneName)
        {
            if (SceneTransitioner.Instance != null)
            {
                SceneTransitioner.Instance.LoadScene(sceneName, defaultTransitionEffect);
            }
            else
            {
                Debug.LogWarning("SceneTransitioner가 없습니다. 기본 로드를 사용합니다.");
                SceneManager.LoadScene(sceneName);
            }
        }
    }   
}

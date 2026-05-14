using System;
using PixeLadder.EasyTransition;
using UnityEngine.SceneManagement;

namespace Code.Tower
{
    public static class TowerSceneLoader
    {
        public static void LoadScene(string sceneName, TransitionEffect transitionEffect = null)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            if (SceneTransitioner.Instance != null)
            {
                SceneTransitioner.Instance.LoadScene(sceneName, transitionEffect);
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        public static void DoTransition(Action midTransitionAction, TransitionEffect transitionEffect = null)
        {
            if (SceneTransitioner.Instance != null)
            {
                SceneTransitioner.Instance.DoTransition(midTransitionAction, null, transitionEffect);
                return;
            }

            midTransitionAction?.Invoke();
        }
    }
}

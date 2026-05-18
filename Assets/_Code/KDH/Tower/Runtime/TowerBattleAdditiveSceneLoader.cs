using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Tower
{
    public class TowerBattleAdditiveSceneLoader : MonoBehaviour
    {
        [SerializeField] private string additiveSceneName = "BattleUI";
        [SerializeField] private bool loadOnStart = true;

        private IEnumerator Start()
        {
            if (!loadOnStart || string.IsNullOrWhiteSpace(additiveSceneName))
                yield break;

            if (SceneManager.GetSceneByName(additiveSceneName).isLoaded)
                yield break;

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(additiveSceneName, LoadSceneMode.Additive);

            while (loadOperation is { isDone: false })
                yield return null;
        }
    }
}

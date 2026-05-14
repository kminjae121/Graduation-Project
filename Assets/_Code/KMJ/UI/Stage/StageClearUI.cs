using Code.Tower;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class StageClearUI : MonoBehaviour
    {
        [SerializeField] private Button returnBtn;

        private void Awake()
        {
            returnBtn.onClick.AddListener(ReturnHome);
        }

        private void OnDisable()
        {
            returnBtn.onClick.RemoveListener(ReturnHome);
        }

        public void ReturnHome()
        {
            Time.timeScale = 1;
            TowerRunSession.CompleteCurrentRoom();
            TowerSceneLoader.LoadScene(TowerRunSession.TowerSceneName);
        }
    }
}

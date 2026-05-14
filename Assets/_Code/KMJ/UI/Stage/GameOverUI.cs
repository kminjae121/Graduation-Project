using _Code.UnitSystem;
using Code.Tower;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private Button restartBtn;
        [SerializeField] private Button returnBtn;

        private void Awake()
        {
            restartBtn.onClick.AddListener(RestartBtn);
            returnBtn.onClick.AddListener(ReturnHome);
        }

        private void OnDisable()
        {
            restartBtn.onClick.RemoveListener(RestartBtn);
            returnBtn.onClick.RemoveListener(ReturnHome);
        }

        public void ReturnHome()
        {
            string lobbySceneName = TowerRunSession.LobbySceneName;
            TowerRunSession.EndRun();
            TowerSceneLoader.LoadScene(lobbySceneName);
            InGameStatCompo.Instance.ReStartGame();
        }

        public void RestartBtn()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

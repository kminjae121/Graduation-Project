using UnityEngine;

namespace Code.Tower
{
    public class TowerRoomExitPortal : MonoBehaviour
    {
        [SerializeField] private bool completeCurrentRoom = true;
        [SerializeField] private bool triggerOnPlayerEnter = true;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private string fallbackTowerMapSceneName = TowerRunSession.DefaultTowerMapSceneName;

        private bool _isLoading;

        public void ExitRoom()
        {
            if (_isLoading)
                return;

            _isLoading = true;

            if (completeCurrentRoom)
                TowerRunSession.CompleteCurrentRoom();

            string sceneName = TowerRunSession.IsActive
                ? TowerRunSession.TowerSceneName
                : fallbackTowerMapSceneName;

            TowerSceneLoader.LoadScene(sceneName);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!triggerOnPlayerEnter)
                return;

            if (!string.IsNullOrWhiteSpace(playerTag) && !other.CompareTag(playerTag))
                return;

            ExitRoom();
        }
    }
}

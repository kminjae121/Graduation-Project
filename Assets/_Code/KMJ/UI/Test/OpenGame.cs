using Code.Core.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.UI.Test
{
    public class OpenGame : MonoBehaviour
    {
        public void Open()
        {
            SceneChangeManager.Instance.ChangeSelectScene("LobbyScene");
            
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}

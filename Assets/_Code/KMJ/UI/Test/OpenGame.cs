using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.UI.Test
{
    public class OpenGame : MonoBehaviour
    {
        public void Open()
        {
            SceneManager.LoadScene("Lobby");
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}
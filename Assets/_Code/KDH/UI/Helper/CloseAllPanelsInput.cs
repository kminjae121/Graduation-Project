using UnityEngine;
using Input; 

namespace Code.UI
{
    public class CloseAllPanelsInput : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private InputReader inputReader;

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.OnCancelEvent += HandleCloseAll;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.OnCancelEvent -= HandleCloseAll;
            }
        }

        private void HandleCloseAll()
        {
            PanelManager.CloseAll();
        }
    }
}
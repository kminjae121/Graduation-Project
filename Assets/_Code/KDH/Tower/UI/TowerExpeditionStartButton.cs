using Code.UI;
using PixeLadder.EasyTransition;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Tower.UI
{
    public class TowerExpeditionStartButton : MonoBehaviour
    {
        [SerializeField] private Button expeditionButton;
        [SerializeField] private GameObject partySelectionRoot;
        [SerializeField] private string partyPanelId;
        [SerializeField] private TransitionEffect transitionEffect;

        private void Awake()
        {
            if (expeditionButton == null)
                expeditionButton = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (expeditionButton != null)
                expeditionButton.onClick.AddListener(OpenPartySelection);
        }

        private void OnDisable()
        {
            if (expeditionButton != null)
                expeditionButton.onClick.RemoveListener(OpenPartySelection);
        }

        public void OpenPartySelection()
        {
            TowerSceneLoader.DoTransition(ShowPartySelection, transitionEffect);
        }

        private void ShowPartySelection()
        {
            if (!string.IsNullOrWhiteSpace(partyPanelId))
            {
                PanelManager.Open(partyPanelId);
                return;
            }

            if (partySelectionRoot != null)
            {
                partySelectionRoot.SetActive(true);
                return;
            }

            PartyUI partyUI = FindFirstObjectByType<PartyUI>(FindObjectsInactive.Include);

            if (partyUI != null)
                partyUI.gameObject.SetActive(true);
        }
    }
}

using Code.Core.Managers;
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
        [SerializeField] private bool hidePartySelectionOnStart = true;
        [SerializeField] private PartyUIManager partyUIManager;
        [SerializeField] private bool enablePartyHoverEventsInSelection;
        [SerializeField] private TransitionEffect transitionEffect;

        private void Awake()
        {
            if (expeditionButton == null)
                expeditionButton = GetComponent<Button>();

            if (partyUIManager == null)
                partyUIManager = FindFirstObjectByType<PartyUIManager>(FindObjectsInactive.Include);
        }

        private void Start()
        {
            if (hidePartySelectionOnStart && partySelectionRoot != null)
                partySelectionRoot.SetActive(false);

            partyUIManager?.ResetCharacterInteractionMode();
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
            partyUIManager?.SetCharacterInteractionMode(
                CharacterStateClickMode.SelectExpeditionParty,
                enablePartyHoverEventsInSelection);

            if (!string.IsNullOrWhiteSpace(partyPanelId) && PanelManager.TryOpen(partyPanelId))
                return;

            if (partySelectionRoot != null)
            {
                partySelectionRoot.SetActive(true);
                partySelectionRoot.transform.SetAsLastSibling();
                return;
            }

            PartyUI partyUI = FindFirstObjectByType<PartyUI>(FindObjectsInactive.Include);

            if (partyUI != null)
            {
                partyUI.gameObject.SetActive(true);
                partyUI.transform.SetAsLastSibling();
            }
            else
            {
                Debug.LogWarning("원정 파티 선택 UI를 찾을 수 없습니다.", this);
            }
        }
    }
}

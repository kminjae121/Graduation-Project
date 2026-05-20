using System.Collections.Generic;
using Code.UI;
using Code.UnitManaging;
using UnityEngine;

namespace Code.Core.Managers
{
    public class PartyUIManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private UnitStorageSO unitStorage;

        [Header("UI Elements")]
        [SerializeField] private List<CharacterStateUI> characterUIList;

        [Header("Default Interaction")]
        [SerializeField] private CharacterStateClickMode characterClickMode = CharacterStateClickMode.OpenInfoPanel;
        [SerializeField] private bool sendPartyHoverEvents;

        private void Start()
        {
            BindPartyUnits();
            ResetCharacterInteractionMode();
        }

        public void SetCharacterInteractionMode(CharacterStateClickMode clickMode, bool enablePartyHoverEvents)
        {
            ApplyCharacterInteractionMode(clickMode, enablePartyHoverEvents);
        }

        public void ResetCharacterInteractionMode()
        {
            ApplyCharacterInteractionMode(characterClickMode, sendPartyHoverEvents);
        }

        private void BindPartyUnits()
        {
            if (characterUIList == null)
                return;

            for (int i = 0; i < characterUIList.Count; ++i)
            {
                CharacterStateUI characterUI = characterUIList[i];

                if (characterUI == null)
                    continue;

                if (unitStorage != null && i < unitStorage.unitStates.Count)
                {
                    characterUI.gameObject.SetActive(true);
                    characterUI.SetUnit(unitStorage.unitStates[i]);
                }
                else
                {
                    characterUI.SetUnit(null);
                    characterUI.gameObject.SetActive(false);
                }
            }
        }

        private void ApplyCharacterInteractionMode(CharacterStateClickMode clickMode, bool enablePartyHoverEvents)
        {
            if (characterUIList == null)
                return;

            foreach (CharacterStateUI characterUI in characterUIList)
            {
                if (characterUI != null)
                    characterUI.SetClickMode(clickMode, enablePartyHoverEvents);
            }
        }
    }
}

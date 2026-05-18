using System.Collections.Generic;
using Code.UI;
using Code.UnitManaging;
using UnityEngine;

namespace Code.Managers
{
    public class PartyUIManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private UnitStorageSO unitStorage;

        [Header("UI Elements")]
        [SerializeField] private List<CharacterStateUI> characterUIList;

        [Header("Interaction")]
        [SerializeField] private CharacterStateClickMode characterClickMode = CharacterStateClickMode.OpenInfoPanel;
        [SerializeField] private bool sendPartyHoverEvents;

        private void Start()
        {
            BindPartyUnits();
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

                characterUI.SetClickMode(characterClickMode, sendPartyHoverEvents);

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
    }
}

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
        
        private void Start()
        {
            BindPartyUnits();
        }
        
        private void BindPartyUnits()
        {
            for (int i = 0; i < characterUIList.Count; ++i)
                if (i < unitStorage.unitStates.Count)
                {
                    characterUIList[i].gameObject.SetActive(true);
                    characterUIList[i].SetUnit(unitStorage.unitStates[i]);
                }
                else
                    characterUIList[i].gameObject.SetActive(false);
        }
    }
}
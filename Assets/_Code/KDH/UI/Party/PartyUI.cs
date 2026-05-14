using System.Collections.Generic;
using Code.Core.Debugs;
using Code.Core.Events.Bus;
using Code.Tower;
using Code.UnitManaging;
using Code.UnitSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public enum PartySelectionMode
    {
        InitialParty,
        TowerExpedition
    }

    public class PartyUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button startButton;

        [Header("Slots")]
        [SerializeField] private List<SelectedCharacterSlotUI> characterSlots;

        [Header("Data")]
        [SerializeField] private UnitStorageSO unitStorage;
        [SerializeField] private int maxUnitCount = 3;

        [Header("Flow")]
        [SerializeField] private PartySelectionMode selectionMode = PartySelectionMode.InitialParty;
        [SerializeField] private string lobbySceneName = "LobbyScene";
        [SerializeField] private string towerMapSceneName = "LobbyScene";

        private UnitSO[] _partyUnits;

        private void Awake()
        {
            _partyUnits = new UnitSO[maxUnitCount];

            Bus<PartyCharacterSelectEvent>.Subscribe(HandleCharacterSelected);
            Bus<PartyCharacterDeselectEvent>.Subscribe(HandleCharacterDeselected);

            if (startButton != null)
                startButton.onClick.AddListener(HandleStartButton);
        }

        private void Start()
        {
            for (int i = 0; i < characterSlots.Count; i++)
                if (characterSlots[i] != null)
                    characterSlots[i].UpdateSlot(null);
        }

        private void OnDestroy()
        {
            Bus<PartyCharacterSelectEvent>.Unsubscribe(HandleCharacterSelected);
            Bus<PartyCharacterDeselectEvent>.Unsubscribe(HandleCharacterDeselected);

            if (startButton != null)
                startButton.onClick.RemoveListener(HandleStartButton);
        }

        private void HandleCharacterSelected(PartyCharacterSelectEvent evt)
        {
            for (int i = 0; i < _partyUnits.Length; i++)
                if (_partyUnits[i] == evt.Unit)
                    return;

            for (int i = 0; i < _partyUnits.Length; i++)
            {
                if (_partyUnits[i] != null)
                    continue;

                _partyUnits[i] = evt.Unit;

                if (i < characterSlots.Count)
                    characterSlots[i].UpdateSlot(evt.Unit);

                break;
            }
        }

        private void HandleCharacterDeselected(PartyCharacterDeselectEvent evt)
        {
            for (int i = 0; i < _partyUnits.Length; i++)
            {
                if (_partyUnits[i] != evt.Unit)
                    continue;

                _partyUnits[i] = null;

                if (i < characterSlots.Count)
                    characterSlots[i].UpdateSlot(null);

                break;
            }
        }

        private void HandleStartButton()
        {
            List<UnitSO> selectedUnits = CollectSelectedUnits();

            if (selectedUnits.Count == 0)
            {
                UnityLogger.Log("파티에 유닛이 없습니다.");
                return;
            }

            if (selectionMode == PartySelectionMode.InitialParty)
            {
                WriteSelectedUnitsToStorage(selectedUnits);
                TowerRunSession.EndRun();
                TowerSceneLoader.LoadScene(lobbySceneName);
                return;
            }

            TowerRunSession.StartNewRun(selectedUnits, towerMapSceneName, lobbySceneName);
            TowerRunSession.WritePartyToStorage(unitStorage);
            TowerSceneLoader.LoadScene(TowerRunSession.TowerSceneName);
        }

        private List<UnitSO> CollectSelectedUnits()
        {
            List<UnitSO> selectedUnits = new();

            foreach (UnitSO unit in _partyUnits)
                if (unit != null)
                    selectedUnits.Add(unit);

            return selectedUnits;
        }

        private void WriteSelectedUnitsToStorage(IEnumerable<UnitSO> selectedUnits)
        {
            if (unitStorage == null)
                return;

            unitStorage.units.Clear();
            unitStorage.unitStates.Clear();

            foreach (UnitSO unit in selectedUnits)
            {
                if (unit == null)
                    continue;

                unitStorage.units.Add(unit.UnitSpawn);
                unitStorage.unitStates.Add(new UnitState(unit));
            }
        }
    }
}

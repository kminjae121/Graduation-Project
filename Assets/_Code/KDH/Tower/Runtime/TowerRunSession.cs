using System.Collections.Generic;
using Code.UnitManaging;
using Code.UnitSystem;
using UnityEngine;

namespace Code.Tower
{
    public static class TowerRunSession
    {
        private static readonly List<UnitSO> _selectedUnits = new();

        public static bool IsActive { get; private set; }
        public static TowerFloorKey CurrentFloorKey { get; private set; } = new(1, 1);
        public static TowerFloorMap CurrentMap { get; private set; }
        public static string TowerSceneName { get; private set; } = "LobbyScene";
        public static string LobbySceneName { get; private set; } = "LobbyScene";
        public static IReadOnlyList<UnitSO> SelectedUnits => _selectedUnits;

        public static void StartNewRun(
            IEnumerable<UnitSO> selectedUnits,
            string towerSceneName = "LobbyScene",
            string lobbySceneName = "LobbyScene",
            int seed = 0)
        {
            _selectedUnits.Clear();

            if (selectedUnits != null)
                foreach (UnitSO unit in selectedUnits)
                    if (unit != null && !_selectedUnits.Contains(unit))
                        _selectedUnits.Add(unit);

            TowerSceneName = string.IsNullOrWhiteSpace(towerSceneName) ? "LobbyScene" : towerSceneName;
            LobbySceneName = string.IsNullOrWhiteSpace(lobbySceneName) ? "LobbyScene" : lobbySceneName;
            IsActive = true;
            CurrentFloorKey = new TowerFloorKey(1, 1);
            CurrentMap = TowerMapGenerator.Generate(CurrentFloorKey, seed);
        }

        public static void CompleteCurrentRoom()
        {
            if (!IsActive || CurrentMap == null)
                return;

            CurrentMap.ClearCurrentRoom();
        }

        public static void AdvanceToNextFloor(int seed = 0)
        {
            if (!IsActive)
                return;

            CurrentFloorKey = CurrentFloorKey.Next();
            CurrentMap = TowerMapGenerator.Generate(CurrentFloorKey, seed);
        }

        public static void EndRun()
        {
            IsActive = false;
            CurrentMap = null;
            _selectedUnits.Clear();
        }

        public static void WritePartyToStorage(UnitStorageSO unitStorage)
        {
            if (unitStorage == null)
                return;

            unitStorage.units.Clear();
            unitStorage.unitStates.Clear();

            foreach (UnitSO unit in _selectedUnits)
            {
                if (unit == null)
                    continue;

                unitStorage.units.Add(unit.UnitSpawn);
                unitStorage.unitStates.Add(new UnitState(unit));
            }
        }

        public static string GetCurrentFloorName()
        {
            if (!IsActive)
                return string.Empty;

            return CurrentFloorKey.DisplayName;
        }
    }
}

using System.Collections.Generic;
using Code.Campaign;
using Code.UnitManaging;
using Code.UnitSystem;
using UnityEngine;

namespace Code.Tower
{
    public static class TowerRunSession
    {
        public const string DefaultLobbySceneName = "LobbyScene";
        public const string DefaultTowerMapSceneName = "TowerMapScene";

        private static readonly List<UnitSO> _selectedUnits = new();
        private static TowerMapDatabase _mapDatabase;

        public static bool IsActive { get; private set; }
        public static TowerFloorKey CurrentFloorKey { get; private set; } = new(1, 1);
        public static TowerFloorMap CurrentMap { get; private set; }
        public static string TowerSceneName { get; private set; } = DefaultTowerMapSceneName;
        public static string LobbySceneName { get; private set; } = DefaultLobbySceneName;
        public static IReadOnlyList<UnitSO> SelectedUnits => _selectedUnits;
        public static TowerRoomNode CurrentRoom => CurrentMap?.GetCurrentRoom();
        public static TowerRoomType CurrentRoomType => CurrentRoom?.RoomType ?? TowerRoomType.Start;
        public static bool CanUseCurrentPortal =>
            IsActive &&
            CurrentRoom is { IsCleared: true } &&
            (CurrentRoom.RoomType == TowerRoomType.Portal || CurrentRoom.RoomType == TowerRoomType.Boss);

        public static void StartNewRun(
            IEnumerable<UnitSO> selectedUnits,
            string towerSceneName = DefaultTowerMapSceneName,
            string lobbySceneName = DefaultLobbySceneName,
            int seed = 0,
            TowerMapDatabase mapDatabase = null)
        {
            _selectedUnits.Clear();

            if (selectedUnits != null)
                foreach (UnitSO unit in selectedUnits)
                    if (unit != null && !_selectedUnits.Contains(unit))
                        _selectedUnits.Add(unit);

            TowerSceneName = string.IsNullOrWhiteSpace(towerSceneName) ? DefaultTowerMapSceneName : towerSceneName;
            LobbySceneName = string.IsNullOrWhiteSpace(lobbySceneName) ? DefaultLobbySceneName : lobbySceneName;
            _mapDatabase = mapDatabase != null ? mapDatabase : TowerMapDatabase.LoadDefault();
            IsActive = true;
            CurrentFloorKey = new TowerFloorKey(1, 1);
            CurrentMap = CreateMap(CurrentFloorKey, seed);
        }

        public static bool TryMoveToRoom(int roomId, out TowerRoomNode room)
        {
            room = null;

            if (!IsActive || CurrentMap == null)
                return false;

            TowerRoomNode currentRoom = CurrentMap.GetCurrentRoom();

            if (currentRoom == null || !currentRoom.IsCleared)
                return false;

            if (!CurrentMap.MoveTo(roomId))
                return false;

            room = CurrentMap.GetCurrentRoom();
            room?.Visit();
            return room != null;
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
            CurrentMap = CreateMap(CurrentFloorKey, seed);
        }

        public static void FailRun()
        {
            EndRun(CampaignDateAdvanceReason.ExpeditionFailed);
        }

        public static void EndRun()
        {
            EndRun(CampaignDateAdvanceReason.ExpeditionReturned);
        }

        private static void EndRun(CampaignDateAdvanceReason advanceReason)
        {
            bool shouldAdvanceDate = IsActive;

            IsActive = false;
            CurrentMap = null;
            _mapDatabase = null;
            _selectedUnits.Clear();
            TowerSceneName = DefaultTowerMapSceneName;
            LobbySceneName = DefaultLobbySceneName;

            if (shouldAdvanceDate)
                CampaignDateSystem.AdvanceWeek(advanceReason);
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

        private static TowerFloorMap CreateMap(TowerFloorKey floorKey, int seed)
        {
            _mapDatabase ??= TowerMapDatabase.LoadDefault();

            if (_mapDatabase != null && _mapDatabase.TryCreateMap(floorKey, out TowerFloorMap map))
                return map;

            Debug.LogWarning($"[TowerRunSession] {floorKey.DisplayName} 수동 맵 정의를 찾지 못해 자동 생성 맵을 사용합니다.");
            return TowerMapGenerator.Generate(floorKey, seed);
        }
    }
}

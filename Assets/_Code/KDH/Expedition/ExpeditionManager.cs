using System.Collections.Generic;
using Code.Core;
using Code.Tower;
using Code.Tower.UI;
using PixeLadder.EasyTransition;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

namespace Code.Expedition.Managers
{
    public class ExpeditionManager : MonoSingleton<ExpeditionManager>
    {
        [Header("Scene")]
        [SerializeField] private string battleSceneName = "BattleScene1";
        [SerializeField] private List<string> battleSceneNames = new() { "BattleScene1" };
        [SerializeField] private string eliteBattleSceneName = "BattleScene1";
        [SerializeField] private List<string> eliteBattleSceneNames = new();
        [SerializeField] private string bossBattleSceneName = "BattleScene1";
        [SerializeField] private List<string> bossBattleSceneNames = new();
        [SerializeField] private string eventSceneName = "SelectEventScene";
        [SerializeField] private string rewardSceneName = "RewardScene";
        [SerializeField] private TransitionEffect battleTransitionEffect;

        [Header("Runtime UI")]
        [SerializeField] private bool autoCreateRuntimeUI = true;
        [SerializeField] private Canvas canvas;
        [SerializeField] private TowerNodeMapView nodeMapView;
        [SerializeField] private TowerPortalChoicePanel portalChoicePanel;

        protected override void Awake()
        {
            isDontDestroyOnLoad = false;
            base.Awake();
        }

        private void Start()
        {
            if (!TowerRunSession.IsActive)
            {
                Debug.LogWarning("[ExpeditionManager] 진행 중인 탑 원정이 없습니다.");
                return;
            }

            EnsureRuntimeUI();
            WireUIEvents();

            if (ResolveCurrentRoomOnMapEnter())
                RefreshUI();
        }

        private void OnDestroy()
        {
            UnwireUIEvents();
        }

        public void RequestMoveToRoom(int roomId)
        {
            if (!TowerRunSession.TryMoveToRoom(roomId, out TowerRoomNode movedRoom))
            {
                Debug.LogWarning($"현재 방이 클리어되어 있고 연결된 방일 때만 {roomId}번 방으로 이동할 수 있습니다.");
                return;
            }

            if (ResolveCurrentRoomOnMapEnter(movedRoom))
                RefreshUI();
        }

        private bool ResolveCurrentRoomOnMapEnter(TowerRoomNode roomOverride = null)
        {
            TowerFloorMap map = TowerRunSession.CurrentMap;
            TowerRoomNode room = roomOverride ?? map?.GetCurrentRoom();

            if (room == null)
                return true;

            room.Visit();

            switch (room.RoomType)
            {
                case TowerRoomType.Start:
                case TowerRoomType.Portal:
                    room.Clear();
                    break;
                case TowerRoomType.Event:
                    if (!room.IsCleared)
                    {
                        LoadRoomScene(eventSceneName);
                        return false;
                    }
                    break;
                case TowerRoomType.Reward:
                    if (!room.IsCleared)
                    {
                        LoadRoomScene(rewardSceneName);
                        return false;
                    }
                    break;
                case TowerRoomType.Combat:
                    if (!room.IsCleared)
                    {
                        LoadBattleScene(GetRandomSceneName(battleSceneNames, battleSceneName));
                        return false;
                    }
                    break;
                case TowerRoomType.EliteCombat:
                    if (!room.IsCleared)
                    {
                        LoadBattleScene(GetRandomSceneName(eliteBattleSceneNames, eliteBattleSceneName));
                        return false;
                    }
                    break;
                case TowerRoomType.Boss:
                    if (!room.IsCleared)
                    {
                        LoadBattleScene(GetRandomSceneName(bossBattleSceneNames, bossBattleSceneName));
                        return false;
                    }
                    break;
            }

            return true;
        }

        private void RefreshUI()
        {
            TowerFloorMap map = TowerRunSession.CurrentMap;

            if (map == null)
                return;

            SetRuntimeUIVisible(true);
            nodeMapView?.Render(map);

            if (TowerRunSession.CanUseCurrentPortal)
                portalChoicePanel?.Show(map.FloorKey, TowerRunSession.CurrentRoomType == TowerRoomType.Boss);
            else
                portalChoicePanel?.Hide();
        }

        private void HandleNextFloorSelected()
        {
            TowerRunSession.AdvanceToNextFloor();
            ResolveCurrentRoomOnMapEnter();
            RefreshUI();
        }

        private void HandleReturnLobbySelected()
        {
            string lobbySceneName = TowerRunSession.LobbySceneName;
            TowerRunSession.EndRun();
            TowerSceneLoader.LoadScene(lobbySceneName);
        }

        private void LoadBattleScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("[ExpeditionManager] 전투 씬 이름이 비어 있습니다.");
                return;
            }

            SetRuntimeUIVisible(false);
            TowerSceneLoader.LoadScene(sceneName, battleTransitionEffect);
        }

        private void LoadRoomScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("[ExpeditionManager] 방 씬 이름이 비어 있습니다.");
                return;
            }

            SetRuntimeUIVisible(false);
            TowerSceneLoader.LoadScene(sceneName, battleTransitionEffect);
        }

        private static string GetRandomSceneName(IReadOnlyList<string> sceneNames, string fallbackSceneName)
        {
            int validSceneCount = 0;

            if (sceneNames != null)
            {
                for (int i = 0; i < sceneNames.Count; i++)
                    if (!string.IsNullOrWhiteSpace(sceneNames[i]))
                        validSceneCount++;
            }

            if (validSceneCount <= 0)
                return fallbackSceneName;

            int selectedIndex = Random.Range(0, validSceneCount);

            for (int i = 0; i < sceneNames.Count; i++)
            {
                string sceneName = sceneNames[i];
                if (string.IsNullOrWhiteSpace(sceneName))
                    continue;

                if (selectedIndex == 0)
                    return sceneName;

                selectedIndex--;
            }

            return fallbackSceneName;
        }

        private void SetRuntimeUIVisible(bool visible)
        {
            if (nodeMapView != null)
                nodeMapView.gameObject.SetActive(visible);

            if (!visible)
                portalChoicePanel?.Hide();
        }

        private void EnsureRuntimeUI()
        {
            if (!autoCreateRuntimeUI)
                return;

            if (canvas == null)
                canvas = FindAnyObjectByType<Canvas>();

            if (canvas == null)
                canvas = CreateRuntimeCanvas();

            EnsureCanvasRaycaster(canvas);
            EnsureEventSystem();

            RectTransform canvasRect = canvas.transform as RectTransform;

            if (nodeMapView == null)
            {
                GameObject nodeMapObject = new("TowerNodeMap", typeof(RectTransform));
                nodeMapView = nodeMapObject.AddComponent<TowerNodeMapView>();
                nodeMapView.BuildDefaultLayout(canvasRect);
            }

            if (portalChoicePanel == null)
            {
                GameObject portalObject = new("TowerPortalChoicePanel", typeof(RectTransform));
                portalChoicePanel = portalObject.AddComponent<TowerPortalChoicePanel>();
                portalChoicePanel.BuildDefaultLayout(canvasRect);
            }
        }

        private static Canvas CreateRuntimeCanvas()
        {
            GameObject canvasObject = new("TowerRuntimeCanvas");
            Canvas newCanvas = canvasObject.AddComponent<Canvas>();
            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObject.AddComponent<GraphicRaycaster>();
            return newCanvas;
        }

        private static void EnsureCanvasRaycaster(Canvas targetCanvas)
        {
            if (targetCanvas == null)
                return;

            if (targetCanvas.GetComponent<GraphicRaycaster>() == null)
                targetCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;

            GameObject eventSystemObject = new("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private void WireUIEvents()
        {
            if (nodeMapView != null)
            {
                nodeMapView.OnRoomSelected -= RequestMoveToRoom;
                nodeMapView.OnRoomSelected += RequestMoveToRoom;
            }

            if (portalChoicePanel != null)
            {
                portalChoicePanel.OnNextFloorSelected -= HandleNextFloorSelected;
                portalChoicePanel.OnNextFloorSelected += HandleNextFloorSelected;
                portalChoicePanel.OnReturnLobbySelected -= HandleReturnLobbySelected;
                portalChoicePanel.OnReturnLobbySelected += HandleReturnLobbySelected;
            }
        }

        private void UnwireUIEvents()
        {
            if (nodeMapView != null)
                nodeMapView.OnRoomSelected -= RequestMoveToRoom;

            if (portalChoicePanel != null)
            {
                portalChoicePanel.OnNextFloorSelected -= HandleNextFloorSelected;
                portalChoicePanel.OnReturnLobbySelected -= HandleReturnLobbySelected;
            }
        }
    }
}

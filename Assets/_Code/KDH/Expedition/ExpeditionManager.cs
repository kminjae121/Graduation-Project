using Code.Core;
using Code.Tower;
using Code.Tower.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Expedition.Managers
{
    public class ExpeditionManager : MonoSingleton<ExpeditionManager>
    {
        [Header("Scene")]
        [SerializeField] private string battleSceneName = "BattleScene";
        [SerializeField] private string eliteBattleSceneName = "BattleScene";
        [SerializeField] private string bossBattleSceneName = "BattleScene";

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
                return;

            EnsureRuntimeUI();
            WireUIEvents();
            ResolveCurrentRoomOnMapEnter();
            RefreshUI();
        }

        private void OnDestroy()
        {
            UnwireUIEvents();
        }

        public void RequestMoveToRoom(int roomId)
        {
            TowerFloorMap map = TowerRunSession.CurrentMap;

            if (map == null)
                return;

            TowerRoomNode currentRoom = map.GetCurrentRoom();

            if (currentRoom is { IsCleared: false })
            {
                Debug.LogWarning("현재 방을 클리어해야 다음 방으로 이동할 수 있습니다.");
                return;
            }

            if (!map.MoveTo(roomId))
            {
                Debug.LogWarning($"현재 방에서 {roomId}번 방으로 이동할 수 없습니다.");
                return;
            }

            ResolveCurrentRoomOnMapEnter();
            RefreshUI();
        }

        private void ResolveCurrentRoomOnMapEnter()
        {
            TowerFloorMap map = TowerRunSession.CurrentMap;
            TowerRoomNode room = map?.GetCurrentRoom();

            if (room == null)
                return;

            room.Visit();

            switch (room.RoomType)
            {
                case TowerRoomType.Start:
                case TowerRoomType.Event:
                case TowerRoomType.Reward:
                case TowerRoomType.Portal:
                    room.Clear();
                    break;
                case TowerRoomType.Combat:
                    if (!room.IsCleared)
                        LoadBattleScene(battleSceneName);
                    break;
                case TowerRoomType.EliteCombat:
                    if (!room.IsCleared)
                        LoadBattleScene(eliteBattleSceneName);
                    break;
                case TowerRoomType.Boss:
                    if (!room.IsCleared)
                        LoadBattleScene(bossBattleSceneName);
                    break;
            }
        }

        private void RefreshUI()
        {
            TowerFloorMap map = TowerRunSession.CurrentMap;

            if (map == null)
                return;

            nodeMapView?.Render(map);

            TowerRoomNode currentRoom = map.GetCurrentRoom();
            bool canUsePortal = currentRoom != null &&
                                currentRoom.IsCleared &&
                                (currentRoom.RoomType == TowerRoomType.Portal ||
                                 currentRoom.RoomType == TowerRoomType.Boss);

            if (canUsePortal)
                portalChoicePanel?.Show(map.FloorKey, currentRoom.RoomType == TowerRoomType.Boss);
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
                Debug.LogWarning("전투 씬 이름이 비어 있습니다.");
                return;
            }

            TowerSceneLoader.LoadScene(sceneName);
        }

        private void EnsureRuntimeUI()
        {
            if (!autoCreateRuntimeUI)
                return;

            if (canvas == null)
                canvas = FindAnyObjectByType<Canvas>();

            if (canvas == null)
                canvas = CreateRuntimeCanvas();

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

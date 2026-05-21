using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Tower.UI
{
    public class TowerNodeMapView : MonoBehaviour
    {
        [Serializable]
        private sealed class RoomSpriteBinding
        {
            public TowerRoomType roomType;
            public Sprite sprite;
        }

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI floorText;

        [Header("Map Roots")]
        [SerializeField] private RectTransform nodeRoot;

        [Header("Prefabs")]
        [SerializeField] private TowerRoomNodeView roomNodePrefab;
        [SerializeField] private TowerRoomTooltip roomTooltip;

        [Header("Room Sprites")]
        [SerializeField] private List<RoomSpriteBinding> roomSprites = new();
        [SerializeField] private Sprite unknownRoomSprite;

        [Header("Layout")]
        [SerializeField] private Vector2 cellSize = new(230f, 170f);
        [SerializeField] private Vector2 nodeSize = new(118f, 118f);
        [SerializeField] private float connectionWidth = 5f;
        [SerializeField] private float mapPadding = 120f;
        [SerializeField] private float minMapScale = 0.58f;
        [SerializeField] private float maxMapScale = 1.08f;

        [Header("Colors")]
        [SerializeField] private Color lockedConnectionColor = new(0.16f, 0.17f, 0.19f, 0.78f);
        [SerializeField] private Color lockedRoomColor = new(0.2f, 0.21f, 0.23f, 0.92f);

        private static readonly Color ActiveConnectionColor = Color.white;
        private static readonly Color ActiveRoomColor = Color.white;

        private readonly Dictionary<int, Vector2> _nodePositions = new();
        private float _mapScale = 1f;

        public event Action<int> OnRoomSelected;

        public void BuildDefaultLayout(RectTransform parent)
        {
            RectTransform self = GetComponent<RectTransform>();
            self.SetParent(parent, false);
            self.anchorMin = Vector2.zero;
            self.anchorMax = Vector2.one;
            self.offsetMin = Vector2.zero;
            self.offsetMax = Vector2.zero;

            Image background = GetOrAdd<Image>(gameObject);
            background.color = new Color(0.018f, 0.023f, 0.03f, 1f);

            CreateBackdropBand("TopBand", self, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -110f), Vector2.zero, new Color(0.015f, 0.018f, 0.028f, 0.92f));

            floorText = CreateText("FloorText", self, new Vector2(52f, -30f), new Vector2(520f, -86f), 36f, FontStyles.Bold);
            floorText.alignment = TextAlignmentOptions.Left;

            nodeRoot = CreateRect("TowerMapRoot", self);
            nodeRoot.anchorMin = new Vector2(0f, 0f);
            nodeRoot.anchorMax = new Vector2(1f, 1f);
            nodeRoot.offsetMin = new Vector2(46f, 104f);
            nodeRoot.offsetMax = new Vector2(-46f, -148f);

            roomTooltip = TowerRoomTooltip.CreateDefault(self);
        }

        public void Render(TowerFloorMap map)
        {
            if (map == null)
                return;

            EnsureNodeRoot();
            EnsureTooltip();
            ClearNodeRoot();
            roomTooltip?.Hide();

            TowerRoomNode currentRoom = map.GetCurrentRoom();
            if (currentRoom == null)
                return;

            List<TowerRoomNode> rooms = map.Rooms.ToList();
            CacheNodePositions(rooms);
            UpdateFloorText(map);
            DrawConnections(map, currentRoom, rooms);
            DrawRoomNodes(map, currentRoom, rooms);
        }

        private void UpdateFloorText(TowerFloorMap map)
        {
            if (floorText != null)
                floorText.text = map.FloorKey.DisplayName;
        }

        private void DrawConnections(TowerFloorMap map, TowerRoomNode currentRoom, IReadOnlyList<TowerRoomNode> rooms)
        {
            HashSet<string> drawnConnections = new();

            foreach (TowerRoomNode room in rooms)
            {
                foreach (int connectedId in room.ConnectedRoomIds)
                {
                    if (!map.TryGetRoom(connectedId, out TowerRoomNode connectedRoom))
                        continue;

                    string key = room.Id < connectedRoom.Id
                        ? $"{room.Id}:{connectedRoom.Id}"
                        : $"{connectedRoom.Id}:{room.Id}";

                    if (!drawnConnections.Add(key))
                        continue;

                    DrawConnection(map, currentRoom, room, connectedRoom);
                }
            }
        }

        private void DrawConnection(TowerFloorMap map, TowerRoomNode currentRoom, TowerRoomNode fromRoom, TowerRoomNode toRoom)
        {
            Vector2 from = GetNodePosition(fromRoom);
            Vector2 to = GetNodePosition(toRoom);
            bool connectsCurrent = fromRoom.Id == currentRoom.Id || toRoom.Id == currentRoom.Id;
            bool isAvailable = currentRoom.IsCleared && connectsCurrent && (map.CanMoveTo(fromRoom.Id) || map.CanMoveTo(toRoom.Id));
            bool isClearedPath = fromRoom.IsCleared && toRoom.IsCleared;
            Color color = isAvailable || isClearedPath ? ActiveConnectionColor : lockedConnectionColor;

            CreateConnectionLine("ConnectionShadow", from, to, connectionWidth + 5f, new Color(0f, 0f, 0f, 0.42f));
            CreateConnectionLine("Connection", from, to, connectionWidth, color);
        }

        private void DrawRoomNodes(TowerFloorMap map, TowerRoomNode currentRoom, IReadOnlyList<TowerRoomNode> rooms)
        {
            foreach (TowerRoomNode room in rooms)
                DrawRoomNode(map, currentRoom, room);
        }

        private void DrawRoomNode(TowerFloorMap map, TowerRoomNode currentRoom, TowerRoomNode room)
        {
            bool isCurrent = room.Id == currentRoom.Id;
            bool isAvailable = !isCurrent && currentRoom.IsCleared && map.CanMoveTo(room.Id);
            bool isRevealed = ShouldRevealRoomType(currentRoom, room);
            bool useLockedColor = !isRevealed || room.IsCleared || (!isCurrent && !isAvailable);

            TowerRoomNodeView nodeView = CreateNodeView(room);
            RectTransform nodeRect = nodeView.transform as RectTransform;

            if (nodeRect != null)
            {
                nodeRect.sizeDelta = nodeSize * Mathf.Clamp(_mapScale, 0.78f, 1.02f);
                nodeRect.anchoredPosition = GetNodePosition(room);
            }

            nodeView.Apply(
                GetRoomSprite(room.RoomType),
                unknownRoomSprite,
                isRevealed,
                room.IsCleared,
                isAvailable,
                useLockedColor ? lockedRoomColor : ActiveRoomColor);

            if (isAvailable)
            {
                int roomId = room.Id;
                nodeView.Clicked += () => OnRoomSelected?.Invoke(roomId);
            }

            if (isRevealed)
                WireTooltip(nodeView, map, currentRoom, room, isAvailable);
        }

        private TowerRoomNodeView CreateNodeView(TowerRoomNode room)
        {
            TowerRoomNodeView nodeView = roomNodePrefab != null
                ? Instantiate(roomNodePrefab, nodeRoot)
                : CreateFallbackNodeView(nodeRoot);

            nodeView.gameObject.name = $"TowerNode_{room.Id}_{room.RoomType}";
            return nodeView;
        }

        private void WireTooltip(TowerRoomNodeView nodeView, TowerFloorMap map, TowerRoomNode currentRoom, TowerRoomNode room, bool isAvailable)
        {
            string title = TowerRoomTypePresentation.GetDisplayName(room.RoomType);
            string body = BuildTooltipBody(map, currentRoom, room, isAvailable);

            nodeView.PointerEntered += eventData => ShowTooltip(title, body, eventData);
            nodeView.PointerMoved += eventData => roomTooltip?.SetPosition(eventData);
            nodeView.PointerExited += _ => roomTooltip?.Hide();
        }

        private void ShowTooltip(string title, string body, PointerEventData eventData)
        {
            EnsureTooltip();
            roomTooltip?.Show(title, body, eventData);
        }

        private string BuildTooltipBody(TowerFloorMap map, TowerRoomNode currentRoom, TowerRoomNode room, bool isAvailable)
        {
            string state = GetStateLabel(currentRoom, room, isAvailable);
            string actionText = isAvailable
                ? "클릭하면 이 방으로 이동합니다."
                : room.Id == currentRoom.Id
                    ? "현재 파티가 있는 방입니다."
                    : room.IsCleared
                        ? "이미 클리어한 방입니다."
                        : "아직 이동할 수 없는 방입니다.";

            int connectedCount = map.GetConnectedRooms(room.Id).Count();
            return $"{TowerRoomTypePresentation.GetDescription(room.RoomType)}\n\n상태: {state}\n연결된 방: {connectedCount}개\n{actionText}";
        }

        private Color GetConnectionColor(TowerRoomNode fromRoom, TowerRoomNode toRoom)
            => fromRoom.IsCleared && toRoom.IsCleared ? ActiveConnectionColor : lockedConnectionColor;

        private Sprite GetRoomSprite(TowerRoomType roomType)
        {
            if (roomSprites == null)
                return null;

            foreach (RoomSpriteBinding binding in roomSprites)
                if (binding != null && binding.roomType == roomType)
                    return binding.sprite;

            return null;
        }

        private static bool ShouldRevealRoomType(TowerRoomNode currentRoom, TowerRoomNode room)
        {
            return room.Id == currentRoom.Id ||
                   room.IsVisited ||
                   room.IsCleared ||
                   currentRoom.IsConnectedTo(room.Id);
        }

        private static string GetStateLabel(TowerRoomNode currentRoom, TowerRoomNode room, bool isAvailable)
        {
            if (room.Id == currentRoom.Id)
                return "현재 위치";

            if (room.IsCleared)
                return "클리어됨";

            if (isAvailable)
                return "이동 가능";

            if (room.IsVisited)
                return "방문함";

            return ShouldRevealRoomType(currentRoom, room) ? "인접함" : "미확인";
        }

        private void CacheNodePositions(IReadOnlyList<TowerRoomNode> rooms)
        {
            _nodePositions.Clear();

            if (rooms == null || rooms.Count == 0)
            {
                _mapScale = 1f;
                return;
            }

            int minX = rooms.Min(room => room.GridPosition.x);
            int maxX = rooms.Max(room => room.GridPosition.x);
            int minY = rooms.Min(room => room.GridPosition.y);
            int maxY = rooms.Max(room => room.GridPosition.y);

            Vector2 center = new((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            Vector2 mapSize = new(Mathf.Max(1, maxX - minX + 1) * cellSize.x, Mathf.Max(1, maxY - minY + 1) * cellSize.y);
            Vector2 rootSize = GetNodeRootSize();
            Vector2 availableSize = new(Mathf.Max(1f, rootSize.x - mapPadding * 2f), Mathf.Max(1f, rootSize.y - mapPadding * 2f));
            _mapScale = Mathf.Clamp(Mathf.Min(availableSize.x / mapSize.x, availableSize.y / mapSize.y), minMapScale, maxMapScale);

            foreach (TowerRoomNode room in rooms)
            {
                Vector2 gridDelta = room.GridPosition - center;
                _nodePositions[room.Id] = new Vector2(gridDelta.x * cellSize.x, gridDelta.y * cellSize.y) * _mapScale;
            }
        }

        private Vector2 GetNodePosition(TowerRoomNode room)
        {
            return room != null && _nodePositions.TryGetValue(room.Id, out Vector2 position)
                ? position
                : Vector2.zero;
        }

        private Vector2 GetNodeRootSize()
        {
            Vector2 size = nodeRoot.rect.size;

            if (size.x > 0f && size.y > 0f)
                return size;

            return nodeRoot.sizeDelta.sqrMagnitude > 0f ? nodeRoot.sizeDelta : new Vector2(1100f, 640f);
        }

        private void CreateConnectionLine(string objectName, Vector2 from, Vector2 to, float width, Color color)
        {
            Vector2 delta = to - from;

            RectTransform line = CreateRect(objectName, nodeRoot);
            line.sizeDelta = new Vector2(delta.magnitude, width);
            line.anchoredPosition = from + delta * 0.5f;
            line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            line.SetAsFirstSibling();

            Image image = line.gameObject.AddComponent<Image>();
            image.color = color;
        }

        private void EnsureNodeRoot()
        {
            RectTransform self = transform as RectTransform;

            if (nodeRoot != null)
                return;

            nodeRoot = CreateRect("TowerMapRoot", self != null ? self : transform);
            nodeRoot.anchorMin = Vector2.zero;
            nodeRoot.anchorMax = Vector2.one;
            nodeRoot.offsetMin = new Vector2(46f, 104f);
            nodeRoot.offsetMax = new Vector2(-46f, -148f);
        }

        private void EnsureTooltip()
        {
            if (roomTooltip != null)
                return;

            RectTransform self = transform as RectTransform;
            roomTooltip = TowerRoomTooltip.CreateDefault(self != null ? self : nodeRoot);
        }

        private void ClearNodeRoot()
        {
            for (int i = nodeRoot.childCount - 1; i >= 0; --i)
                Destroy(nodeRoot.GetChild(i).gameObject);
        }

        private static TowerRoomNodeView CreateFallbackNodeView(Transform parent)
        {
            RectTransform root = CreateRect("TowerNode", parent);
            Image background = root.gameObject.AddComponent<Image>();
            background.color = ActiveRoomColor;

            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = background;

            RectTransform iconRect = CreateRect("Room Icon", root);
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(12f, 12f);
            iconRect.offsetMax = new Vector2(-12f, -12f);
            iconRect.gameObject.AddComponent<Image>().raycastTarget = false;

            RectTransform clearedRect = CreateRect("Cleared Room Icon", root);
            clearedRect.anchorMin = new Vector2(1f, 1f);
            clearedRect.anchorMax = new Vector2(1f, 1f);
            clearedRect.pivot = new Vector2(1f, 1f);
            clearedRect.anchoredPosition = new Vector2(-4f, -4f);
            clearedRect.sizeDelta = new Vector2(28f, 28f);
            Image clearedImage = clearedRect.gameObject.AddComponent<Image>();
            clearedImage.raycastTarget = false;
            clearedImage.color = new Color(0.35f, 1f, 0.72f, 0.95f);
            clearedRect.gameObject.SetActive(false);

            return root.gameObject.AddComponent<TowerRoomNodeView>();
        }

        private static void CreateBackdropBand(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            RectTransform rect = CreateRect(objectName, parent);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject go = new(objectName, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static TextMeshProUGUI CreateText(
            string objectName,
            Transform parent,
            Vector2 offsetMin,
            Vector2 offsetMax,
            float fontSize,
            FontStyles style)
        {
            RectTransform rect = CreateRect(objectName, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
        [SerializeField] private ScrollRect mapScrollRect;
        [SerializeField] private RectTransform viewportRoot;
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
        [SerializeField] private float horizontalMapPadding = 120f;
        [SerializeField] private float verticalMapPadding = 120f;
        [SerializeField, Min(1f)] private float verticalMapLengthMultiplier = 1.85f;
        [SerializeField, Min(0f)] private float minScrollableContentHeight = 1600f;

        [Header("Scroll")]
        [SerializeField] private bool scrollToCurrentRoom = true;
        [SerializeField, Min(1f)] private float scrollSensitivity = 34f;

        [Header("Connection Style")]
        [SerializeField] private Color connectionColor = new(0.19f, 0.2f, 0.18f, 0.92f);
        [SerializeField] private Color connectionShadowColor = new(0f, 0f, 0f, 0.18f);
        [SerializeField, Min(1f)] private float connectionDashLength = 12f;
        [SerializeField, Min(1f)] private float connectionDashGap = 10f;
        [SerializeField, Range(0f, 0.45f)] private float connectionCurveStrength = 0.18f;
        [SerializeField, Min(0f)] private float connectionMinCurveOffset = 34f;
        [SerializeField, Min(0f)] private float connectionMaxCurveOffset = 96f;

        private readonly Dictionary<int, Vector2> _nodePositions = new();
        private float _resolvedMapScale = 1f;
        private Vector2 _resolvedContentSize = new(1200f, 1600f);

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

            BuildScrollArea(self, null);

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
            UpdateScrollPosition(currentRoom);
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

                    DrawConnection(room, connectedRoom);
                }
            }
        }

        private void DrawConnection(TowerRoomNode fromRoom, TowerRoomNode toRoom)
        {
            Vector2 from = GetNodePosition(fromRoom);
            Vector2 to = GetNodePosition(toRoom);
            float scaledConnectionWidth = connectionWidth * GetMapScale();
            float curveSign = GetConnectionCurveSign(fromRoom.Id, toRoom.Id);

            CreateConnectionLine("ConnectionShadow", from, to, scaledConnectionWidth + 4f, connectionShadowColor, curveSign);
            CreateConnectionLine("Connection", from, to, scaledConnectionWidth, connectionColor, curveSign);
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

            TowerRoomNodeView nodeView = CreateNodeView(room);
            RectTransform nodeRect = nodeView.transform as RectTransform;

            if (nodeRect != null)
            {
                ConfigureMapItemRect(nodeRect);
                nodeRect.sizeDelta = nodeSize * GetMapScale();
                nodeRect.anchoredPosition = GetNodePosition(room);
            }

            nodeView.Apply(
                GetRoomSprite(room.RoomType),
                unknownRoomSprite,
                isRevealed,
                isAvailable,
                isCurrent);

            if (isAvailable)
            {
                int roomId = room.Id;
                nodeView.Clicked += () =>
                {
                    nodeView.PlaySelected();
                    OnRoomSelected?.Invoke(roomId);
                };
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
            nodeView.PointerMoved += eventData =>
            {
                EnsureTooltip();
                roomTooltip?.SetPosition(eventData);
            };
            nodeView.PointerExited += _ => roomTooltip?.Hide();
        }

        private void ShowTooltip(string title, string body, PointerEventData eventData)
        {
            EnsureTooltip();
            roomTooltip?.transform.SetAsLastSibling();
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
                _resolvedMapScale = 1f;
                return;
            }

            int minX = rooms.Min(room => room.GridPosition.x);
            int maxX = rooms.Max(room => room.GridPosition.x);
            int minY = rooms.Min(room => room.GridPosition.y);
            int maxY = rooms.Max(room => room.GridPosition.y);

            float centerY = (minY + maxY) * 0.5f;
            _resolvedMapScale = CalculateAutoMapScale(minY, maxY);
            _resolvedContentSize = CalculateContentSize(minX, maxX, minY, maxY);
            ResizeNodeRoot(_resolvedContentSize);

            foreach (TowerRoomNode room in rooms)
            {
                _nodePositions[room.Id] = new Vector2(
                    (room.GridPosition.y - centerY) * cellSize.x * _resolvedMapScale,
                    (room.GridPosition.x - minX) * GetVerticalCellSize() * _resolvedMapScale + verticalMapPadding + nodeSize.y * _resolvedMapScale * 0.5f);
            }
        }

        private float CalculateAutoMapScale(int minY, int maxY)
        {
            float gridWidth = Mathf.Max(0, maxY - minY) * cellSize.x + nodeSize.x;
            Vector2 rootSize = GetViewportSize();
            float horizontalPadding = Mathf.Max(0f, horizontalMapPadding);
            float availableWidth = Mathf.Max(1f, rootSize.x - horizontalPadding * 2f);

            float scale = Mathf.Min(1f, availableWidth / Mathf.Max(1f, gridWidth));
            return float.IsNaN(scale) || float.IsInfinity(scale) ? 1f : Mathf.Max(0.1f, scale);
        }

        private Vector2 CalculateContentSize(int minX, int maxX, int minY, int maxY)
        {
            Vector2 viewportSize = GetViewportSize();
            float gridWidth = Mathf.Max(0, maxY - minY) * cellSize.x * _resolvedMapScale + nodeSize.x * _resolvedMapScale + horizontalMapPadding * 2f;
            float gridHeight = Mathf.Max(0, maxX - minX) * GetVerticalCellSize() * _resolvedMapScale + nodeSize.y * _resolvedMapScale + verticalMapPadding * 2f;

            return new Vector2(
                Mathf.Max(viewportSize.x, gridWidth),
                Mathf.Max(viewportSize.y, minScrollableContentHeight, gridHeight));
        }

        private void ResizeNodeRoot(Vector2 contentSize)
        {
            if (nodeRoot == null)
                return;

            nodeRoot.anchorMin = new Vector2(0.5f, 0f);
            nodeRoot.anchorMax = new Vector2(0.5f, 0f);
            nodeRoot.pivot = new Vector2(0.5f, 0f);
            nodeRoot.anchoredPosition = Vector2.zero;
            nodeRoot.sizeDelta = contentSize;
        }

        private float GetVerticalCellSize()
        {
            return cellSize.y * Mathf.Max(1f, verticalMapLengthMultiplier);
        }

        private Vector2 GetNodePosition(TowerRoomNode room)
        {
            return room != null && _nodePositions.TryGetValue(room.Id, out Vector2 position)
                ? position
                : Vector2.zero;
        }

        private Vector2 GetViewportSize()
        {
            RectTransform target = viewportRoot != null ? viewportRoot : transform as RectTransform;
            Vector2 size = target != null ? target.rect.size : Vector2.zero;

            if (size.x > 0f && size.y > 0f)
                return size;

            return target != null && target.sizeDelta.sqrMagnitude > 0f ? target.sizeDelta : new Vector2(1100f, 640f);
        }

        private void CreateConnectionLine(string objectName, Vector2 from, Vector2 to, float width, Color color, float curveSign)
        {
            Vector2 delta = to - from;
            if (delta.sqrMagnitude <= 0.01f)
                return;

            RectTransform group = CreateConnectionGroup(objectName);
            Vector2 controlA = GetConnectionControlPoint(from, to, 0.34f, curveSign);
            Vector2 controlB = GetConnectionControlPoint(from, to, 0.66f, -curveSign * 0.72f);
            float length = EstimateBezierLength(from, controlA, controlB, to);
            float dashLength = connectionDashLength * GetMapScale();
            float dashGap = connectionDashGap * GetMapScale();
            float dashStep = Mathf.Max(1f, dashLength + dashGap);
            int dashCount = Mathf.Max(1, Mathf.FloorToInt(length / dashStep));

            for (int i = 0; i < dashCount; ++i)
            {
                float t = dashCount == 1 ? 0.5f : (i + 0.5f) / dashCount;
                Vector2 point = EvaluateBezier(from, controlA, controlB, to, t);
                Vector2 tangent = EvaluateBezierTangent(from, controlA, controlB, to, t);
                CreateConnectionDash(group, point, tangent, dashLength, width, color);
            }

        }

        private RectTransform CreateConnectionGroup(string objectName)
        {
            RectTransform group = CreateRect(objectName, nodeRoot);
            ConfigureMapItemRect(group);
            group.pivot = new Vector2(0.5f, 0f);
            group.anchoredPosition = Vector2.zero;
            group.sizeDelta = _resolvedContentSize;
            return group;
        }

        private void CreateConnectionDash(RectTransform parent, Vector2 position, Vector2 tangent, float length, float width, Color color)
        {
            RectTransform dash = CreateRect("Dash", parent);
            ConfigureMapItemRect(dash);
            dash.sizeDelta = new Vector2(length, width);
            dash.anchoredPosition = position;
            dash.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);

            Image image = dash.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.color = color;
        }

        private Vector2 GetConnectionControlPoint(Vector2 from, Vector2 to, float pathT, float curveSign)
        {
            Vector2 delta = to - from;
            Vector2 perpendicular = new(-delta.y, delta.x);

            if (perpendicular.sqrMagnitude <= 0.01f)
                perpendicular = Vector2.right;
            else
                perpendicular.Normalize();

            float curveOffset = Mathf.Clamp(delta.magnitude * connectionCurveStrength, connectionMinCurveOffset, connectionMaxCurveOffset);
            return Vector2.Lerp(from, to, pathT) + perpendicular * curveOffset * curveSign;
        }

        private static float GetConnectionCurveSign(int fromId, int toId)
        {
            unchecked
            {
                int hash = fromId * 73856093 ^ toId * 19349663;
                return (hash & 1) == 0 ? 1f : -1f;
            }
        }

        private static Vector2 EvaluateBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * oneMinusT * a
                   + 3f * oneMinusT * oneMinusT * t * b
                   + 3f * oneMinusT * t * t * c
                   + t * t * t * d;
        }

        private static Vector2 EvaluateBezierTangent(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            float oneMinusT = 1f - t;
            Vector2 tangent = 3f * oneMinusT * oneMinusT * (b - a)
                              + 6f * oneMinusT * t * (c - b)
                              + 3f * t * t * (d - c);

            return tangent.sqrMagnitude <= 0.01f ? Vector2.up : tangent.normalized;
        }

        private static float EstimateBezierLength(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            const int segmentCount = 24;
            float length = 0f;
            Vector2 previous = a;

            for (int i = 1; i <= segmentCount; ++i)
            {
                Vector2 current = EvaluateBezier(a, b, c, d, i / (float)segmentCount);
                length += Vector2.Distance(previous, current);
                previous = current;
            }

            return length;
        }

        private void EnsureNodeRoot()
        {
            EnsureMapScrollArea();
        }

        private void EnsureMapScrollArea()
        {
            RectTransform self = transform as RectTransform;
            if (self == null)
                return;

            if (mapScrollRect == null)
                mapScrollRect = GetComponentInChildren<ScrollRect>(true);

            if (mapScrollRect != null)
            {
                if (viewportRoot == null)
                    viewportRoot = mapScrollRect.viewport;

                if (nodeRoot == null)
                    nodeRoot = mapScrollRect.content;

                if (viewportRoot == null || nodeRoot == null)
                    BuildScrollArea(self, nodeRoot);
                else
                    ConfigureScrollRect();

                return;
            }

            BuildScrollArea(self, nodeRoot);
        }

        private void BuildScrollArea(RectTransform self, RectTransform existingNodeRoot)
        {
            RectTransform scrollArea = CreateRect("TowerMapScrollView", self);
            CopyMapAreaRect(existingNodeRoot, scrollArea);

            mapScrollRect = scrollArea.gameObject.AddComponent<ScrollRect>();
            mapScrollRect.horizontal = false;
            mapScrollRect.vertical = true;
            mapScrollRect.movementType = ScrollRect.MovementType.Elastic;
            mapScrollRect.inertia = true;
            mapScrollRect.scrollSensitivity = scrollSensitivity;
            ConfigureScrollRaycastTarget(scrollArea);

            viewportRoot = CreateRect("Viewport", scrollArea);
            viewportRoot.anchorMin = Vector2.zero;
            viewportRoot.anchorMax = Vector2.one;
            viewportRoot.offsetMin = Vector2.zero;
            viewportRoot.offsetMax = Vector2.zero;
            viewportRoot.gameObject.AddComponent<RectMask2D>();

            nodeRoot = existingNodeRoot != null ? existingNodeRoot : CreateRect("TowerMapRoot", viewportRoot);
            nodeRoot.SetParent(viewportRoot, false);

            mapScrollRect.viewport = viewportRoot;
            mapScrollRect.content = nodeRoot;
            ConfigureNodeRootForScrollContent();
        }

        private void ConfigureScrollRect()
        {
            mapScrollRect.horizontal = false;
            mapScrollRect.vertical = true;
            mapScrollRect.scrollSensitivity = scrollSensitivity;
            ConfigureScrollRaycastTarget(mapScrollRect.transform as RectTransform);

            if (viewportRoot != null && viewportRoot.GetComponent<RectMask2D>() == null)
                viewportRoot.gameObject.AddComponent<RectMask2D>();

            if (nodeRoot != null && viewportRoot != null && nodeRoot.parent != viewportRoot)
                nodeRoot.SetParent(viewportRoot, false);

            ConfigureNodeRootForScrollContent();
        }

        private void ConfigureScrollRaycastTarget(RectTransform target)
        {
            if (target == null)
                return;

            Image image = GetOrAdd<Image>(target.gameObject);
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
        }

        private void ConfigureNodeRootForScrollContent()
        {
            if (nodeRoot == null)
                return;

            ConfigureMapItemRect(nodeRoot);
            nodeRoot.pivot = new Vector2(0.5f, 0f);
            nodeRoot.anchoredPosition = Vector2.zero;

            if (nodeRoot.sizeDelta.sqrMagnitude <= 0f)
                nodeRoot.sizeDelta = _resolvedContentSize;
        }

        private static void ConfigureMapItemRect(RectTransform rect)
        {
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private void CopyMapAreaRect(RectTransform source, RectTransform target)
        {
            if (target == null)
                return;

            if (source != null)
            {
                target.SetSiblingIndex(source.GetSiblingIndex());
                target.anchorMin = source.anchorMin;
                target.anchorMax = source.anchorMax;
                target.offsetMin = source.offsetMin;
                target.offsetMax = source.offsetMax;
                target.pivot = source.pivot;
                return;
            }

            target.anchorMin = new Vector2(0f, 0f);
            target.anchorMax = new Vector2(1f, 1f);
            target.offsetMin = new Vector2(0f, 104f);
            target.offsetMax = new Vector2(0f, -148f);
            target.pivot = new Vector2(0.5f, 0.5f);
        }

        private void UpdateScrollPosition(TowerRoomNode currentRoom)
        {
            if (!scrollToCurrentRoom || mapScrollRect == null || viewportRoot == null || nodeRoot == null || currentRoom == null)
                return;

            Canvas.ForceUpdateCanvases();

            float contentHeight = Mathf.Max(1f, nodeRoot.rect.height);
            float viewportHeight = Mathf.Max(1f, viewportRoot.rect.height);
            float scrollableHeight = contentHeight - viewportHeight;

            if (scrollableHeight <= 0f)
            {
                mapScrollRect.verticalNormalizedPosition = 0f;
                return;
            }

            float currentY = GetNodePosition(currentRoom).y;
            float targetOffset = Mathf.Clamp(currentY - viewportHeight * 0.34f, 0f, scrollableHeight);
            mapScrollRect.verticalNormalizedPosition = Mathf.InverseLerp(0f, scrollableHeight, targetOffset);
        }

        private float GetMapScale()
            => Mathf.Max(0.1f, _resolvedMapScale);

        private void EnsureTooltip()
        {
            RectTransform parent = GetTooltipParent();

            if (roomTooltip != null && roomTooltip.gameObject.scene.IsValid())
            {
                if (nodeRoot != null && roomTooltip.transform.IsChildOf(nodeRoot))
                    roomTooltip.transform.SetParent(parent, false);

                roomTooltip.transform.SetAsLastSibling();
                return;
            }

            TowerRoomTooltip tooltipPrefab = roomTooltip;
            roomTooltip = tooltipPrefab != null
                ? Instantiate(tooltipPrefab, parent)
                : TowerRoomTooltip.CreateDefault(parent);

            roomTooltip.gameObject.name = tooltipPrefab != null
                ? tooltipPrefab.gameObject.name
                : "Room Hover Popup";
            roomTooltip.transform.SetAsLastSibling();
            roomTooltip.Hide();
        }

        private RectTransform GetTooltipParent()
        {
            RectTransform self = transform as RectTransform;

            if (self != null)
                return self;

            return nodeRoot;
        }

        private void ClearNodeRoot()
        {
            for (int i = nodeRoot.childCount - 1; i >= 0; --i)
            {
                Transform child = nodeRoot.GetChild(i);

                if (IsTooltipTransform(child))
                    continue;

                Destroy(child.gameObject);
            }
        }

        private bool IsTooltipTransform(Transform child)
        {
            return roomTooltip != null &&
                   (child == roomTooltip.transform ||
                    child.IsChildOf(roomTooltip.transform) ||
                    roomTooltip.transform.IsChildOf(child));
        }

        private static TowerRoomNodeView CreateFallbackNodeView(Transform parent)
        {
            RectTransform root = CreateRect("TowerNode", parent);

            RectTransform iconRect = CreateRect("Room Icon", root);
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(12f, 12f);
            iconRect.offsetMax = new Vector2(-12f, -12f);
            Image iconImage = iconRect.gameObject.AddComponent<Image>();
            iconImage.color = Color.white;

            Button button = iconRect.gameObject.AddComponent<Button>();
            button.targetGraphic = iconImage;
            button.transition = Selectable.Transition.None;

            RectTransform selectedRect = CreateRect("RoomIcon_Selected", root);
            selectedRect.anchorMin = Vector2.zero;
            selectedRect.anchorMax = Vector2.one;
            selectedRect.offsetMin = new Vector2(12f, 12f);
            selectedRect.offsetMax = new Vector2(-12f, -12f);
            Image selectedImage = selectedRect.gameObject.AddComponent<Image>();
            selectedImage.raycastTarget = false;
            selectedImage.color = Color.black;
            selectedImage.type = Image.Type.Filled;
            selectedImage.fillMethod = Image.FillMethod.Radial360;
            selectedImage.fillAmount = 0f;

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

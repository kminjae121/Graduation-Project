using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Tower.UI
{
    public class TowerNodeMapView : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI floorText;
        [SerializeField] private TextMeshProUGUI roomText;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private TextMeshProUGUI detailTitleText;
        [SerializeField] private TextMeshProUGUI detailBodyText;

        [Header("Map Roots")]
        [SerializeField] private RectTransform nodeRoot;
        [SerializeField] private RectTransform detailPanelRoot;

        [Header("Layout")]
        [SerializeField] private Vector2 cellSize = new(260f, 190f);
        [SerializeField] private Vector2 nodeSize = new(138f, 138f);
        [SerializeField] private float connectionWidth = 8f;

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new(0.025f, 0.027f, 0.038f, 1f);
        [SerializeField] private Color panelColor = new(0.055f, 0.06f, 0.085f, 0.92f);
        [SerializeField] private Color currentRoomColor = new(0.98f, 0.95f, 0.78f);
        [SerializeField] private Color availableConnectionColor = new(0.74f, 0.92f, 1f, 0.88f);
        [SerializeField] private Color lockedConnectionColor = new(0.27f, 0.31f, 0.42f, 0.55f);
        [SerializeField] private Color lockedRoomColor = new(0.28f, 0.31f, 0.43f, 0.72f);
        [SerializeField] private Color clearedRoomColor = new(0.46f, 0.82f, 0.7f, 0.9f);

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
            background.color = backgroundColor;

            CreateBackdropBand("TopBand", self, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -138f), new Vector2(0f, 0f), new Color(0.02f, 0.022f, 0.035f, 0.9f));
            CreateBackdropBand("BottomBand", self, Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 94f), new Color(0.02f, 0.022f, 0.035f, 0.72f));

            floorText = CreateText("FloorText", self, new Vector2(54f, -34f), new Vector2(520f, -90f), 36f, FontStyles.Bold);
            floorText.alignment = TextAlignmentOptions.Left;

            roomText = CreateText("RoomText", self, new Vector2(54f, -84f), new Vector2(780f, -128f), 20f, FontStyles.Normal);
            roomText.alignment = TextAlignmentOptions.Left;

            hintText = CreateText("HintText", self, new Vector2(54f, 24f), new Vector2(-54f, 74f), 18f, FontStyles.Normal);
            hintText.alignment = TextAlignmentOptions.Center;

            nodeRoot = CreateRect("DreamGateRoot", self);
            nodeRoot.anchorMin = new Vector2(0.5f, 0.5f);
            nodeRoot.anchorMax = new Vector2(0.5f, 0.5f);
            nodeRoot.pivot = new Vector2(0.5f, 0.5f);
            nodeRoot.anchoredPosition = new Vector2(-110f, -8f);
            nodeRoot.sizeDelta = new Vector2(1100f, 640f);

            detailPanelRoot = CreatePanel("RoomDetailPanel", self, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-420f, -205f), new Vector2(-54f, 205f), panelColor);
            detailTitleText = CreateText("DetailTitle", detailPanelRoot, new Vector2(26f, -28f), new Vector2(-26f, -86f), 24f, FontStyles.Bold);
            detailTitleText.alignment = TextAlignmentOptions.Left;
            detailBodyText = CreateText("DetailBody", detailPanelRoot, new Vector2(26f, -96f), new Vector2(-26f, -352f), 17f, FontStyles.Normal);
            detailBodyText.alignment = TextAlignmentOptions.TopLeft;
            detailBodyText.textWrappingMode = TextWrappingModes.Normal;
        }

        public void Render(TowerFloorMap map)
        {
            if (map == null)
                return;

            EnsureNodeRoot();
            ClearNodeRoot();

            TowerRoomNode currentRoom = map.GetCurrentRoom();

            if (currentRoom == null)
                return;

            List<TowerRoomNode> visibleRooms = map.Rooms
                .Where(room => room.Id == currentRoom.Id || currentRoom.IsConnectedTo(room.Id))
                .OrderBy(room => room.Id == currentRoom.Id ? -1 : room.Id)
                .ToList();

            UpdateHeader(map, currentRoom, visibleRooms);

            foreach (TowerRoomNode room in visibleRooms)
                DrawConnection(map, currentRoom, room);

            foreach (TowerRoomNode room in visibleRooms)
                DrawRoomNode(map, currentRoom, room);
        }

        private void UpdateHeader(TowerFloorMap map, TowerRoomNode currentRoom, IReadOnlyCollection<TowerRoomNode> visibleRooms)
        {
            string roomName = TowerRoomTypePresentation.GetDisplayName(currentRoom.RoomType);
            int exits = visibleRooms.Count(room => room.Id != currentRoom.Id);

            if (floorText != null)
                floorText.text = $"탑 {map.FloorKey.DisplayName}";

            if (roomText != null)
            {
                roomText.text = currentRoom.IsCleared
                    ? $"{roomName} 클리어 - 연결된 꿈의 문 {exits}개"
                    : $"{roomName} 진행 중";
            }

            if (hintText != null)
            {
                hintText.text = currentRoom.IsCleared
                    ? "빛이 이어진 방을 선택해 다음 공간으로 이동하세요."
                    : "현재 방을 클리어하면 다음 방으로 이어지는 길이 열립니다.";
            }

            if (detailTitleText != null)
                detailTitleText.text = roomName;

            if (detailBodyText != null)
            {
                string stateText = currentRoom.IsCleared ? "클리어됨" : currentRoom.IsVisited ? "진행 중" : "미방문";
                detailBodyText.text =
                    $"{TowerRoomTypePresentation.GetDescription(currentRoom.RoomType)}\n\n" +
                    $"현재 상태: {stateText}\n" +
                    $"이동 가능 방: {(currentRoom.IsCleared ? exits : 0)}개\n\n" +
                    "현재 위치는 중앙에 표시되고, 바로 이동할 수 있는 방만 주변 꿈의 문으로 드러납니다.";
            }
        }

        private void DrawConnection(TowerFloorMap map, TowerRoomNode currentRoom, TowerRoomNode room)
        {
            if (room.Id == currentRoom.Id)
                return;

            Vector2 from = Vector2.zero;
            Vector2 to = ToAnchoredPosition(currentRoom, room);
            bool available = currentRoom.IsCleared && map.CanMoveTo(room.Id);

            CreateConnectionLine("ConnectionShadow", from, to, connectionWidth + 7f, new Color(0f, 0f, 0f, 0.46f));
            CreateConnectionLine("Connection", from, to, connectionWidth, available ? availableConnectionColor : lockedConnectionColor);

            if (available)
                CreateConnectionLine("ConnectionGlow", from, to, 2.5f, Color.white);
        }

        private void DrawRoomNode(TowerFloorMap map, TowerRoomNode currentRoom, TowerRoomNode room)
        {
            bool isCurrent = room.Id == currentRoom.Id;
            bool isAvailable = !isCurrent && currentRoom.IsCleared && map.CanMoveTo(room.Id);

            RectTransform wrapper = CreateRect($"DreamGate_{room.Id}", nodeRoot);
            wrapper.sizeDelta = nodeSize;
            wrapper.anchoredPosition = isCurrent ? Vector2.zero : ToAnchoredPosition(currentRoom, room);

            Image plate = wrapper.gameObject.AddComponent<Image>();
            plate.color = GetNodeColor(room, isCurrent, isAvailable);

            Shadow shadow = wrapper.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.58f);
            shadow.effectDistance = new Vector2(0f, -7f);

            Outline outline = wrapper.gameObject.AddComponent<Outline>();
            outline.effectColor = isCurrent ? new Color(1f, 0.96f, 0.62f, 0.95f) : new Color(1f, 1f, 1f, isAvailable ? 0.55f : 0.18f);
            outline.effectDistance = new Vector2(3f, -3f);

            Button button = wrapper.gameObject.AddComponent<Button>();
            button.targetGraphic = plate;
            button.interactable = isAvailable;

            int roomId = room.Id;
            button.onClick.AddListener(() => OnRoomSelected?.Invoke(roomId));

            TextMeshProUGUI typeText = CreateText("Type", wrapper, new Vector2(0f, -10f), new Vector2(0f, -72f), isCurrent ? 34f : 30f, FontStyles.Bold);
            typeText.alignment = TextAlignmentOptions.Center;
            typeText.color = isCurrent ? new Color(0.1f, 0.08f, 0.02f) : Color.white;
            typeText.text = TowerRoomTypePresentation.GetShortName(room.RoomType);

            TextMeshProUGUI nameLabel = CreateText("Name", wrapper, new Vector2(-18f, -72f), new Vector2(18f, -106f), 14f, FontStyles.Bold);
            nameLabel.alignment = TextAlignmentOptions.Center;
            nameLabel.color = isCurrent ? new Color(0.13f, 0.11f, 0.04f) : Color.white;
            nameLabel.text = TowerRoomTypePresentation.GetDisplayName(room.RoomType);
            nameLabel.textWrappingMode = TextWrappingModes.Normal;

            TextMeshProUGUI stateLabel = CreateText("State", wrapper, new Vector2(-18f, 14f), new Vector2(18f, 42f), 12f, FontStyles.Bold);
            stateLabel.alignment = TextAlignmentOptions.Center;
            stateLabel.color = isAvailable ? new Color(0.15f, 0.95f, 0.78f) : isCurrent ? new Color(0.1f, 0.08f, 0.02f) : new Color(0.68f, 0.72f, 0.84f);
            stateLabel.text = GetStateLabel(room, isCurrent, isAvailable);
        }

        private Color GetNodeColor(TowerRoomNode room, bool isCurrent, bool isAvailable)
        {
            if (isCurrent)
                return currentRoomColor;

            if (room.IsCleared)
                return clearedRoomColor;

            if (!isAvailable)
                return lockedRoomColor;

            Color roomColor = TowerRoomTypePresentation.GetColor(room.RoomType);
            roomColor.a = 0.94f;
            return roomColor;
        }

        private static string GetStateLabel(TowerRoomNode room, bool isCurrent, bool isAvailable)
        {
            if (isCurrent)
                return "현재 위치";

            if (room.IsCleared)
                return "클리어";

            return isAvailable ? "이동 가능" : "잠김";
        }

        private Vector2 ToAnchoredPosition(TowerRoomNode currentRoom, TowerRoomNode room)
        {
            Vector2 delta = room.GridPosition - currentRoom.GridPosition;

            if (delta.sqrMagnitude < 0.001f)
                return Vector2.zero;

            return new Vector2(delta.x * cellSize.x, delta.y * cellSize.y);
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

            if (nodeRoot == null)
            {
                nodeRoot = CreateRect("DreamGateRoot", self != null ? self : transform);
                nodeRoot.anchorMin = new Vector2(0.5f, 0.5f);
                nodeRoot.anchorMax = new Vector2(0.5f, 0.5f);
                nodeRoot.pivot = new Vector2(0.5f, 0.5f);
                nodeRoot.anchoredPosition = Vector2.zero;
                nodeRoot.sizeDelta = new Vector2(1100f, 640f);
            }
        }

        private void ClearNodeRoot()
        {
            for (int i = nodeRoot.childCount - 1; i >= 0; --i)
                Destroy(nodeRoot.GetChild(i).gameObject);
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

        private static RectTransform CreatePanel(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            RectTransform rect = CreateRect(objectName, parent);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;

            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.13f);
            outline.effectDistance = new Vector2(2f, -2f);
            return rect;
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

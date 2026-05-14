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
        [SerializeField] private TextMeshProUGUI floorText;
        [SerializeField] private TextMeshProUGUI roomText;
        [SerializeField] private RectTransform nodeRoot;
        [SerializeField] private Vector2 cellSize = new(150f, 120f);
        [SerializeField] private Vector2 nodeSize = new(96f, 72f);
        [SerializeField] private Color currentRoomColor = Color.white;
        [SerializeField] private Color availableConnectionColor = new(1f, 1f, 1f, 0.85f);

        public event Action<int> OnRoomSelected;

        public void BuildDefaultLayout(RectTransform parent)
        {
            RectTransform self = gameObject.GetComponent<RectTransform>();
            self.SetParent(parent, false);
            self.anchorMin = Vector2.zero;
            self.anchorMax = Vector2.one;
            self.offsetMin = Vector2.zero;
            self.offsetMax = Vector2.zero;

            Image background = gameObject.AddComponent<Image>();
            background.color = new Color(0.03f, 0.035f, 0.045f, 0.9f);

            floorText = CreateText("FloorText", self, new Vector2(48f, -40f), new Vector2(-48f, -100f), 34f, FontStyles.Bold);
            floorText.alignment = TextAlignmentOptions.Center;

            roomText = CreateText("RoomText", self, new Vector2(48f, -104f), new Vector2(-48f, -158f), 20f, FontStyles.Normal);
            roomText.alignment = TextAlignmentOptions.Center;

            nodeRoot = CreateRect("NodeRoot", self);
            nodeRoot.anchorMin = new Vector2(0.5f, 0.5f);
            nodeRoot.anchorMax = new Vector2(0.5f, 0.5f);
            nodeRoot.pivot = new Vector2(0.5f, 0.5f);
            nodeRoot.anchoredPosition = new Vector2(0f, -32f);
            nodeRoot.sizeDelta = new Vector2(900f, 560f);
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

            if (floorText != null)
                floorText.text = $"탑 {map.FloorKey.DisplayName}층";

            if (roomText != null)
            {
                roomText.text = currentRoom.IsCleared
                    ? $"{TowerRoomTypePresentation.GetDisplayName(currentRoom.RoomType)} 완료 - 연결된 방을 선택하세요."
                    : $"{TowerRoomTypePresentation.GetDisplayName(currentRoom.RoomType)} 진행 중";
            }

            List<TowerRoomNode> visibleRooms = map.Rooms
                .Where(room => room.Id == currentRoom.Id || currentRoom.IsConnectedTo(room.Id))
                .ToList();

            Vector2 center = CalculateCenter(visibleRooms);

            foreach (TowerRoomNode room in visibleRooms)
                DrawConnection(currentRoom, room, center);

            foreach (TowerRoomNode room in visibleRooms)
                DrawRoomNode(map, currentRoom, room, center);
        }

        private void DrawConnection(TowerRoomNode currentRoom, TowerRoomNode room, Vector2 center)
        {
            if (room.Id == currentRoom.Id)
                return;

            Vector2 from = ToAnchoredPosition(currentRoom.GridPosition, center);
            Vector2 to = ToAnchoredPosition(room.GridPosition, center);
            Vector2 delta = to - from;

            RectTransform line = CreateRect("Connection", nodeRoot);
            line.sizeDelta = new Vector2(delta.magnitude, 5f);
            line.anchoredPosition = from + delta * 0.5f;
            line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            line.SetAsFirstSibling();

            Image image = line.gameObject.AddComponent<Image>();
            image.color = availableConnectionColor;
        }

        private void DrawRoomNode(TowerFloorMap map, TowerRoomNode currentRoom, TowerRoomNode room, Vector2 center)
        {
            RectTransform rect = CreateRect($"Room_{room.Id}", nodeRoot);
            rect.sizeDelta = nodeSize;
            rect.anchoredPosition = ToAnchoredPosition(room.GridPosition, center);

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = room.Id == currentRoom.Id ? currentRoomColor : TowerRoomTypePresentation.GetColor(room.RoomType);

            Button button = rect.gameObject.AddComponent<Button>();
            button.interactable = map.CanMoveTo(room.Id) && currentRoom.IsCleared;

            int roomId = room.Id;
            button.onClick.AddListener(() => OnRoomSelected?.Invoke(roomId));

            TextMeshProUGUI shortLabel = CreateText("ShortLabel", rect, Vector2.zero, Vector2.zero, 24f, FontStyles.Bold);
            shortLabel.text = TowerRoomTypePresentation.GetShortName(room.RoomType);
            shortLabel.alignment = TextAlignmentOptions.Center;
            shortLabel.color = Color.black;

            TextMeshProUGUI nameLabel = CreateText("NameLabel", rect, new Vector2(-20f, -34f), new Vector2(20f, -86f), 15f, FontStyles.Bold);
            nameLabel.text = TowerRoomTypePresentation.GetDisplayName(room.RoomType);
            nameLabel.alignment = TextAlignmentOptions.Center;
            nameLabel.color = Color.white;
        }

        private Vector2 CalculateCenter(IReadOnlyCollection<TowerRoomNode> rooms)
        {
            if (rooms == null || rooms.Count == 0)
                return Vector2.zero;

            float minX = rooms.Min(room => room.GridPosition.x);
            float maxX = rooms.Max(room => room.GridPosition.x);
            float minY = rooms.Min(room => room.GridPosition.y);
            float maxY = rooms.Max(room => room.GridPosition.y);

            return new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        }

        private Vector2 ToAnchoredPosition(Vector2Int gridPosition, Vector2 center)
            => new((gridPosition.x - center.x) * cellSize.x, (gridPosition.y - center.y) * cellSize.y);

        private void EnsureNodeRoot()
        {
            if (nodeRoot != null)
                return;

            nodeRoot = CreateRect("NodeRoot", transform);
            nodeRoot.anchorMin = Vector2.zero;
            nodeRoot.anchorMax = Vector2.one;
            nodeRoot.offsetMin = Vector2.zero;
            nodeRoot.offsetMax = Vector2.zero;
        }

        private void ClearNodeRoot()
        {
            for (int i = nodeRoot.childCount - 1; i >= 0; --i)
                Destroy(nodeRoot.GetChild(i).gameObject);
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
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }
    }
}

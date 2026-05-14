using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Tower.UI
{
    public class TowerPortalChoicePanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button nextFloorButton;
        [SerializeField] private Button returnLobbyButton;

        public event Action OnNextFloorSelected;
        public event Action OnReturnLobbySelected;

        public void BuildDefaultLayout(RectTransform parent)
        {
            RectTransform self = gameObject.GetComponent<RectTransform>();
            self.SetParent(parent, false);
            self.anchorMin = new Vector2(0.5f, 0.5f);
            self.anchorMax = new Vector2(0.5f, 0.5f);
            self.pivot = new Vector2(0.5f, 0.5f);
            self.anchoredPosition = Vector2.zero;
            self.sizeDelta = new Vector2(460f, 260f);

            root = gameObject;

            Image background = gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.72f);

            titleText = CreateText("Title", self, new Vector2(24f, -24f), new Vector2(-24f, -82f), 24f, FontStyles.Bold);
            titleText.alignment = TextAlignmentOptions.Center;

            nextFloorButton = CreateButton("NextFloorButton", self, new Vector2(40f, -118f), new Vector2(-40f, -166f), "다음 층으로 이동");
            returnLobbyButton = CreateButton("ReturnLobbyButton", self, new Vector2(40f, -178f), new Vector2(-40f, -226f), "정비하러 복귀");

            WireButtons();
            Hide();
        }

        private void Awake()
        {
            WireButtons();
        }

        private void OnDestroy()
        {
            if (nextFloorButton != null)
                nextFloorButton.onClick.RemoveListener(HandleNextFloorButton);

            if (returnLobbyButton != null)
                returnLobbyButton.onClick.RemoveListener(HandleReturnLobbyButton);
        }

        public void Show(TowerFloorKey floorKey, bool isBossPortal)
        {
            if (root == null)
                root = gameObject;

            root.SetActive(true);

            if (titleText != null)
                titleText.text = isBossPortal
                    ? $"탑 {floorKey.DisplayName}층 보스 격파"
                    : $"탑 {floorKey.DisplayName}층 포탈 발견";
        }

        public void Hide()
        {
            if (root == null)
                root = gameObject;

            root.SetActive(false);
        }

        private void WireButtons()
        {
            if (nextFloorButton != null)
            {
                nextFloorButton.onClick.RemoveListener(HandleNextFloorButton);
                nextFloorButton.onClick.AddListener(HandleNextFloorButton);
            }

            if (returnLobbyButton != null)
            {
                returnLobbyButton.onClick.RemoveListener(HandleReturnLobbyButton);
                returnLobbyButton.onClick.AddListener(HandleReturnLobbyButton);
            }
        }

        private void HandleNextFloorButton()
            => OnNextFloorSelected?.Invoke();

        private void HandleReturnLobbyButton()
            => OnReturnLobbySelected?.Invoke();

        private static Button CreateButton(string objectName, Transform parent, Vector2 offsetMin, Vector2 offsetMax, string label)
        {
            RectTransform rect = CreateRect(objectName, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.9f, 0.82f, 0.58f);

            Button button = rect.gameObject.AddComponent<Button>();

            TextMeshProUGUI text = CreateText("Label", rect, Vector2.zero, Vector2.zero, 18f, FontStyles.Bold);
            text.text = label;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            return button;
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
            return text;
        }
    }
}

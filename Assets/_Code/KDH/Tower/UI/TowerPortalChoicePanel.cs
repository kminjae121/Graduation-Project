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
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button nextFloorButton;
        [SerializeField] private Button returnLobbyButton;

        public event Action OnNextFloorSelected;
        public event Action OnReturnLobbySelected;

        public void BuildDefaultLayout(RectTransform parent)
        {
            RectTransform self = GetComponent<RectTransform>();
            self.SetParent(parent, false);
            self.anchorMin = Vector2.zero;
            self.anchorMax = Vector2.one;
            self.offsetMin = Vector2.zero;
            self.offsetMax = Vector2.zero;

            root = gameObject;

            Image overlay = gameObject.AddComponent<Image>();
            overlay.color = new Color(0.01f, 0.012f, 0.02f, 0.72f);

            RectTransform panel = CreatePanel("PortalPanel", self, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-270f, -190f), new Vector2(270f, 190f), new Color(0.055f, 0.06f, 0.09f, 0.96f));

            titleText = CreateText("Title", panel, new Vector2(32f, -30f), new Vector2(-32f, -86f), 27f, FontStyles.Bold);
            titleText.alignment = TextAlignmentOptions.Center;

            descriptionText = CreateText("Description", panel, new Vector2(40f, -98f), new Vector2(-40f, -170f), 17f, FontStyles.Normal);
            descriptionText.alignment = TextAlignmentOptions.Center;
            descriptionText.textWrappingMode = TextWrappingModes.Normal;

            nextFloorButton = CreateButton("NextFloorButton", panel, new Vector2(46f, -206f), new Vector2(-46f, -260f), "다음 층으로 이동");
            returnLobbyButton = CreateButton("ReturnLobbyButton", panel, new Vector2(46f, -276f), new Vector2(-46f, -330f), "로비로 귀환");

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
            transform.SetAsLastSibling();

            if (titleText != null)
                titleText.text = isBossPortal ? $"{floorKey.DisplayName} 보스 격파" : $"{floorKey.DisplayName} 포탈 발견";

            if (descriptionText != null)
            {
                descriptionText.text = isBossPortal
                    ? "보스방에 열린 포탈이 다음 꿈으로 이어집니다."
                    : "포탈의 빛이 안정되었습니다. 더 깊이 내려가거나 원정을 마칠 수 있습니다.";
            }
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
            image.color = new Color(0.82f, 0.92f, 1f, 0.95f);

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            TextMeshProUGUI text = CreateText("Label", rect, Vector2.zero, Vector2.zero, 18f, FontStyles.Bold);
            text.text = label;
            text.color = new Color(0.05f, 0.06f, 0.09f);
            text.alignment = TextAlignmentOptions.Center;
            return button;
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
            outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
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
            return text;
        }
    }
}

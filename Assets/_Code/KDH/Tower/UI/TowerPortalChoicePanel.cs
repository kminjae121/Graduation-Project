using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Tower.UI
{
    public class TowerPortalChoicePanel : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Buttons")]
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

            Image overlay = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            overlay.color = new Color(0.01f, 0.012f, 0.02f, 0.72f);
            overlay.raycastTarget = true;

            RectTransform panel = CreatePanel(
                "PortalPanel",
                self,
                new Vector2(0.5f, 0.5f),
                new Vector2(540f, 380f),
                new Color(0.055f, 0.06f, 0.09f, 0.96f));

            titleText = CreateText("Title", panel, new Vector2(32f, -86f), new Vector2(-32f, -30f), 27f, FontStyles.Bold);
            titleText.alignment = TextAlignmentOptions.Center;

            descriptionText = CreateText("Description", panel, new Vector2(40f, -176f), new Vector2(-40f, -98f), 17f, FontStyles.Normal);
            descriptionText.alignment = TextAlignmentOptions.Center;
            descriptionText.textWrappingMode = TextWrappingModes.Normal;

            nextFloorButton = CreateButton("NextFloorButton", panel, new Vector2(46f, -260f), new Vector2(-46f, -206f), "다음 층으로 이동");
            returnLobbyButton = CreateButton("ReturnLobbyButton", panel, new Vector2(46f, -330f), new Vector2(-46f, -276f), "로비로 귀환");

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
            SetButtonsInteractable(true);
            WireButtons();

            if (titleText != null)
                titleText.text = isBossPortal ? $"{floorKey.DisplayName} 보스 격파" : $"{floorKey.DisplayName} 포탈 발견";

            if (descriptionText != null)
            {
                descriptionText.text = isBossPortal
                    ? "보스방에 열린 포탈이 다음 구역으로 이어집니다."
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
        {
            SetButtonsInteractable(false);
            OnNextFloorSelected?.Invoke();
        }

        private void HandleReturnLobbyButton()
        {
            SetButtonsInteractable(false);
            OnReturnLobbySelected?.Invoke();
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (nextFloorButton != null)
                nextFloorButton.interactable = interactable;

            if (returnLobbyButton != null)
                returnLobbyButton.interactable = interactable;
        }

        private static Button CreateButton(string objectName, Transform parent, Vector2 offsetMin, Vector2 offsetMax, string label)
        {
            RectTransform rect = CreateStretchRect(objectName, parent, offsetMin, offsetMax);

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.82f, 0.92f, 1f, 0.95f);
            image.raycastTarget = true;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            TextMeshProUGUI text = CreateText("Label", rect, Vector2.zero, Vector2.zero, 18f, FontStyles.Bold);
            text.text = label;
            text.color = new Color(0.05f, 0.06f, 0.09f);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return button;
        }

        private static RectTransform CreatePanel(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
        {
            RectTransform rect = CreateRect(objectName, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
            outline.effectDistance = new Vector2(2f, -2f);
            return rect;
        }

        private static RectTransform CreateStretchRect(string objectName, Transform parent, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = CreateRect(objectName, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
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
            RectTransform rect = CreateStretchRect(objectName, parent, offsetMin, offsetMax);

            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            return text;
        }
    }
}

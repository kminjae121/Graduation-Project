using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Tower.UI
{
    [DisallowMultipleComponent]
    public sealed class TowerRoomTooltip : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Vector2 screenOffset = new(24f, -18f);
        [SerializeField] private bool clampToParent = true;

        private RectTransform _rectTransform;

        private void Awake()
        {
            ResolveBindings();
            Hide();
        }

        public void Show(string title, string body, PointerEventData eventData)
        {
            ResolveBindings();

            if (titleText != null)
                titleText.text = title;

            if (bodyText != null)
                bodyText.text = body;

            gameObject.SetActive(true);
            GetRoot().gameObject.SetActive(true);
            transform.SetAsLastSibling();
            SetPosition(eventData);
        }

        public void SetPosition(PointerEventData eventData)
        {
            if (eventData == null)
                return;

            ResolveBindings();

            RectTransform positionTarget = GetPositionTarget();
            RectTransform parentRect = positionTarget.parent as RectTransform;

            if (parentRect == null)
                return;

            Camera eventCamera = eventData.pressEventCamera;
            if (eventCamera == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    eventCamera = canvas.worldCamera;
            }

            Vector2 screenPosition = eventData.position + screenOffset;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, eventCamera, out Vector2 localPosition))
                return;

            if (clampToParent)
                localPosition = ClampToParent(parentRect, positionTarget, localPosition);

            positionTarget.anchoredPosition = localPosition;
        }

        public void Hide()
            => GetRoot().gameObject.SetActive(false);

        private Vector2 ClampToParent(RectTransform parentRect, RectTransform positionTarget, Vector2 localPosition)
        {
            Vector2 parentSize = parentRect.rect.size;
            Vector2 tooltipSize = GetTooltipSize(positionTarget);
            Vector2 pivot = positionTarget.pivot;

            float minX = -parentSize.x * 0.5f + tooltipSize.x * pivot.x;
            float maxX = parentSize.x * 0.5f - tooltipSize.x * (1f - pivot.x);
            float minY = -parentSize.y * 0.5f + tooltipSize.y * pivot.y;
            float maxY = parentSize.y * 0.5f - tooltipSize.y * (1f - pivot.y);

            localPosition.x = Mathf.Clamp(localPosition.x, minX, maxX);
            localPosition.y = Mathf.Clamp(localPosition.y, minY, maxY);
            return localPosition;
        }

        private RectTransform GetRoot()
        {
            if (root == null)
                root = transform as RectTransform;

            return root;
        }

        private RectTransform GetPositionTarget()
        {
            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;

            return _rectTransform != null ? _rectTransform : GetRoot();
        }

        private Vector2 GetTooltipSize(RectTransform positionTarget)
        {
            RectTransform visualRoot = GetRoot();
            Vector2 visualSize = visualRoot != null ? visualRoot.rect.size : Vector2.zero;

            if (visualSize.x > 1f && visualSize.y > 1f)
                return visualSize;

            return positionTarget.rect.size;
        }

        private void ResolveBindings()
        {
            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;

            if (root == null)
                root = _rectTransform;

            if (titleText == null || bodyText == null)
            {
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);

                if (titleText == null && texts.Length > 0)
                    titleText = texts[0];

                if (bodyText == null && texts.Length > 1)
                    bodyText = texts[1];
            }

            DisableRaycastTargets();
        }

        private void DisableRaycastTargets()
        {
            Graphic[] graphics = GetComponentsInChildren<Graphic>(true);

            foreach (Graphic graphic in graphics)
                if (graphic != null)
                    graphic.raycastTarget = false;
        }

        public static TowerRoomTooltip CreateDefault(RectTransform parent)
        {
            RectTransform root = CreateRect("Room Hover Popup", parent);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0f, 1f);
            root.sizeDelta = new Vector2(300f, 150f);

            Image background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0.03f, 0.035f, 0.045f, 0.96f);

            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.16f);
            outline.effectDistance = new Vector2(2f, -2f);

            TextMeshProUGUI title = CreateText("Title", root, new Vector2(16f, -12f), new Vector2(-16f, -48f), 21f, FontStyles.Bold);
            title.alignment = TextAlignmentOptions.Left;

            TextMeshProUGUI body = CreateText("Body", root, new Vector2(16f, -50f), new Vector2(-16f, -132f), 15f, FontStyles.Normal);
            body.alignment = TextAlignmentOptions.TopLeft;
            body.textWrappingMode = TextWrappingModes.Normal;

            TowerRoomTooltip tooltip = root.gameObject.AddComponent<TowerRoomTooltip>();
            tooltip.root = root;
            tooltip.titleText = title;
            tooltip.bodyText = body;
            tooltip.Hide();
            return tooltip;
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
    }
}

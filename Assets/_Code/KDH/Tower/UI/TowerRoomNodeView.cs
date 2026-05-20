using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Tower.UI
{
    [DisallowMultipleComponent]
    public sealed class TowerRoomNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [Header("Bindings")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image roomIconImage;
        [SerializeField] private GameObject unknownRoomIcon;
        [SerializeField] private GameObject clearedRoomIcon;
        [SerializeField] private Button button;

        public event Action Clicked;
        public event Action<PointerEventData> PointerEntered;
        public event Action<PointerEventData> PointerMoved;
        public event Action<PointerEventData> PointerExited;

        private void Awake()
        {
            ResolveBindings();
            SetClearedIconVisible(false);
        }

        private void OnEnable()
        {
            ResolveBindings();

            if (button != null)
                button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClick);
        }

        public void Apply(
            Sprite roomSprite,
            Sprite unknownSprite,
            bool isRevealed,
            bool isCleared,
            bool isInteractable,
            Color backgroundColor)
        {
            ResolveBindings();

            if (backgroundImage != null)
                backgroundImage.color = backgroundColor;

            Sprite iconSprite = isRevealed ? roomSprite : unknownSprite;
            if (roomIconImage != null)
            {
                roomIconImage.sprite = iconSprite;
                roomIconImage.enabled = iconSprite != null;
                roomIconImage.color = Color.white;
            }

            if (unknownRoomIcon != null)
                unknownRoomIcon.SetActive(!isRevealed && unknownSprite == null);

            SetClearedIconVisible(isCleared);

            if (button != null)
                button.interactable = isInteractable;
        }

        public void OnPointerEnter(PointerEventData eventData)
            => PointerEntered?.Invoke(eventData);

        public void OnPointerMove(PointerEventData eventData)
            => PointerMoved?.Invoke(eventData);

        public void OnPointerExit(PointerEventData eventData)
            => PointerExited?.Invoke(eventData);

        private void HandleClick()
            => Clicked?.Invoke();

        private void SetClearedIconVisible(bool isVisible)
        {
            if (clearedRoomIcon != null)
                clearedRoomIcon.SetActive(isVisible);
        }

        private void ResolveBindings()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();

            if (roomIconImage == null)
                roomIconImage = FindFirstChildImageExcept(backgroundImage);

            if (unknownRoomIcon == null)
                unknownRoomIcon = FindChildByName(transform, "Unknown Room Icon");

            if (clearedRoomIcon == null)
                clearedRoomIcon = FindChildByName(transform, "Cleared Room Icon");
        }

        private Image FindFirstChildImageExcept(Image excludedImage)
        {
            Image[] images = GetComponentsInChildren<Image>(true);

            foreach (Image image in images)
                if (image != null && image != excludedImage)
                    return image;

            return null;
        }

        private static GameObject FindChildByName(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
                return null;

            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child.gameObject;

                GameObject found = FindChildByName(child, childName);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}

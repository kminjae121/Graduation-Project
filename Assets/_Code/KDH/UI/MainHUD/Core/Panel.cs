using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    [DisallowMultipleComponent]
    public class Panel : MonoBehaviour
    {
        [SerializeField] private string id = "";
        [SerializeField] private RectTransform container = null;
        [SerializeField] private Button closeButton = null;

        public string ID => id;
        public bool IsInitialized { get; private set; }
        public bool IsOpen { get; private set; }

        public Canvas Canvas { get; set; }

        public virtual void Awake()
        {
            Initialize();
            WireCloseButton();
            PanelManager.Register(this);
        }

        protected virtual void OnDestroy()
        {
            UnwireCloseButton();
            PanelManager.Unregister(this);
        }

        public virtual void Initialize()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;
            Close();
        }

        public virtual void Open()
        {
            if (!IsInitialized)
                Initialize();

            transform.SetAsLastSibling();

            if (container != null)
                container.gameObject.SetActive(true);

            IsOpen = true;
        }

        public virtual void Close()
        {
            if (!IsInitialized)
                Initialize();

            if (container != null)
                container.gameObject.SetActive(false);

            IsOpen = false;
        }

        protected void WireCloseButton()
        {
            if (closeButton == null)
                closeButton = FindCloseButton();

            if (closeButton == null)
                return;

            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }

        protected void UnwireCloseButton()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);
        }

        private Button FindCloseButton()
        {
            Transform searchRoot = container != null ? container : transform;
            return FindChildButtonByName(searchRoot, "CloseButton");
        }

        private static Button FindChildButtonByName(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
                return null;

            foreach (Transform child in parent)
            {
                if (child.name == childName && child.TryGetComponent(out Button button))
                    return button;

                Button found = FindChildButtonByName(child, childName);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}

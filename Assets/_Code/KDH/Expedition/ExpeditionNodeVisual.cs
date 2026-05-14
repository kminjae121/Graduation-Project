using UnityEngine;
using UnityEngine.UI;

namespace Code.Expedition.Components
{
    public class ExpeditionNodeVisual : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private ExpeditionNode expeditionNode;

        [Header("Animation Settings")]
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float floatHeight = 0.5f;
        [SerializeField] private bool useBillboard = true;

        private Vector3 _initialLocalPos;
        private Camera _mainCamera;
        private RectTransform _iconRectTransform;

        private void Start()
        {
            _mainCamera = Camera.main;

            if (expeditionNode == null)
                expeditionNode = GetComponentInParent<ExpeditionNode>();

            if (iconImage != null)
            {
                _iconRectTransform = iconImage.GetComponent<RectTransform>();
                _initialLocalPos = _iconRectTransform.localPosition;
            }

            UpdateVisual();
        }

        private void Update()
        {
            HandleFloating();
            HandleBillboarding();
        }

        public void UpdateVisual()
        {
            if (expeditionNode != null && expeditionNode.NodeData != null)
            {
                if (iconImage != null)
                {
                    iconImage.sprite = expeditionNode.NodeData.icon;

                    if (iconImage.sprite == null)
                        iconImage.enabled = false;
                    else
                        iconImage.enabled = true;
                }
            }
        }

        public void SetIconColor(Color color)
        {
            if (iconImage != null)
            {
                iconImage.color = color;
            }
        }

        private void HandleFloating()
        {
            if (_iconRectTransform == null) return;

            float newY = _initialLocalPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            _iconRectTransform.localPosition = new Vector3(_initialLocalPos.x, newY, _initialLocalPos.z);
        }

        private void HandleBillboarding()
        {
            if (useBillboard && _mainCamera != null)
            {
                transform.rotation = _mainCamera.transform.rotation;
            }
        }
    }
}
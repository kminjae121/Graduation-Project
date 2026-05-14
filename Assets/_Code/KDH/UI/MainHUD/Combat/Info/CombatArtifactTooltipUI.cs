using Code.Core.Events.Bus;
using Code.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class CombatArtifactTooltipUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private RectTransform tooltipRect;
        [SerializeField] private TextMeshProUGUI artifactNameText;
        [SerializeField] private Image artifactIcon;

        [Header("Settings")]
        [SerializeField] private Vector2 offset = new Vector2(20f, -20f);

        private void Awake()
        {
            if (tooltipRect == null)
            {
                tooltipRect = GetComponent<RectTransform>();
            }
            
            HideTooltip();
            Bus<CombatArtifactHoverEvent>.Subscribe(HandleArtifactHover);
        }

        private void OnDestroy()
        {
            Bus<CombatArtifactHoverEvent>.Unsubscribe(HandleArtifactHover);
        }

        private void Update()
        {
            if (tooltipRect != null && tooltipRect.gameObject.activeSelf)
            {
                Vector2 mousePos = UnityEngine.Input.mousePosition;
                tooltipRect.position = mousePos + offset;
            }
        }

        private void HandleArtifactHover(CombatArtifactHoverEvent evt)
        {
            if (evt.IsShow && evt.Artifact != null)
            {
                ShowTooltip(evt.Artifact);
            }
            else
            {
                HideTooltip();
            }
        }

        private void ShowTooltip(ItemSO artifact)
        {
            if (artifactNameText != null)
            {
                artifactNameText.text = artifact.name;
            }
            
            if (artifactIcon != null && artifact.itemIcon != null)
            {
                artifactIcon.sprite = artifact.itemIcon;
            }

            if (tooltipRect != null)
            {
                tooltipRect.gameObject.SetActive(true);
            }
        }

        private void HideTooltip()
        {
            if (tooltipRect != null)
            {
                tooltipRect.gameObject.SetActive(false);
            }
        }
    }
}
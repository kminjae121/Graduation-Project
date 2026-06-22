using Code.Core.Events.Bus;
using Code.SkillSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CombatSkillPopupUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image skillIconImage;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private TextMeshProUGUI skillDescText;
        [SerializeField] private TextMeshProUGUI skillCostText;
        [SerializeField] private TextMeshProUGUI skillDamageText;
        [SerializeField] private TextMeshProUGUI skillRangeText;

        [Header("Position")]
        [SerializeField] private Vector2 popupOffset;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private Canvas _canvas;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _parentRectTransform = _rectTransform.parent as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
            
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            
            Bus<CombatSkillHoverEvent>.Subscribe(HandleHoverUI);

            Hide();
        }

        private void OnDestroy()
        {
            Bus<CombatSkillHoverEvent>.Unsubscribe(HandleHoverUI);
        }

        private void HandleHoverUI(CombatSkillHoverEvent evt)
        {
            if (evt.Skill == null)
            {
                Hide();
                return;
            }

            if (skillIconImage != null)
            {
                skillIconImage.sprite = evt.Skill.skillUIImage;
                skillIconImage.gameObject.SetActive(true);
            }

            if (skillNameText != null) skillNameText.text = evt.Skill.skillName;
            if (skillDescText != null) skillDescText.text = evt.Skill.SkillDescription;
            if (skillCostText != null) skillCostText.text = evt.Skill.SkillCost.ToString();
            if (skillDamageText != null) skillDamageText.text = evt.Skill.SkillDamage.ToString();
            if (skillRangeText != null) skillRangeText.text = evt.Skill.SkillRange.ToString();

            if (evt.Pivot != null)
                SetPopupPosition(evt.Pivot, evt.Offset + popupOffset);
            
            Show();
        }

        private void SetPopupPosition(RectTransform pivot, Vector2 offset)
        {
            if (_parentRectTransform == null)
                _parentRectTransform = _rectTransform.parent as RectTransform;

            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>();

            Camera uiCamera = GetCanvasCamera();
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, pivot.position);

            if (_parentRectTransform != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRectTransform, screenPosition, uiCamera, out Vector2 localPosition))
            {
                _rectTransform.anchoredPosition = localPosition + offset;
                return;
            }

            _rectTransform.position = pivot.position;
            _rectTransform.anchoredPosition += offset;
        }

        private Camera GetCanvasCamera()
        {
            if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;

            return _canvas.worldCamera;
        }

        private void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

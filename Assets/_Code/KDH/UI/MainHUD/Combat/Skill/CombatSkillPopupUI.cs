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

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            
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
            if (skillCostText != null) skillCostText.text = evt.Skill.SkillValue.ToString();
            if (skillDamageText != null) skillDamageText.text = evt.Skill.SkillDamage.ToString();
            if (skillRangeText != null) skillRangeText.text = evt.Skill.SkillRange.ToString();

            if (evt.Pivot != null)
            {
                _rectTransform.position = evt.Pivot.position;
                _rectTransform.anchoredPosition += new Vector2(evt.Offset.x, evt.Offset.y);
            }
            
            Show();
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
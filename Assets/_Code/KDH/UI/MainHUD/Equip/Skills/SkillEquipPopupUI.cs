using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.SkillSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SkillEquipPopupUI : MonoBehaviour
    {
        [SerializeField] private Vector2 manualOffset;
        
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button unequipButton;
        
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _parentCanvas;
        private SkillSO _targetSkill;
        private bool _isCurrentlyEquipped;
        private int _frameCountOnOpen;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _parentCanvas = GetComponentInParent<Canvas>();

            Bus<SkillEquipPopupEvent>.Subscribe(HandlePopupEvent);
            
            equipButton.onClick.AddListener(HandleEquip);
            unequipButton.onClick.AddListener(HandleUnequip);

            Hide();
        }

        private void OnDestroy()
        {
            Bus<SkillEquipPopupEvent>.Unsubscribe(HandlePopupEvent);
            equipButton.onClick.RemoveListener(HandleEquip);
            unequipButton.onClick.RemoveListener(HandleUnequip);
        }

        private void Update()
        {
            if (Time.frameCount == _frameCountOnOpen) return;

            if (UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetMouseButtonDown(1))
            {
                Camera cam = null;
                if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    cam = _parentCanvas.worldCamera;
                }

                if (!RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, UnityEngine.Input.mousePosition, cam))
                {
                    Hide();
                }
            }
        }

        private void HandlePopupEvent(SkillEquipPopupEvent evt)
        {
            if (evt.Skill == null)
            {
                Hide();
                return;
            }

            _targetSkill = evt.Skill;
            _isCurrentlyEquipped = evt.IsEquipped;
            
            _canvasGroup.blocksRaycasts = !evt.IsReadOnly;

            if (descriptionText != null)
            {
                descriptionText.text = _isCurrentlyEquipped ? "스킬을\n해제하시겠습니까?" : "스킬을\n장착하시겠습니까?";
            }

            equipButton.gameObject.SetActive(!_isCurrentlyEquipped && !evt.IsReadOnly);
            unequipButton.gameObject.SetActive(_isCurrentlyEquipped && !evt.IsReadOnly);

            if (evt.Pivot != null)
            {
                _rectTransform.position = evt.Pivot.position;
                _rectTransform.localPosition += new Vector3(manualOffset.x, manualOffset.y, 0f);
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _frameCountOnOpen = Time.frameCount;
        }

        private void HandleEquip()
        {
            if (_targetSkill != null && !_isCurrentlyEquipped)
            {
                Bus<SkillEquipEvent>.Raise(new SkillEquipEvent(_targetSkill));
            }
            Hide();
        }

        private void HandleUnequip()
        {
            if (_targetSkill != null && _isCurrentlyEquipped)
            {
                Bus<SkillUnequipEvent>.Raise(new SkillUnequipEvent(_targetSkill));
            }
            Hide();
        }

        private void Hide()
        {
            gameObject.SetActive(false);
            _targetSkill = null;
        }
    }
}
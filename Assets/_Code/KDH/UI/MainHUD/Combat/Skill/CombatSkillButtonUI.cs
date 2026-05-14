using Code.Core.Events.Bus;
using Code.SkillSystem;
using DG.Tweening;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class CombatSkillButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPoolable
    {
        [SerializeField] private PoolingItemSO poolingType;
        [SerializeField] private Image skillIcon;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private GameObject hoverImage;
        [SerializeField] private GameObject blindImage;
        
        [Header("Animation Offset")]
        [SerializeField] private float hoverYOffset = 15f;
        [SerializeField] private float selectYOffset = 30f;
        [SerializeField] private float animDuration = 0.2f;
        [SerializeField] private Ease animEase = Ease.OutCubic;

        [SerializeField] private float darkenMultiplier = 0.4f;

        private RectTransform _rectTransform;
        private Image _backgroundImage;
        private Vector2 _originalPosition;
        private Tween _moveTween;
        private SkillSO _currentSkill;
        private SkillComponent _skillCompo;
        private bool _isSelected;
        private bool _isInteractable;

        private Color _originBgColor = Color.white;
        private Color _originIconColor = Color.white;
        private Color _originDamageColor = Color.white;
        private Color _originCostColor = Color.white;
        
        private Pool _pool;

        public PoolingItemSO PoolingType => poolingType;
        public GameObject GameObject => gameObject;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _backgroundImage = GetComponent<Image>();
            _originalPosition = _rectTransform.anchoredPosition;

            if (_backgroundImage != null) _originBgColor = _backgroundImage.color;
            if (skillIcon != null) _originIconColor = skillIcon.color;
            if (damageText != null) _originDamageColor = damageText.color;
            if (costText != null) _originCostColor = costText.color;
            
            if (hoverImage != null) hoverImage.SetActive(false);
            if (blindImage != null) blindImage.SetActive(false);
            
            Bus<CombatSkillCancelEvent>.Subscribe(HandleSkillCancel);
            Bus<CombatSkillSelectEvent>.Subscribe(HandleOtherSkillSelected);
        }

        private void OnDestroy()
        {
            Bus<CombatSkillCancelEvent>.Unsubscribe(HandleSkillCancel);
            Bus<CombatSkillSelectEvent>.Unsubscribe(HandleOtherSkillSelected);
            _moveTween?.Kill();
        }

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
        }

        public void ResetItem()
        {
            _currentSkill = null;
            _skillCompo = null;
            _isSelected = false;
            _isInteractable = false;
            ApplyColorMultiplier(1f);
            
            if (hoverImage != null) hoverImage.SetActive(false);
            if (blindImage != null) blindImage.SetActive(false);
            
            ResetPosition();
        }

        public void ReturnToPool()
        {
            if (_pool != null)
            {
                _pool.Push(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetupSkill(SkillSO skill, SkillComponent compo, int currentTurnCost)
        {
            _currentSkill = skill;
            _skillCompo = compo;
            _isSelected = false;
            
            if (hoverImage != null) hoverImage.SetActive(false);
            
            if (skillIcon != null) skillIcon.sprite = skill.skillUIImage;
            if (damageText != null) damageText.text = skill.SkillDamage.ToString();
            if (costText != null) costText.text = skill.SkillCost.ToString();
            
            UpdateInteractability(currentTurnCost);
            ResetPosition();
        }

        public void UpdateInteractability(int currentTurnCost)
        {
            if (_currentSkill == null) return;
            
            _isInteractable = currentTurnCost >= _currentSkill.SkillCost;

            if (blindImage != null) blindImage.SetActive(!_isInteractable);

            if (!_isInteractable)
            {
                transform.SetAsFirstSibling();
                ApplyColorMultiplier(darkenMultiplier);
            }
            else
            {
                ApplyColorMultiplier(1f);
            }
        }

        private void ApplyColorMultiplier(float multiplier)
        {
            Color tint = new Color(multiplier, multiplier, multiplier, 1f);

            if (_backgroundImage != null) _backgroundImage.color = _originBgColor * tint;
            if (skillIcon != null) skillIcon.color = _originIconColor * tint;
            if (damageText != null) damageText.color = _originDamageColor * tint;
            if (costText != null) costText.color = _originCostColor * tint;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isInteractable || _isSelected) return;
            if (hoverImage != null) hoverImage.SetActive(true);

            _moveTween?.Kill();
            _moveTween = _rectTransform.DOAnchorPosY(_originalPosition.y + hoverYOffset, animDuration).SetEase(animEase);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isInteractable || _isSelected) return;
            if (hoverImage != null) hoverImage.SetActive(false);
            ResetPosition();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                TrySelectSkill();
            }
        }

        public void TrySelectSkill()
        {
            if (!_isInteractable)
            {
                Bus<ShowMessageUIEvent>.Raise(new ShowMessageUIEvent("코스트가 부족하여 스킬을 사용할 수 없습니다."));
                return;
            }
            
            SelectThisSkill();
        }

        private void SelectThisSkill()
        {
            _isSelected = true;
            
            if (hoverImage != null) hoverImage.SetActive(true);
            
            _moveTween?.Kill();
            _moveTween = _rectTransform.DOAnchorPosY(_originalPosition.y + selectYOffset, animDuration).SetEase(animEase);
            
            if (_skillCompo != null && _currentSkill != null)
            {
                _skillCompo.CancelAllSkill();
                _skillCompo.StartSkill(_currentSkill);
            }
            
            Bus<CombatSkillSelectEvent>.Raise(new CombatSkillSelectEvent(_currentSkill));
        }

        private void HandleOtherSkillSelected(CombatSkillSelectEvent evt)
        {
            if (evt.SelectedSkill != _currentSkill && _isSelected)
            {
                _isSelected = false;
                if (hoverImage != null) hoverImage.SetActive(false);
                ResetPosition();
            }
        }

        private void HandleSkillCancel(CombatSkillCancelEvent evt)
        {
            if (_isSelected)
            {
                _isSelected = false;
                if (hoverImage != null) hoverImage.SetActive(false);
                ResetPosition();
                
                if (_skillCompo != null)
                {
                    _skillCompo.CancelAllSkill();
                }
            }
        }

        private void ResetPosition()
        {
            _moveTween?.Kill();
            _moveTween = _rectTransform.DOAnchorPosY(_originalPosition.y, animDuration).SetEase(animEase);
        }
    }
}
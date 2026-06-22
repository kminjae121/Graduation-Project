using Code.Core.Events.Bus;
using Code.SkillSystem;
using System.Collections.Generic;
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
        
        [Header("Popup Settings")]
        [SerializeField] private Vector2 popupOffset = new Vector2(0f, 300f);

        [Header("Animation Offset")]
        [SerializeField] private float hoverYOffset = 15f;
        [SerializeField] private float selectYOffset = 30f;
        [SerializeField] private float animDuration = 0.2f;
        [SerializeField] private Ease animEase = Ease.OutCubic;

        [SerializeField] private float darkenMultiplier = 0.4f;

        private RectTransform _rectTransform;
        private RectTransform _visualRoot;
        private Image _backgroundImage;
        private Vector2 _visualRootOriginPosition;
        private Tween _moveTween;
        private SkillSO _currentSkill;
        private SkillComponent _skillCompo;
        private Canvas _canvas;
        private bool _isSelected;
        private bool _isInteractable;
        private bool _isPointerInside;

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
            _canvas = GetComponentInParent<Canvas>();
            EnsureRaycastTarget();
            _visualRoot = CreateVisualRoot();
            _visualRootOriginPosition = _visualRoot.anchoredPosition;
            _backgroundImage = GetComponent<Image>();

            if (_backgroundImage != null) _originBgColor = _backgroundImage.color;
            if (skillIcon != null) _originIconColor = skillIcon.color;
            if (damageText != null) _originDamageColor = damageText.color;
            if (costText != null) _originCostColor = costText.color;
            
            if (hoverImage != null) hoverImage.SetActive(false);
            if (blindImage != null) blindImage.SetActive(false);
            
            Bus<CombatSkillCancelEvent>.Subscribe(HandleSkillCancel);
            Bus<CombatSkillSelectEvent>.Subscribe(HandleOtherSkillSelected);
        }

        private void Update()
        {
            if (!_isPointerInside) return;
            if (IsPointerInsideButton()) return;

            _isPointerInside = false;
            HidePopup();

            if (!_isSelected)
            {
                ClearHoverVisual();
            }
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
            _isPointerInside = false;
            ApplyColorMultiplier(1f);
            
            if (hoverImage != null) hoverImage.SetActive(false);
            if (blindImage != null) blindImage.SetActive(false);
            
            HidePopup();
            ResetPosition(true);
        }

        public void ReturnToPool()
        {
            HidePopup();

            if (_pool != null)
            {
                _pool.Push(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDisable()
        {
            _isPointerInside = false;
            _isSelected = false;
            if (hoverImage != null) hoverImage.SetActive(false);
            HidePopup();
            ResetPosition(true);
        }

        public void SetupSkill(SkillSO skill, SkillComponent compo, int currentTurnCost)
        {
            _currentSkill = skill;
            _skillCompo = compo;
            _isSelected = false;
            _isPointerInside = false;
            
            if (hoverImage != null) hoverImage.SetActive(false);
            
            if (skillIcon != null) skillIcon.sprite = skill.skillUIImage;
            if (damageText != null) damageText.text = skill.SkillDamage.ToString();
            if (costText != null) costText.text = skill.SkillCost.ToString();
            
            UpdateInteractability(currentTurnCost);
            ResetPosition(true);
        }

        public void UpdateInteractability(int currentTurnCost)
        {
            if (_currentSkill == null) return;
            
            _isInteractable = currentTurnCost >= _currentSkill.SkillCost;

            if (blindImage != null) blindImage.SetActive(!_isInteractable);

            if (!_isInteractable)
            {
                ApplyColorMultiplier(darkenMultiplier);
                if (!_isSelected)
                    ClearHoverVisual();
            }
            else
            {
                ApplyColorMultiplier(1f);
                if (_isPointerInside && !_isSelected)
                    ApplyHoverVisual();
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
            if (!IsPointerInsideButton(eventData)) return;

            _isPointerInside = true;
            ShowPopup();

            if (!_isInteractable || _isSelected) return;
            ApplyHoverVisual();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerInside = false;
            HidePopup();

            if (_isSelected) return;
            ClearHoverVisual();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && IsPointerInsideButton(eventData))
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
            ApplySelectedVisual();
            
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
                if (_isPointerInside && _isInteractable)
                    ApplyHoverVisual();
                else
                    ClearHoverVisual();
            }
        }

        private void HandleSkillCancel(CombatSkillCancelEvent evt)
        {
            if (_isSelected)
            {
                _isSelected = false;
                if (_isPointerInside && _isInteractable)
                    ApplyHoverVisual();
                else
                    ClearHoverVisual();
                
                if (_skillCompo != null)
                {
                    _skillCompo.CancelAllSkill();
                }
            }
        }

        private void ApplyHoverVisual()
        {
            if (hoverImage != null) hoverImage.SetActive(true);
            MoveToYOffset(hoverYOffset);
        }

        private void ApplySelectedVisual()
        {
            if (hoverImage != null) hoverImage.SetActive(true);
            MoveToYOffset(selectYOffset);
        }

        private void ClearHoverVisual()
        {
            if (hoverImage != null) hoverImage.SetActive(false);
            ResetPosition();
        }

        private void MoveToYOffset(float yOffset)
        {
            _moveTween?.Kill();
            _moveTween = _visualRoot.DOAnchorPosY(_visualRootOriginPosition.y + yOffset, animDuration).SetEase(animEase);
        }

        private void ResetPosition(bool instant = false)
        {
            _moveTween?.Kill();

            if (instant || !gameObject.activeInHierarchy)
            {
                _visualRoot.anchoredPosition = _visualRootOriginPosition;
                return;
            }

            _moveTween = _visualRoot.DOAnchorPosY(_visualRootOriginPosition.y, animDuration).SetEase(animEase);
        }

        private void ShowPopup()
        {
            if (_currentSkill == null) return;
            Bus<CombatSkillHoverEvent>.Raise(new CombatSkillHoverEvent(_currentSkill, _rectTransform, popupOffset));
        }

        private void HidePopup()
        {
            Bus<CombatSkillHoverEvent>.Raise(new CombatSkillHoverEvent(null, null));
        }

        private void EnsureRaycastTarget()
        {
            Graphic graphic = GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = true;
                return;
            }

            Image image = gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
        }

        private RectTransform CreateVisualRoot()
        {
            Transform existing = _rectTransform.Find("CombatSkillVisualRoot");
            if (existing is RectTransform existingRect)
            {
                return existingRect;
            }

            GameObject rootObject = new GameObject("CombatSkillVisualRoot", typeof(RectTransform));
            rootObject.layer = gameObject.layer;

            RectTransform root = rootObject.GetComponent<RectTransform>();
            root.SetParent(_rectTransform, false);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = _rectTransform.sizeDelta;
            root.anchoredPosition = Vector2.zero;
            root.localScale = Vector3.one;
            root.localRotation = Quaternion.identity;

            List<Transform> children = new List<Transform>();
            for (int i = 0; i < _rectTransform.childCount; i++)
            {
                Transform child = _rectTransform.GetChild(i);
                if (child != root)
                {
                    children.Add(child);
                }
            }

            foreach (Transform child in children)
            {
                child.SetParent(root, false);
            }

            return root;
        }

        private bool IsPointerInsideButton(PointerEventData eventData = null)
        {
            Vector2 screenPosition = eventData != null ? eventData.position : (Vector2)UnityEngine.Input.mousePosition;
            return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, screenPosition, GetEventCamera(eventData));
        }

        private Camera GetEventCamera(PointerEventData eventData)
        {
            if (eventData != null && eventData.enterEventCamera != null)
            {
                return eventData.enterEventCamera;
            }

            if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return _canvas.worldCamera;
        }
    }
}

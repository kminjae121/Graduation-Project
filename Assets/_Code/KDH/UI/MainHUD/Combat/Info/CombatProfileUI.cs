using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class CombatProfileUI : MonoBehaviour
    {
        [Header("UI Panel & Animation")]
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private Vector2 hiddenPosition;
        [SerializeField] private Vector2 visiblePosition;
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private Ease slideEase = Ease.OutBack;

        [Header("Profile Elements")]
        [SerializeField] private Image profileIconImage;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image hpFillImage;

        private Tween _slideTween;
        private bool _isCurrentlyVisible = false;

        private bool _isSkillPlaying = false;
        private bool _isMovePlaying = false;
        private bool _isAtkUIHidden = false;
        private bool _isTurnEnded = true;
        private bool _isSkillReceived = false;
        
        private CharacterUnit _currentAllyUnit;
        private UnitHealth _currentHealthCompo;

        private void Awake()
        {
            if (panelRect == null) panelRect = GetComponent<RectTransform>();
            panelRect.anchoredPosition = hiddenPosition;

            Bus<SkillUIEvent>.Subscribe(HandleSkillUI);
            Bus<SetAtkUIEvent>.Subscribe(HandleAtkUI);
            Bus<UnitSkilStartEvent>.Subscribe(HandleSkillStart);
            Bus<UnitMoveControlEvent>.Subscribe(HandleMoveControl);
            Bus<UnitTurnEndEvent>.Subscribe(HandleUnitTurnEnd);
            Bus<CombatSkillCancelEvent>.Subscribe(HandleSkillCancel);
        }

        private void OnDestroy()
        {
            Bus<SkillUIEvent>.Unsubscribe(HandleSkillUI);
            Bus<SetAtkUIEvent>.Unsubscribe(HandleAtkUI);
            Bus<UnitSkilStartEvent>.Unsubscribe(HandleSkillStart);
            Bus<UnitMoveControlEvent>.Unsubscribe(HandleMoveControl);
            Bus<UnitTurnEndEvent>.Unsubscribe(HandleUnitTurnEnd);
            Bus<CombatSkillCancelEvent>.Unsubscribe(HandleSkillCancel);
            
            UnsubscribeHealth();
            _slideTween?.Kill();
        }

        private void EvaluateVisibility()
        {
            bool canShow = !_isSkillPlaying && !_isMovePlaying && !_isAtkUIHidden && !_isTurnEnded && _isSkillReceived;
            
            if (canShow) ShowUI();
            else HideUI();
        }

        private void HandleSkillUI(SkillUIEvent evt)
        {
            UnsubscribeHealth();
            _isTurnEnded = false;
            _isSkillReceived = evt.SkillCompo != null;

            HideUI();

            if (evt.SkillCompo != null)
            {
                _currentAllyUnit = evt.SkillCompo.GetComponentInParent<CharacterUnit>();
                if (_currentAllyUnit != null)
                {
                    if (profileIconImage != null && _currentAllyUnit.unitSO != null)
                        profileIconImage.sprite = _currentAllyUnit.unitSO.UnitImage;

                    if (unitNameText != null && _currentAllyUnit.unitSO != null)
                        unitNameText.text = _currentAllyUnit.unitSO.UnitName;

                    _currentHealthCompo = _currentAllyUnit.HealthCompo;
                    
                    if (_currentHealthCompo != null)
                    {
                        _currentHealthCompo.OnHealthChangedEvent += UpdateHealthUI;
                        UpdateHealthUI(_currentHealthCompo.CurrentHealth, _currentHealthCompo.MaxHealth);
                    }
                }
            }

            DOVirtual.DelayedCall(0.5f, () => 
            {
                if (this == null) return;
                EvaluateVisibility();
            });
        }

        private void UnsubscribeHealth()
        {
            if (_currentHealthCompo != null)
            {
                _currentHealthCompo.OnHealthChangedEvent -= UpdateHealthUI;
                _currentHealthCompo = null;
            }
        }

        private void UpdateHealthUI(float currentHp, float maxHp)
        {
            if (hpText != null) hpText.text = $"{Mathf.CeilToInt(currentHp)}";
            if (hpFillImage != null) hpFillImage.fillAmount = maxHp > 0 ? (currentHp / maxHp) : 0f;
        }

        private void HandleAtkUI(SetAtkUIEvent evt) { _isAtkUIHidden = !evt.IsActive; EvaluateVisibility(); }
        private void HandleSkillStart(UnitSkilStartEvent evt) { _isSkillPlaying = evt.isStart; if (!evt.isStart) _isAtkUIHidden = false; EvaluateVisibility(); }
        private void HandleMoveControl(UnitMoveControlEvent evt) { _isMovePlaying = !evt.isMoving; if (evt.isMoving) _isAtkUIHidden = false; EvaluateVisibility(); }
        private void HandleUnitTurnEnd(UnitTurnEndEvent evt) { _isTurnEnded = true; _isSkillPlaying = false; _isMovePlaying = false; _isAtkUIHidden = false; EvaluateVisibility(); }
        private void HandleSkillCancel(CombatSkillCancelEvent evt) { if (!_isSkillPlaying && !_isMovePlaying) { _isAtkUIHidden = false; EvaluateVisibility(); } }

        private void ShowUI()
        {
            if (_isCurrentlyVisible) return;
            _isCurrentlyVisible = true;
            _slideTween?.Kill();
            _slideTween = panelRect.DOAnchorPos(visiblePosition, slideDuration).SetEase(slideEase);
        }

        private void HideUI()
        {
            if (!_isCurrentlyVisible) return;
            _isCurrentlyVisible = false;
            _slideTween?.Kill();
            _slideTween = panelRect.DOAnchorPos(hiddenPosition, slideDuration).SetEase(Ease.InBack);
        }
    }
}
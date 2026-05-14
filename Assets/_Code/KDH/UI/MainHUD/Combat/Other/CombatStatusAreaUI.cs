using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;

namespace Code.UI
{
    public class CombatStatusAreaUI : MonoBehaviour
    {
        [Header("Status Areas")]
        [SerializeField] private RectTransform buffArea;
        [SerializeField] private RectTransform debuffArea;

        [Header("Animation Settings")]
        [SerializeField] private Vector2 buffVisiblePosition;
        [SerializeField] private Vector2 buffHiddenPosition;
        [SerializeField] private Vector2 debuffVisiblePosition;
        [SerializeField] private Vector2 debuffHiddenPosition;
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private Ease slideEase = Ease.OutBack;

        private Tween _buffTween;
        private Tween _debuffTween;
        private bool _isCurrentlyVisible = false;

        private bool _isSkillPlaying = false;
        private bool _isMovePlaying = false;
        private bool _isAtkUIHidden = false;
        private bool _isTurnEnded = true;
        private bool _isSkillReceived = false;

        private void Awake()
        {
            if (buffArea != null) buffArea.anchoredPosition = buffHiddenPosition;
            if (debuffArea != null) debuffArea.anchoredPosition = debuffHiddenPosition;

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
            
            _buffTween?.Kill();
            _debuffTween?.Kill();
        }

        private void EvaluateVisibility()
        {
            bool canShow = !_isSkillPlaying && !_isMovePlaying && !_isAtkUIHidden && !_isTurnEnded && _isSkillReceived;
            
            if (canShow) ShowUI();
            else HideUI();
        }

        private void HandleSkillUI(SkillUIEvent evt)
        {
            _isTurnEnded = false;
            _isSkillReceived = evt.SkillCompo != null;

            HideUI();

            DOVirtual.DelayedCall(0.5f, () => 
            {
                if (this == null) return;
                EvaluateVisibility();
            });
        }

        private void HandleAtkUI(SetAtkUIEvent evt)
        {
            _isAtkUIHidden = !evt.IsActive;
            EvaluateVisibility();
        }

        private void HandleSkillStart(UnitSkilStartEvent evt)
        {
            _isSkillPlaying = evt.isStart;
            if (!evt.isStart) _isAtkUIHidden = false;
            EvaluateVisibility();
        }

        private void HandleMoveControl(UnitMoveControlEvent evt)
        {
            _isMovePlaying = !evt.isMoving; 
            if (evt.isMoving) _isAtkUIHidden = false;
            EvaluateVisibility();
        }

        private void HandleUnitTurnEnd(UnitTurnEndEvent evt)
        {
            _isTurnEnded = true;
            _isSkillPlaying = false;
            _isMovePlaying = false;
            _isAtkUIHidden = false;
            EvaluateVisibility();
        }

        private void HandleSkillCancel(CombatSkillCancelEvent evt)
        {
            if (!_isSkillPlaying && !_isMovePlaying) 
            {
                _isAtkUIHidden = false;
                EvaluateVisibility();
            }
        }

        private void ShowUI()
        {
            if (_isCurrentlyVisible) return;
            
            _isCurrentlyVisible = true;
            
            if (buffArea != null)
            {
                _buffTween?.Kill();
                _buffTween = buffArea.DOAnchorPos(buffVisiblePosition, slideDuration).SetEase(slideEase);
            }

            if (debuffArea != null)
            {
                _debuffTween?.Kill();
                _debuffTween = debuffArea.DOAnchorPos(debuffVisiblePosition, slideDuration).SetEase(slideEase);
            }
        }

        private void HideUI()
        {
            if (!_isCurrentlyVisible) return;
            
            _isCurrentlyVisible = false;
            
            if (buffArea != null)
            {
                _buffTween?.Kill();
                _buffTween = buffArea.DOAnchorPos(buffHiddenPosition, slideDuration).SetEase(Ease.InBack);
            }

            if (debuffArea != null)
            {
                _debuffTween?.Kill();
                _debuffTween = debuffArea.DOAnchorPos(debuffHiddenPosition, slideDuration).SetEase(Ease.InBack);
            }
        }
    }
}
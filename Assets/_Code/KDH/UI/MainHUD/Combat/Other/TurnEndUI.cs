using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Code.UI
{
    public class TurnEndUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private Button turnEndButton;

        [Header("Animation Settings")]
        [SerializeField] private Vector2 visiblePosition;
        [SerializeField] private Vector2 hiddenPosition;
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private Ease slideEase = Ease.OutBack;

        private Tween _slideTween;
        private bool _isCurrentlyVisible = false;

        private bool _isSkillPlaying = false;
        private bool _isMovePlaying = false;
        private bool _isAtkUIHidden = false;
        private bool _isTurnEnded = true;
        private bool _isSkillReceived = false;

        private Vector3 _originalButtonScale;

        private void Awake()
        {
            if (panelRect == null)
            {
                panelRect = GetComponent<RectTransform>();
            }
            
            panelRect.anchoredPosition = hiddenPosition;

            if (turnEndButton != null)
            {
                _originalButtonScale = turnEndButton.transform.localScale;
                turnEndButton.onClick.AddListener(OnTurnEndButtonClicked);

                EventTrigger trigger = turnEndButton.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = turnEndButton.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                enterEntry.callback.AddListener((data) => { turnEndButton.transform.DOScale(_originalButtonScale * 1.05f, 0.2f).SetEase(Ease.OutQuad); });
                trigger.triggers.Add(enterEntry);

                EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                exitEntry.callback.AddListener((data) => { turnEndButton.transform.DOScale(_originalButtonScale, 0.2f).SetEase(Ease.OutQuad); });
                trigger.triggers.Add(exitEntry);
            }

            Bus<SkillUIEvent>.Subscribe(HandleSkillUI);
            Bus<SetAtkUIEvent>.Subscribe(HandleAtkUI);
            Bus<UnitSkilStartEvent>.Subscribe(HandleSkillStart);
            Bus<UnitMoveControlEvent>.Subscribe(HandleMoveControl);
            Bus<UnitTurnEndEvent>.Subscribe(HandleUnitTurnEnd);
            Bus<CombatSkillCancelEvent>.Subscribe(HandleSkillCancel);
        }

        private void OnDestroy()
        {
            if (turnEndButton != null)
            {
                turnEndButton.onClick.RemoveListener(OnTurnEndButtonClicked);
            }

            Bus<SkillUIEvent>.Unsubscribe(HandleSkillUI);
            Bus<SetAtkUIEvent>.Unsubscribe(HandleAtkUI);
            Bus<UnitSkilStartEvent>.Unsubscribe(HandleSkillStart);
            Bus<UnitMoveControlEvent>.Unsubscribe(HandleMoveControl);
            Bus<UnitTurnEndEvent>.Unsubscribe(HandleUnitTurnEnd);
            Bus<CombatSkillCancelEvent>.Unsubscribe(HandleSkillCancel);
            
            _slideTween?.Kill();
        }

        private void OnTurnEndButtonClicked()
        {
            turnEndButton.transform.DOScale(_originalButtonScale, 0.1f);
            Bus<UnitTurnEndEvent>.Raise(new UnitTurnEndEvent());
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
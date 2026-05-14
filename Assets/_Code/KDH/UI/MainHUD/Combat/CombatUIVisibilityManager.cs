using Code.Core.Events.Bus;
using DG.Tweening;
using UnityEngine;

namespace Code.Managers
{
    public class CombatUIVisibilityManager : MonoBehaviour
    {
        private bool _isActionPlaying = false;
        private Tween _delayTween;

        private void Awake()
        {
            Bus<SkillUIEvent>.Subscribe(HandleTurnStart);
            Bus<UnitTurnEndEvent>.Subscribe(HandleTurnEnd);
            Bus<SetAtkUIEvent>.Subscribe(HandleActionState);
            Bus<CombatSkillCancelEvent>.Subscribe(HandleSkillCancel);
        }

        private void OnDestroy()
        {
            Bus<SkillUIEvent>.Unsubscribe(HandleTurnStart);
            Bus<UnitTurnEndEvent>.Unsubscribe(HandleTurnEnd);
            Bus<SetAtkUIEvent>.Unsubscribe(HandleActionState);
            Bus<CombatSkillCancelEvent>.Unsubscribe(HandleSkillCancel);
            
            _delayTween?.Kill();
        }

        private void HandleTurnStart(SkillUIEvent evt)
        {
            if (evt.SkillCompo != null)
            {
                _delayTween?.Kill();
                _delayTween = DOVirtual.DelayedCall(0.4f, () => 
                {
                    if (this == null) return;
                    if (!_isActionPlaying)
                    {
                        Bus<CombatUIVisibilityEvent>.Raise(new CombatUIVisibilityEvent(true));
                    }
                });
            }
        }

        private void HandleTurnEnd(UnitTurnEndEvent evt)
        {
            _isActionPlaying = false;
            _delayTween?.Kill();
            Bus<CombatUIVisibilityEvent>.Raise(new CombatUIVisibilityEvent(false));
        }

        private void HandleActionState(SetAtkUIEvent evt)
        {
            _isActionPlaying = !evt.IsActive;
            _delayTween?.Kill();
            Bus<CombatUIVisibilityEvent>.Raise(new CombatUIVisibilityEvent(evt.IsActive));
        }

        private void HandleSkillCancel(CombatSkillCancelEvent evt)
        {
            if (!_isActionPlaying)
            {
                _delayTween?.Kill();
                Bus<CombatUIVisibilityEvent>.Raise(new CombatUIVisibilityEvent(true));
            }
        }
    }
}
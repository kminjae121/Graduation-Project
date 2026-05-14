using Code.Core.Events.Bus;
using Code.Core.Interfaces;
using UnityEngine;

namespace Code.UnitSystem
{
    [RequireComponent(typeof(ITurnable))]
    public class UnitHoverHighlight : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [SerializeField] private GameObject highlightEffectObj;

        private ITurnable _myTurnable;
        private GameObject _targetUnit;

        private void Awake()
        {
            _myTurnable = GetComponent<ITurnable>();
            
            if (highlightEffectObj != null)
            {
                highlightEffectObj.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[UnitHoverHighlight] 강조 효과용 오브젝트가 할당되지 않았습니다.");
            }
            
            Bus<CombatUnitHoverEvent>.Subscribe(HandleHoverEvent);
        }

        private void OnDestroy()
        {
            Bus<CombatUnitHoverEvent>.Unsubscribe(HandleHoverEvent);
        }

        private void HandleHoverEvent(CombatUnitHoverEvent evt)
        {
            if (_myTurnable == null || evt.HoveredUnit == null) return;
            
            if (evt.IsHoverEnter)
            {
                if(_targetUnit != null && _targetUnit != evt.HoveredUnit.UnitObj)
                    _targetUnit.GetComponentInChildren<UnitOutLineCompo>()?.ResetOutLine();
                
                _targetUnit = evt.HoveredUnit.UnitObj;
                _targetUnit.GetComponentInChildren<UnitOutLineCompo>()?.SetOutSelectOutLine();
                
                if (_myTurnable.Equals(evt.HoveredUnit))
                {
                    if (highlightEffectObj != null) highlightEffectObj.SetActive(true);
                }
            }
            else
            {
                if (_targetUnit != null)
                {
                    _targetUnit.GetComponentInChildren<UnitOutLineCompo>()?.ResetOutLine();
                    _targetUnit = null; 
                }

                if (_myTurnable.Equals(evt.HoveredUnit))
                {
                    if (highlightEffectObj != null) highlightEffectObj.SetActive(false);
                }
            }
        }
    }
}
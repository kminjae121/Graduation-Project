using Code.Core.Debugs;
using Code.Core.Events.Bus;
using Input;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class UnitTrait : MonoBehaviour, IUnitComponent
    {
        [field: SerializeField] public bool IsNeedTarget { get; private set; } = false;
        [SerializeField] private InputReader inputReader;
        
        private Unit _unit;
        private IUnitPerform _perform;
        private IUnitCondition _condition;
        private bool _isPerformed = false;
        private GameObject _targetEnemy = null;
        private UnitType _unitType = UnitType.None;
        
        public bool IsTargeting { get; private set; } = false;
        
        public void Initialize(Unit owner)
        {
            _unit = owner;

            _unitType = _unit.unitSO.UnitType;
            _condition = GetComponentInChildren<IUnitCondition>();
            _perform = GetComponentInChildren<IUnitPerform>();
            
            Bus<UseSpecEvent>.Subscribe(CheckCondition);

            if (inputReader != null)
                inputReader.OnAttackEvent += HandleTraitEnemy;
            else
                UnityLogger.LogWarning("InputReader가 존재하지 않습니다.");
            
            if (_unit != null)
            {
                if (_condition == null)
                {
                    Debug.LogWarning($"{_unitType} : 컨디션 컴포넌트가 존재하지 않습니다.");
                    return;
                }
                else
                {
                    _condition.Initialize(_unit);
                }

                if (_perform == null)
                {
                    Debug.LogWarning($"{_unitType} : 실행컴포넌트가 존재하지 않습니다.");
                    return;
                }
                else
                    _perform.Initialize(_unit);
            }
            else
                Debug.LogWarning("유닛이 할당되어있지 않습니다.");
        }


        private void OnDestroy()
        {
            Bus<UseSpecEvent>.Unsubscribe(CheckCondition);
            if(inputReader != null)
                inputReader.OnAttackEvent -= HandleTraitEnemy;
        }

        public void CheckCondition(UseSpecEvent evt)
        {
            if (evt.unitType != _unitType)
                return;
            
            if (_condition.CheckCondition(evt.target))
            {
                _isPerformed = true;
                return;
            }
        }

        public void Perform(GameObject target)
        {
            if (_isPerformed)
            {
                _perform.Perform(target);
                _isPerformed = false;
            }
        }

        public void Perform()
        {
            if (_isPerformed && !IsNeedTarget)
            {
                _perform.Perform(null);
                _isPerformed = false;
            }
        }

        public void SetEnemy(GameObject target)
        {
            _targetEnemy = target;
        }

        public void SetTargeting()
        {
            if (_condition.CheckCondition(null))
            {
                IsTargeting = true;
            }
        }

        public void OffTargeting()
        {
            IsTargeting = false;
        }
        private void HandleTraitEnemy()
        {
            if (IsNeedTarget)
            {
                if (_targetEnemy != null && IsTargeting)
                {
                    Perform(_targetEnemy);
                    OffTargeting();
                }
                else
                {
                    if (IsTargeting == true)
                    {
                        OffTargeting();
                    }
                }   
            }
        }
    }
}

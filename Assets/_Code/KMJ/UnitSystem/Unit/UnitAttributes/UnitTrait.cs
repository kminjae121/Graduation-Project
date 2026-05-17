using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core.Events.Bus;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public class UnitTrait : MonoBehaviour, IUnitComponent
    {
        private Unit _unit;
        private IUnitPerform _perform;
        private IUnitCondition _condition;

        private bool _isPerformed = false;
        public bool IsHasEnemy = false;
        
        private UnitType _unitType = UnitType.None;
        
        public void Initialize(Unit owner)
        {
            _unit = owner;

            _condition = GetComponentInChildren<IUnitCondition>();
            _perform = GetComponentInChildren<IUnitPerform>();

            if (_unit != null)
            {
                if (_condition != null)
                {
                    Debug.LogWarning("컨디션 컴포넌트가 존재하지 않습니다.");
                    return;
                }
                else
                {
                    _condition.Initialize(_unit);
                }

                if (_perform == null)
                {
                    Debug.LogWarning("실행컴포넌트가 존재하지 않습니다.");
                    return;
                }
                else
                 _perform.Initialize(_unit);
            }
            else
                Debug.LogWarning("유닛이 할당되어있지 않습니다.");

            _unitType = _unit.unitSO.UnitType;
            
            Bus<UseGimicEvent>.Subscribe(CheckCondition);
        }

        private void OnDestroy()
        {
            Bus<UseGimicEvent>.Unsubscribe(CheckCondition);
        }

        public void CheckCondition(UseGimicEvent evt)
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
            if (_isPerformed && IsHasEnemy)
            {
                _perform.Perform(null);
                _isPerformed = false;
            }
        }
    }
}
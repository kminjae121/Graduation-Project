using System;
using Code.Core;
using Code.Core.Events.Bus;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UnitSystem
{
    public class UnitSkillCost : MonoBehaviour, IUnitComponent
    {
        private Unit _owenr;

        private int _currentCost;
        
        private int AddGaugeValue => _owenr.unitSO.RecoveryManaCost;
        private int MaxGaugeValue => _owenr.unitSO.MaxManaCost;
        
        public UnityEvent<int> skillCostChanged;
        
        public void Initialize(Unit owner)
        {
            _owenr = owner;
            _currentCost = MaxGaugeValue;
        }

        public int GetUnitSkillCost()
            =>  _currentCost;

        public int GetMaxSkillCost()
            => MaxGaugeValue;

        public void UseSkillCost(int useCost)
        {
            if (_currentCost - useCost >= 0)
            {
                _currentCost -= useCost;
                skillCostChanged?.Invoke(_currentCost);
            }
            else
                return ;
        }

        public bool CanUseSkillCost(int useCost)
        {
            if (_currentCost - useCost >= 0)
            {
                return true;
            }
            else
                return false;
        }

        public int CheckSkillCost(int cost)
            => _currentCost - cost;

        public void AddSkillCost(int skillCost = 0)
        {
            if (skillCost == 0)
                _currentCost += AddGaugeValue;
            else
                _currentCost += skillCost;

            if (_currentCost >= MaxGaugeValue)
                _currentCost = MaxGaugeValue;
            
            skillCostChanged?.Invoke(_currentCost);     
        }

    }
}
using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class TurnCostUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform costIconGroup;
        [SerializeField] private TextMeshProUGUI costAmountText;
        [SerializeField] private PoolingItemSO costIconPoolingSO;
        
        private PoolManagerMono _poolManager;
        private List<CostIconUI> _activeIcons = new List<CostIconUI>();
        private CharacterUnit _currentUnit;
        private int _currentPreviewCost = 0;

        private void Awake()
        {
            _poolManager = UnityEngine.Object.FindFirstObjectByType<PoolManagerMono>();

            if (_poolManager == null)
            {
                Debug.LogError("[TurnCostUI] 풀 매니저를 씬에서 찾을 수 없습니다.");
            }
            
            Bus<SkillUIEvent>.Subscribe(HandleSkillReceived);
            Bus<CombatSkillSelectEvent>.Subscribe(HandleSkillSelected);
            Bus<CombatSkillCancelEvent>.Subscribe(HandleSkillCanceled);
            Bus<UnitTurnEndEvent>.Subscribe(HandleTurnEnd);
        }

        private void OnDestroy()
        {
            Bus<SkillUIEvent>.Unsubscribe(HandleSkillReceived);
            Bus<CombatSkillSelectEvent>.Unsubscribe(HandleSkillSelected);
            Bus<CombatSkillCancelEvent>.Unsubscribe(HandleSkillCanceled);
            Bus<UnitTurnEndEvent>.Unsubscribe(HandleTurnEnd);
            
            UnsubscribeCurrentUnit();
        }

        private void HandleSkillReceived(SkillUIEvent evt)
        {
            UnsubscribeCurrentUnit();
            
            if (evt.SkillCompo != null)
            {
                _currentUnit = evt.SkillCompo.GetComponentInParent<CharacterUnit>();
                
                if (_currentUnit != null && _currentUnit.SkillCostCompo != null)
                {
                    _currentUnit.SkillCostCompo.skillCostChanged.AddListener(HandleCostChanged);
                    InitializeIcons(_currentUnit.SkillCostCompo.GetMaxSkillCost());
                    return;
                }
            }
            
            ClearIcons();
        }

        private void HandleTurnEnd(UnitTurnEndEvent evt)
        {
            if (_currentUnit != null && evt.Unit == _currentUnit)
            {
                ClearIcons();
                UnsubscribeCurrentUnit();
                _currentUnit = null;
            }
        }

        private void UnsubscribeCurrentUnit()
        {
            if (_currentUnit != null && _currentUnit.SkillCostCompo != null)
            {
                _currentUnit.SkillCostCompo.skillCostChanged.RemoveListener(HandleCostChanged);
            }
        }

        private void ClearIcons()
        {
            foreach (var icon in _activeIcons)
            {
                if (icon != null) icon.ReturnToPool();
            }
            _activeIcons.Clear();
            
            if (costAmountText != null) costAmountText.text = string.Empty;
        }

        private void InitializeIcons(int maxCost)
        {
            ClearIcons();

            if (_poolManager == null || costIconPoolingSO == null) return;

            for (int i = 0; i < maxCost; i++)
            {
                var icon = _poolManager.Pop<CostIconUI>(costIconPoolingSO);
                if (icon != null)
                {
                    icon.transform.SetParent(costIconGroup);
                    icon.transform.localScale = Vector3.one;
                    icon.transform.SetAsLastSibling(); 
                    
                    _activeIcons.Add(icon);
                }
            }
            
            RefreshIcons(_currentUnit.SkillCostCompo.GetUnitSkillCost());
        }

        private void HandleCostChanged(int nextCost)
        {
            _currentPreviewCost = 0; 
            
            if (_currentUnit != null && _activeIcons.Count != _currentUnit.SkillCostCompo.GetMaxSkillCost())
            {
                InitializeIcons(_currentUnit.SkillCostCompo.GetMaxSkillCost());
            }
            else
            {
                RefreshIcons(nextCost);
            }
        }

        private void RefreshIcons(int currentCost)
        {
            if (_currentUnit != null && costAmountText != null)
            {
                int maxCost = _currentUnit.SkillCostCompo.GetMaxSkillCost();
                costAmountText.text = $"{currentCost}/{maxCost}";
            }

            for (int i = 0; i < _activeIcons.Count; i++)
            {
                if (_activeIcons[i] == null) continue;

                bool isActive = i < currentCost;
                bool isPreview = isActive && i >= currentCost - _currentPreviewCost;

                if (isPreview)
                {
                    _activeIcons[i].SetPreviewState();
                }
                else
                {
                    _activeIcons[i].SetActiveState(isActive);
                }
            }
        }

        private void HandleSkillSelected(CombatSkillSelectEvent evt)
        {
            if (evt.SelectedSkill != null)
            {
                _currentPreviewCost = evt.SelectedSkill.SkillCost;
                if (_currentUnit != null && _currentUnit.SkillCostCompo != null)
                {
                    RefreshIcons(_currentUnit.SkillCostCompo.GetUnitSkillCost());
                }
            }
        }

        private void HandleSkillCanceled(CombatSkillCancelEvent evt)
        {
            _currentPreviewCost = 0;
            if (_currentUnit != null && _currentUnit.SkillCostCompo != null)
            {
                RefreshIcons(_currentUnit.SkillCostCompo.GetUnitSkillCost());
            }
        }
    }
}
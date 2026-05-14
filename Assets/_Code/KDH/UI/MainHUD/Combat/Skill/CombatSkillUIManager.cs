using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.SkillSystem;
using DG.Tweening;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class CombatSkillUIManager : MonoBehaviour
    {
        [Header("UI Area & Pooling")]
        [SerializeField] private RectTransform skillArea;
        [SerializeField] private PoolingItemSO skillButtonPoolingSO;

        [Header("Slots & Buttons")]
        [SerializeField] private List<RectTransform> skillSlotPositions;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button prevPageButton;

        [Header("Slide Animation")]
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private Ease slideEase = Ease.OutBack;
        [SerializeField] private Vector2 hiddenPosition;
        [SerializeField] private Vector2 visiblePosition;

        private Tween _slideTween;
        private List<SkillSO> _equippedSkills = new List<SkillSO>();
        private SkillComponent _currentSkillCompo;
        private CharacterUnit _currentUnit;
        private int _currentPage = 0;
        private const int MaxSkillsPerPage = 3;
        
        private bool _isSkillSelected = false;
        private bool _isCurrentlyVisible = false;

        private bool _isSkillPlaying = false;
        private bool _isMovePlaying = false;
        private bool _isAtkUIHidden = false;
        private bool _isTurnEnded = true;
        private bool _isSkillReceived = false;
        
        private PoolManagerMono _poolManager;
        private List<CombatSkillButtonUI> _activeSkillButtons = new List<CombatSkillButtonUI>();

        private void Awake()
        {
            _poolManager = UnityEngine.Object.FindFirstObjectByType<PoolManagerMono>();

            if (skillArea != null)
            {
                skillArea.anchoredPosition = hiddenPosition;
                skillArea.gameObject.SetActive(true);
            }

            if (nextPageButton != null) nextPageButton.onClick.AddListener(GoToNextPage);
            if (prevPageButton != null) prevPageButton.onClick.AddListener(GoToPrevPage);

            Bus<SkillUIEvent>.Subscribe(HandleSkillReceived);
            Bus<CombatSkillSelectEvent>.Subscribe(HandleSkillSelected);
            Bus<UnitTurnEndEvent>.Subscribe(HandleTurnEnd);
            Bus<SetAtkUIEvent>.Subscribe(HandleAtkUI);
            Bus<UnitSkilStartEvent>.Subscribe(HandleSkillStart);
            Bus<UnitMoveControlEvent>.Subscribe(HandleMoveControl);
            Bus<CombatSkillCancelEvent>.Subscribe(HandleSkillCancel);
        }

        private void OnDestroy()
        {
            if (nextPageButton != null) nextPageButton.onClick.RemoveListener(GoToNextPage);
            if (prevPageButton != null) prevPageButton.onClick.RemoveListener(GoToPrevPage);

            Bus<SkillUIEvent>.Unsubscribe(HandleSkillReceived);
            Bus<CombatSkillSelectEvent>.Unsubscribe(HandleSkillSelected);
            Bus<UnitTurnEndEvent>.Unsubscribe(HandleTurnEnd);
            Bus<SetAtkUIEvent>.Unsubscribe(HandleAtkUI);
            Bus<UnitSkilStartEvent>.Unsubscribe(HandleSkillStart);
            Bus<UnitMoveControlEvent>.Unsubscribe(HandleMoveControl);
            Bus<CombatSkillCancelEvent>.Unsubscribe(HandleSkillCancel);

            UnsubscribeCurrentUnit();
            _slideTween?.Kill();
        }

        private void Update()
        {
            if (_isSkillSelected && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                CancelSkillSelection();
            }

            if (_isCurrentlyVisible && !_isSkillPlaying && !_isMovePlaying && !_isAtkUIHidden)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1)) SelectSkillByIndex(0);
                if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2)) SelectSkillByIndex(1);
                if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3)) SelectSkillByIndex(2);
                if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad4)) SelectSkillByIndex(3);
            }
        }

        private void SelectSkillByIndex(int index)
        {
            if (index >= 0 && index < _activeSkillButtons.Count)
            {
                var btn = _activeSkillButtons[index];
                if (btn != null) btn.TrySelectSkill();
            }
        }

        private void EvaluateVisibility()
        {
            bool canShow = !_isSkillPlaying && !_isMovePlaying && !_isAtkUIHidden && !_isTurnEnded && _isSkillReceived;
            
            if (canShow) SafeShowUI();
            else HideUI(false);
        }

        private void HandleSkillReceived(SkillUIEvent evt)
        {
            UnsubscribeCurrentUnit();

            _isTurnEnded = false;
            _isSkillSelected = false;

            _currentSkillCompo = evt.SkillCompo;
            _currentUnit = _currentSkillCompo != null ? _currentSkillCompo.GetComponentInParent<CharacterUnit>() : null;

            List<SkillSO> validSkills = evt.Skills;
            if ((validSkills == null || validSkills.Count == 0) && _currentUnit != null && _currentUnit.unitSO != null)
            {
                if (_currentUnit.unitSO.SkillStorage != null && _currentUnit.unitSO.SkillStorage.skills.Count > 0)
                {
                    validSkills = _currentUnit.unitSO.SkillStorage.skills;
                    Debug.LogWarning("[CombatSkillUIManager] SkillSendManager가 비어있어 UnitSO에서 직접 스킬을 로드합니다.");
                }
            }

            if (_currentSkillCompo == null || validSkills == null || validSkills.Count == 0)
            {
                _isSkillReceived = false;
                _equippedSkills = null;
                _currentSkillCompo = null;
                _currentUnit = null;
                RefreshSkillSlots();
                EvaluateVisibility();
                return;
            }

            _isSkillReceived = true;
            _equippedSkills = validSkills;
            
            if (_currentUnit != null && _currentUnit.SkillCostCompo != null)
            {
                _currentUnit.SkillCostCompo.skillCostChanged.AddListener(HandleCostChanged);
            }

            _currentPage = 0;

            if (_equippedSkills != null && _equippedSkills.Count > MaxSkillsPerPage)
            {
                if (nextPageButton != null) nextPageButton.gameObject.SetActive(true);
                if (prevPageButton != null) prevPageButton.gameObject.SetActive(false);
            }
            else
            {
                if (nextPageButton != null) nextPageButton.gameObject.SetActive(false);
                if (prevPageButton != null) prevPageButton.gameObject.SetActive(false);
            }

            RefreshSkillSlots();
            EvaluateVisibility();
        }

        private void UnsubscribeCurrentUnit()
        {
            if (_currentUnit != null && _currentUnit.SkillCostCompo != null)
            {
                _currentUnit.SkillCostCompo.skillCostChanged.RemoveListener(HandleCostChanged);
            }
        }

        private void HandleCostChanged(int newCost)
        {
            foreach (var btn in _activeSkillButtons)
            {
                if (btn != null)
                {
                    btn.UpdateInteractability(newCost);
                }
            }
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

        private void HandleTurnEnd(UnitTurnEndEvent evt)
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

        private void RefreshSkillSlots()
        {
            foreach (var btn in _activeSkillButtons)
            {
                if (btn != null) btn.ReturnToPool();
            }
            _activeSkillButtons.Clear();

            if (_equippedSkills == null || _equippedSkills.Count == 0) return;

            int startIndex = _currentPage * MaxSkillsPerPage;
            int currentTurnCost = GetCurrentUnitCost();

            for (int i = 0; i < MaxSkillsPerPage; i++)
            {
                int skillIndex = startIndex + i;
                if (skillIndex < _equippedSkills.Count && _equippedSkills[skillIndex] != null)
                {
                    if (i < skillSlotPositions.Count && skillSlotPositions[i] != null)
                    {
                        var btn = _poolManager.Pop<CombatSkillButtonUI>(skillButtonPoolingSO);
                        if (btn != null)
                        {
                            btn.transform.SetParent(skillSlotPositions[i]);
                            btn.transform.localPosition = Vector3.zero;
                            btn.transform.localScale = Vector3.one;
                            
                            btn.SetupSkill(_equippedSkills[skillIndex], _currentSkillCompo, currentTurnCost);
                            _activeSkillButtons.Add(btn);
                        }
                    }
                }
            }
        }

        private void GoToNextPage()
        {
            _currentPage = 1;
            RefreshSkillSlots();

            if (nextPageButton != null) nextPageButton.gameObject.SetActive(false);
            if (prevPageButton != null) prevPageButton.gameObject.SetActive(true);
        }

        private void GoToPrevPage()
        {
            _currentPage = 0;
            RefreshSkillSlots();

            if (nextPageButton != null) nextPageButton.gameObject.SetActive(true);
            if (prevPageButton != null) prevPageButton.gameObject.SetActive(false);
        }

        private void HandleSkillSelected(CombatSkillSelectEvent evt)
        {
            _isSkillSelected = true;
        }

        private void CancelSkillSelection()
        {
            if (!_isSkillSelected) return;

            _isSkillSelected = false;
            Bus<CombatSkillCancelEvent>.Raise(new CombatSkillCancelEvent());
        }

        private int GetCurrentUnitCost()
        {
            if (_currentUnit != null && _currentUnit.SkillCostCompo != null)
            {
                return _currentUnit.SkillCostCompo.GetUnitSkillCost();
            }
            return 0;
        }

        private void SafeShowUI()
        {
            if (_equippedSkills != null && _equippedSkills.Count > 0 && _currentSkillCompo != null)
            {
                ShowUI();
            }
        }

        private void ShowUI()
        {
            if (_isCurrentlyVisible) return;
            
            _isCurrentlyVisible = true;
            _slideTween?.Kill();
            
            if (skillArea != null)
            {
                skillArea.gameObject.SetActive(true);
                _slideTween = skillArea.DOAnchorPos(visiblePosition, slideDuration).SetEase(slideEase);
            }
        }

        private void HideUI(bool raiseCancelEvent)
        {
            if (raiseCancelEvent) 
                CancelSkillSelection();
                
            if (!_isCurrentlyVisible) return;
            
            _isCurrentlyVisible = false;
            _slideTween?.Kill();
            
            if (skillArea != null)
                _slideTween = skillArea.DOAnchorPos(hiddenPosition, slideDuration).SetEase(Ease.InBack);
        }
    }
}
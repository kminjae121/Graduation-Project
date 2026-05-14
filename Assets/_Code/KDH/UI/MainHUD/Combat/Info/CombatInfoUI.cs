using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.Managers; 
using Code.SkillSystem;
using Code.UnitSystem;
using Code.UnitSystem.Combat; 
using Code.Items;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GondrLib.ObjectPool.Runtime;
using Input;

namespace Code.UI
{
    public class CombatInfoUI : MonoBehaviour
    {
        [SerializeField] private InputReader input;
        [Header("UI Panel & Animation")]
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private GameObject backgroundPanel;
        [SerializeField] private Vector2 hiddenPosition = Vector2.zero;
        [SerializeField] private Vector2 visiblePosition = Vector2.zero;
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private Ease slideEase = Ease.OutQuart;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        [Header("Basic Info")]
        [SerializeField] private Image unitImage;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI unitClassText;
        
        [Header("Health Info")]
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image hpFillImage;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI turnSpeedText;
        [SerializeField] private TextMeshProUGUI moveSpeedText;
        [SerializeField] private TextMeshProUGUI maxHealthText;
        [SerializeField] private TextMeshProUGUI attackDamageText;
        [SerializeField] private TextMeshProUGUI defensivePowerText;
        [SerializeField] private TextMeshProUGUI criticalProbabilityText;
        [SerializeField] private TextMeshProUGUI criticalDamageIncreaseText;
        [SerializeField] private TextMeshProUGUI maxSkillCostText;
        [SerializeField] private TextMeshProUGUI recoverySkillCostText;

        [Header("Skills & Artifacts")]
        [SerializeField] private RectTransform skillPanelGroup;
        [SerializeField] private PoolingItemSO characterSkillButtonPoolingSO;
        [SerializeField] private List<Image> artifactIcons;
        [SerializeField] private List<Image> artifactRarityImages;
        [SerializeField] private List<Sprite> raritySprites;
        [SerializeField] private Sprite emptyArtifactSprite;
        
        [Header("Combat Popup Offset")]
        [SerializeField] private Vector2 combatArtifactPopupOffset;

        private Tween _slideTween;
        private UnitState _manualTargetState; 
        private UnitSO _currentInGameUnitSO;
        private UnitHealth _currentInGameHealth;
        
        private bool _isVisible;
        private bool _isManualTargeting; 
        
        private PoolManagerMono _poolManager;
        private TurnManager _turnManager; 
        private List<CharacterSkillButton> _activeSkillButtons = new List<CharacterSkillButton>();

        private void Awake()
        {
            _poolManager = UnityEngine.Object.FindFirstObjectByType<PoolManagerMono>();
            _turnManager = UnityEngine.Object.FindFirstObjectByType<TurnManager>();

            if (_poolManager == null) Debug.LogError("[CombatInfoUI] 풀 매니저를 찾을 수 없습니다.");

            if (panelRect == null) panelRect = GetComponent<RectTransform>();
            panelRect.anchoredPosition = hiddenPosition;
            
            if (backgroundPanel != null) backgroundPanel.SetActive(false);
            if (openButton != null) openButton.onClick.AddListener(ShowUI);
            if (closeButton != null) closeButton.onClick.AddListener(HideUI);

            Bus<ShowCombatInfoEvent>.Subscribe(HandleShowCombatInfo);
            Bus<TurnOrderUpdateEvent>.Subscribe(HandleTurnUpdate); 
            
            SetupArtifactTriggers();
        }

        private void OnDestroy()
        {
            if (openButton != null) openButton.onClick.RemoveListener(ShowUI);
            if (closeButton != null) closeButton.onClick.RemoveListener(HideUI);
            
            Bus<ShowCombatInfoEvent>.Unsubscribe(HandleShowCombatInfo);
            Bus<TurnOrderUpdateEvent>.Unsubscribe(HandleTurnUpdate);
            
            UnsubscribeHealth();
            _slideTween?.Kill();
        }

        private void HandleTurnUpdate(TurnOrderUpdateEvent evt)
        {
            if (_isVisible && !_isManualTargeting)
            {
                UpdateToCurrentTurnUnit();
            }
        }

        private void HandleShowCombatInfo(ShowCombatInfoEvent evt)
        {
            UnsubscribeHealth();

            if (evt.IsShow && evt.TargetUnit != null)
            {
                _isManualTargeting = true; 
                _manualTargetState = evt.TargetUnit;
                
                if (_manualTargetState.CurrentHp != null)
                {
                    _manualTargetState.CurrentHp.OnValueChanged += OnManualHealthChanged;
                }
                
                ShowUI();
            }
            else
            {
                HideUI();
            }
        }

        private void UpdateToCurrentTurnUnit()
        {
            if (_turnManager == null) _turnManager = UnityEngine.Object.FindFirstObjectByType<TurnManager>();
            
            if (_turnManager != null)
            {
                var units = _turnManager.GetTimelineUnits(1);
                if (units != null && units.Count > 0)
                {
                    UnitSO nextUnitSO = null;
                    UnitHealth nextHealth = null;

                    if (units[0] is Unit unitCompo)
                    {
                        nextUnitSO = unitCompo.unitSO;
                        nextHealth = unitCompo.GetUnitCompo<UnitHealth>();
                    }

                    if (nextUnitSO != null && _currentInGameUnitSO != nextUnitSO)
                    {
                        UnsubscribeHealth();
                        _currentInGameUnitSO = nextUnitSO;
                        _currentInGameHealth = nextHealth;
                        
                        if (_currentInGameHealth != null)
                        {
                            _currentInGameHealth.OnHealthChangedEvent += OnInGameHealthChanged;
                        }
                        
                        if (_isVisible) RefreshAllUI();
                    }
                }
            }
        }

        private void UnsubscribeHealth()
        {
            if (_manualTargetState != null && _manualTargetState.CurrentHp != null)
            {
                _manualTargetState.CurrentHp.OnValueChanged -= OnManualHealthChanged;
            }
            if (_currentInGameHealth != null)
            {
                _currentInGameHealth.OnHealthChangedEvent -= OnInGameHealthChanged;
            }
        }

        private void OnManualHealthChanged(float prevHp, float nextHp) => RefreshHealthUI();
        
        private void OnInGameHealthChanged(float currentHp, float maxHp) => RefreshHealthUI();

        public void ShowUI()
        {
            if (_isVisible) return;
            
            if (!_isManualTargeting)
            {
                UpdateToCurrentTurnUnit();
            }

            _isVisible = true;
            if (backgroundPanel != null) backgroundPanel.SetActive(true);

            RefreshAllUI();

            _slideTween?.Kill();
            _slideTween = panelRect.DOAnchorPos(visiblePosition, slideDuration).SetEase(slideEase).OnComplete(() =>
            {
                input._controls.Player.Disable();
            });
        }

        public void HideUI()
        {
            if (!_isVisible) return;
            
            _isVisible = false;
            _isManualTargeting = false; 
            
            _slideTween?.Kill();
            _slideTween = panelRect.DOAnchorPos(hiddenPosition, slideDuration).SetEase(Ease.InBack).OnComplete(() => 
            {
                if (backgroundPanel != null) backgroundPanel.SetActive(false);
            });
            
            Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            Bus<CombatSkillHoverEvent>.Raise(new CombatSkillHoverEvent(null, null)); 
            Bus<CombatArtifactHoverEvent>.Raise(new CombatArtifactHoverEvent(null, false));

            UnsubscribeHealth();
            _manualTargetState = null; 
            _currentInGameUnitSO = null;
            _currentInGameHealth = null;
            
            input._controls.Player.Enable();
        }

        private UnitSO GetCurrentUnitSO()
        {
            if (_isManualTargeting && _manualTargetState != null) return _manualTargetState.Data;
            if (!_isManualTargeting) return _currentInGameUnitSO;
            return null;
        }

        private void RefreshAllUI()
        {
            var data = GetCurrentUnitSO();
            if (data == null) return;
            
            if (unitImage != null) unitImage.sprite = data.UnitImage;
            if (unitNameText != null) unitNameText.text = data.UnitName;
            if (unitClassText != null) unitClassText.text = data.UnitClass;
            
            RefreshHealthUI();

            if (turnSpeedText != null) turnSpeedText.text = data.Speed.ToString("F1");
            if (moveSpeedText != null) moveSpeedText.text = data.MoveRange.ToString("F1");
            if (maxHealthText != null) maxHealthText.text = data.Maxhealth.ToString("F1");
            if (attackDamageText != null) attackDamageText.text = data.AttackDamage.ToString("F1");
            if (defensivePowerText != null) defensivePowerText.text = data.DefensivePower.ToString("F1");
            if (criticalProbabilityText != null) criticalProbabilityText.text = $"{data.CriticalProbability:F1}%";
            if (criticalDamageIncreaseText != null) criticalDamageIncreaseText.text = data.CriticalDamageIncrease.ToString("F1");
            if (maxSkillCostText != null) maxSkillCostText.text = data.MaxManaCost.ToString();
            if (recoverySkillCostText != null) recoverySkillCostText.text = data.RecoveryManaCost.ToString();

            RefreshSkills();
            RefreshArtifacts();
        }

        private void RefreshHealthUI()
        {
            float currentHp = 0f;
            float maxHp = 0f;

            if (_isManualTargeting && _manualTargetState != null && _manualTargetState.Data != null)
            {
                currentHp = _manualTargetState.CurrentHp.Value;
                maxHp = _manualTargetState.Data.Maxhealth;
            }
            else if (!_isManualTargeting && _currentInGameUnitSO != null && _currentInGameHealth != null)
            {
                currentHp = _currentInGameHealth.CurrentHealth;
                maxHp = _currentInGameHealth.MaxHealth;
            }

            if (hpText != null) hpText.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
            if (hpFillImage != null) hpFillImage.fillAmount = maxHp > 0 ? (currentHp / maxHp) : 0f;
        }

        private void RefreshSkills()
        {
            SkillSO[] equippedSkills = System.Array.Empty<SkillSO>();
            var data = GetCurrentUnitSO();
            
            if (SkillSendManager.Instance != null && data != null)
                equippedSkills = SkillSendManager.Instance.GetEquipSkills(data.UnitType);

            if (equippedSkills == null) equippedSkills = System.Array.Empty<SkillSO>();

            foreach (var btn in _activeSkillButtons)
            {
                if (btn != null) btn.ReturnToPool();
            }
            _activeSkillButtons.Clear();

            if (skillPanelGroup != null && characterSkillButtonPoolingSO != null)
            {
                for (int i = 0; i < equippedSkills.Length; i++)
                {
                    if (equippedSkills[i] != null)
                    {
                        var btn = _poolManager.Pop<CharacterSkillButton>(characterSkillButtonPoolingSO);
                        if (btn != null)
                        {
                            btn.transform.SetParent(skillPanelGroup);
                            btn.transform.localScale = Vector3.one;
                            btn.IsCombatMode = true; 
                            btn.SetSkill(equippedSkills[i], true);
                            _activeSkillButtons.Add(btn);
                        }
                    }
                }
            }
        }

        private void RefreshArtifacts()
        {
            var data = GetCurrentUnitSO();
            for (int i = 0; i < artifactIcons.Count; i++)
            {
                bool hasArtifact = data != null && data.EquippedArtifacts != null && 
                                   i < data.EquippedArtifacts.artifacts.Count && data.EquippedArtifacts.artifacts[i] != null;

                if (hasArtifact)
                {
                    var artifact = data.EquippedArtifacts.artifacts[i];
                    artifactIcons[i].sprite = artifact.itemIcon;

                    if (i < artifactRarityImages.Count && artifactRarityImages[i] != null)
                    {
                        if (artifact is EquipmentItemSO equipSO)
                        {
                            artifactRarityImages[i].gameObject.SetActive(true);
                            int rarityIndex = (int)equipSO.rarity;
                            if (raritySprites != null && rarityIndex >= 0 && rarityIndex < raritySprites.Count)
                            {
                                artifactRarityImages[i].sprite = raritySprites[rarityIndex];
                            }
                        }
                        else
                        {
                            artifactRarityImages[i].gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    artifactIcons[i].sprite = emptyArtifactSprite;
                    if (i < artifactRarityImages.Count && artifactRarityImages[i] != null)
                    {
                        artifactRarityImages[i].gameObject.SetActive(false);
                    }
                }
                
                var trigger = artifactIcons[i].GetComponent<SlotHoverClickTrigger>();
                if (trigger != null) trigger.SetInteractable(hasArtifact);
            }
        }

        private void SetupArtifactTriggers()
        {
            for (int i = 0; i < artifactIcons.Count; i++)
            {
                int index = i;
                var trigger = artifactIcons[i].GetComponent<SlotHoverClickTrigger>();
                if (trigger == null) trigger = artifactIcons[i].gameObject.AddComponent<SlotHoverClickTrigger>();
                
                trigger.useHoverVisuals = false;
                trigger.OnHoverEnter = (pivot, triggerOffset) =>
                {
                    var data = GetCurrentUnitSO();
                    if (data != null && data.EquippedArtifacts != null)
                    {
                        var artifacts = data.EquippedArtifacts.artifacts;
                        if (index < artifacts.Count && artifacts[index] != null)
                        {
                            Bus<CombatArtifactHoverEvent>.Raise(new CombatArtifactHoverEvent(artifacts[index], true, pivot, combatArtifactPopupOffset));
                        }
                    }
                };
                trigger.OnHoverExit = () => Bus<CombatArtifactHoverEvent>.Raise(new CombatArtifactHoverEvent(null, false));
            }
        }
    }
}
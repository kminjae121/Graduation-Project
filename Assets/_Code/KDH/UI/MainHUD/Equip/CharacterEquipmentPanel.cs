using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Code.Item;
using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.Items;
using Code.UnitSystem.ArtifactSystem;
using Code.SkillSystem;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Code.UnitManaging;

namespace Code.UI
{
    public class CharacterEquipmentPanel : Panel
    {
        [Header("Pool Settings")]
        [SerializeField] private PoolingItemSO artifactButtonPoolingSO;
        [SerializeField] private PoolingItemSO skillButtonPoolingSO;

        [Header("Unit Data")]
        [SerializeField] private UnitStorageSO unitStorageSO;

        [Header("Artifact Inventory Area")]
        [SerializeField] private Transform artifactInventoryTrm; 
        [SerializeField] private TextMeshProUGUI artifactCountText;
        [SerializeField] private Button artifactSortButton;
        [SerializeField] private TextMeshProUGUI artifactSortText;
        [SerializeField] private int maxArtifactInventoryCapacity = 20;
        [SerializeField] private int maxArtifactEquipCount = 2;

        [Header("Skill Containers")]
        [SerializeField] private Transform ownSkillContainer;

        [Header("Skill Loadout Settings")]
        [SerializeField] private Image skillLoadoutFillImage;
        [SerializeField] private TextMeshProUGUI skillLoadoutText;
        [SerializeField] private float fillAnimationDuration = 0.3f;

        [Inject] private PoolManagerMono _poolManager;
        
        private UnitSO _unit;
        private List<ArtifactButton> _activeArtifactButtons = new();
        private List<CharacterSkillButton> _activeSkillButtons = new();
        private bool _isArtifactSortedByRarity = false; 
        private Coroutine _fillCoroutine;

        public override void Awake()
        {
            base.Awake();

            if (_poolManager == null)
                _poolManager = FindFirstObjectByType<PoolManagerMono>();

            if (artifactSortButton != null)
                artifactSortButton.onClick.AddListener(ToggleArtifactSort);

            Bus<CharacterInfoEvent>.Subscribe(HandleCharacterInfo);
            Bus<ArtifactEquipEvent>.Subscribe(HandleArtifactEquip);
            Bus<ArtifactUnequipEvent>.Subscribe(HandleArtifactUnequip);
            Bus<SkillEquipEvent>.Subscribe(HandleSkillEquipped);
            Bus<SkillUnequipEvent>.Subscribe(HandleSkillUnequipped);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (artifactSortButton != null)
                artifactSortButton.onClick.RemoveListener(ToggleArtifactSort);

            Bus<CharacterInfoEvent>.Unsubscribe(HandleCharacterInfo);
            Bus<ArtifactEquipEvent>.Unsubscribe(HandleArtifactEquip);
            Bus<ArtifactUnequipEvent>.Unsubscribe(HandleArtifactUnequip);
            Bus<SkillEquipEvent>.Unsubscribe(HandleSkillEquipped);
            Bus<SkillUnequipEvent>.Unsubscribe(HandleSkillUnequipped);
        }

        public override void Open()
        {
            base.Open();
            if (_unit != null)
            {
                if (SkillSendManager.Instance != null)
                    SkillSendManager.Instance.SyncEquippedSkills(_unit);

                RefreshArtifactUI();
                RefreshSkillList();
                RefreshSkillLoadoutUI(true);
            }
        }

        private void HandleCharacterInfo(CharacterInfoEvent evt)
        {
            _unit = evt.Unit.Data;
            
            if (_unit != null && SkillSendManager.Instance != null)
                SkillSendManager.Instance.SyncEquippedSkills(_unit);

            if (IsOpen)
            {
                RefreshArtifactUI();
                RefreshSkillList();
                RefreshSkillLoadoutUI(true);
            }
        }

        private void ToggleArtifactSort()
        {
            _isArtifactSortedByRarity = !_isArtifactSortedByRarity;
            if (artifactSortText != null) artifactSortText.text = _isArtifactSortedByRarity ? "희귀도순" : "획득순";
            RefreshArtifactUI();
        }

        private List<EquipmentItemSO> GetAvailableArtifacts()
        {
            List<EquipmentItemSO> availableArtifacts = new List<EquipmentItemSO>();
            
            if (_unit == null) return availableArtifacts;

            if (_unit.EquippedArtifacts != null && _unit.EquippedArtifacts.artifacts != null)
            {
                availableArtifacts.AddRange(_unit.EquippedArtifacts.artifacts);
            }

            if (_unit.OwnArtifactStorage != null && _unit.OwnArtifactStorage.artifacts != null)
            {
                availableArtifacts.AddRange(_unit.OwnArtifactStorage.artifacts);
            }

            return availableArtifacts;
        }

        private void RefreshArtifactUI()
        {
            if (_unit == null || _unit.OwnArtifactStorage == null) return;

            int currentCount = (_unit.OwnArtifactStorage.artifacts?.Count ?? 0) + 
                               (_unit.EquippedArtifacts?.artifacts?.Count ?? 0);
            if (artifactCountText != null) artifactCountText.text = $"{currentCount}/{maxArtifactInventoryCapacity}";

            foreach (var btn in _activeArtifactButtons) btn.ReturnToPool();
            _activeArtifactButtons.Clear();

            var equippedList = _unit.EquippedArtifacts?.artifacts ?? new List<EquipmentItemSO>();
            var displayList = GetAvailableArtifacts();

            if (_isArtifactSortedByRarity) displayList = displayList.OrderByDescending(a => a.rarity).ToList();

            foreach (var artifact in displayList)
            {
                var btn = _poolManager.Pop<ArtifactButton>(artifactButtonPoolingSO);
                btn.transform.SetParent(artifactInventoryTrm);
                btn.transform.localScale = Vector3.one;

                bool isEquipped = equippedList.Contains(artifact);
                btn.SetArtifact(artifact, isEquipped);
                
                if (isEquipped) btn.transform.SetAsFirstSibling();
                else btn.transform.SetAsLastSibling();

                _activeArtifactButtons.Add(btn);
            }
        }

        private void HandleArtifactEquip(ArtifactEquipEvent evt)
        {
            if (_unit == null || _unit.EquippedArtifacts == null) return;
            if (_unit.EquippedArtifacts.artifacts.Contains(evt.EquipmentItem)) return;

            if (_unit.EquippedArtifacts.artifacts.Count >= maxArtifactEquipCount)
            {
                Bus<WarningUIEvent>.Raise(new WarningUIEvent($"아티팩트는 최대 {maxArtifactEquipCount}개까지만 장착할 수 있습니다."));
                return;
            }
            
            if (_unit.OwnArtifactStorage != null)
            {
                _unit.OwnArtifactStorage.artifacts.Remove(evt.EquipmentItem);
            }

            ItemStorage.Instance.SetItem(_unit.UnitType, evt.EquipmentItem);
            _unit.EquippedArtifacts.artifacts.Add(evt.EquipmentItem);
            
            RefreshArtifactUI();
        }

        private void HandleArtifactUnequip(ArtifactUnequipEvent evt)
        {
            if (_unit == null || _unit.EquippedArtifacts == null) return;
            
            if (_unit.EquippedArtifacts.artifacts.Remove(evt.EquipmentItem))
            {
                if (_unit.OwnArtifactStorage != null)
                {
                    _unit.OwnArtifactStorage.artifacts.Add(evt.EquipmentItem);
                }

                ItemStorage.Instance.RemoveItem(_unit.UnitType, evt.EquipmentItem);
                
                RefreshArtifactUI();
            }
        }

        private void RefreshSkillList()
        {
            if (_unit == null || SkillSendManager.Instance == null) return;

            foreach (var btn in _activeSkillButtons) btn.ReturnToPool();
            _activeSkillButtons.Clear();

            var availableSkills = SkillSendManager.Instance.GetSkillList(_unit.UnitType);
            var equippedSkills = SkillSendManager.Instance.GetEquipSkills(_unit.UnitType);

            foreach (var skillSO in availableSkills)
            {
                var btn = _poolManager.Pop<CharacterSkillButton>(skillButtonPoolingSO);
                btn.transform.SetParent(ownSkillContainer);
                btn.transform.localScale = Vector3.one;

                bool isEquipped = equippedSkills.Contains(skillSO);
                btn.SetSkill(skillSO, isEquipped);
                
                if (isEquipped) btn.transform.SetAsFirstSibling();
                else btn.transform.SetAsLastSibling();
                
                _activeSkillButtons.Add(btn);
            }
        }

        private void RefreshSkillLoadoutUI(bool instant = false)
        {
            if (_unit == null) return;

            int currentCost = GetCurrentSkillLoadoutCost();
            int maxCost = _unit.LoadOutCost;

            if (skillLoadoutText != null) skillLoadoutText.text = $"{currentCost} / {maxCost}";

            if (skillLoadoutFillImage != null)
            {
                skillLoadoutFillImage.type = Image.Type.Filled;
                skillLoadoutFillImage.fillMethod = Image.FillMethod.Vertical;
                skillLoadoutFillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
                
                float targetFillAmount = maxCost > 0 ? (float)currentCost / maxCost : 0f;

                if (instant || !gameObject.activeInHierarchy)
                {
                    skillLoadoutFillImage.fillAmount = targetFillAmount;
                    if (_fillCoroutine != null)
                    {
                        StopCoroutine(_fillCoroutine);
                        _fillCoroutine = null;
                    }
                }
                else
                {
                    if (_fillCoroutine != null) StopCoroutine(_fillCoroutine);
                    _fillCoroutine = StartCoroutine(CoSmoothFill(targetFillAmount));
                }
            }
        }

        private IEnumerator CoSmoothFill(float targetAmount)
        {
            if (skillLoadoutFillImage == null) yield break;

            float startAmount = skillLoadoutFillImage.fillAmount;
            float elapsedTime = 0f;

            while (elapsedTime < fillAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fillAnimationDuration;
                t = t * t * (3f - 2f * t);

                skillLoadoutFillImage.fillAmount = Mathf.Lerp(startAmount, targetAmount, t);
                yield return null;
            }

            skillLoadoutFillImage.fillAmount = targetAmount;
            _fillCoroutine = null;
        }

        private int GetCurrentSkillLoadoutCost()
        {
            int totalCost = 0;
            if (SkillSendManager.Instance != null && _unit != null)
            {
                var equippedSkills = SkillSendManager.Instance.GetEquipSkills(_unit.UnitType);
                foreach (var skill in equippedSkills)
                {
                    if (skill != null) totalCost += skill.SkillValue;
                }
            }
            return totalCost;
        }

        private void HandleSkillEquipped(SkillEquipEvent evt)
        {
            if (_unit == null || evt.Skill == null || evt.Skill.unitType != _unit.UnitType) return;
            if (SkillSendManager.Instance == null) return;

            var equippedSkills = SkillSendManager.Instance.GetEquipSkills(_unit.UnitType);

            if (equippedSkills.Length >= 4)
            {
                Bus<WarningUIEvent>.Raise(new WarningUIEvent("스킬은 최대 4개까지만 장착할 수 있습니다."));
                return;
            }

            int currentCost = GetCurrentSkillLoadoutCost();
            if (currentCost + evt.Skill.SkillValue > _unit.LoadOutCost)
            {
                Bus<WarningUIEvent>.Raise(new WarningUIEvent("스킬 코스트 총량을 초과하여 장착할 수 없습니다."));
                return;
            }

            if (_unit.SkillStorage != null && !_unit.SkillStorage.skills.Contains(evt.Skill))
                _unit.SkillStorage.skills.Add(evt.Skill);

            SkillSendManager.Instance.SyncEquippedSkills(_unit);

            if (IsOpen)
            {
                RefreshSkillList();
                RefreshSkillLoadoutUI();
            }
        }

        private void HandleSkillUnequipped(SkillUnequipEvent evt)
        {
            if (_unit == null || evt.Skill == null || evt.Skill.unitType != _unit.UnitType) return;
            
            if (_unit.SkillStorage != null)
                _unit.SkillStorage.skills.Remove(evt.Skill);

            if (SkillSendManager.Instance != null)
                SkillSendManager.Instance.SyncEquippedSkills(_unit);
            
            if (IsOpen)
            {
                RefreshSkillList();
                RefreshSkillLoadoutUI();
            }
        }
    }
}
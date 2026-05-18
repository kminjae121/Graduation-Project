using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Code.Item;
using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.Items;
using Code.SkillSystem;
using Code.UnitManaging;
using Code.UnitSystem;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class CharacterEquipmentPanel : MonoBehaviour
    {
        [Header("Legacy Panel Binding")]
        [SerializeField] private string id = "EquipPanel";
        [SerializeField] private RectTransform container;

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

        private CharacterMainPanel _owner;
        private UnitSO _unit;
        private readonly List<ArtifactButton> _activeArtifactButtons = new();
        private readonly List<CharacterSkillButton> _activeSkillButtons = new();
        private bool _isArtifactSortedByRarity;
        private Coroutine _fillCoroutine;

        public bool IsVisible => GetContainer().gameObject.activeSelf;

        private void Awake()
        {
            if (_poolManager == null)
                _poolManager = FindFirstObjectByType<PoolManagerMono>();

            if (artifactSortButton != null)
                artifactSortButton.onClick.AddListener(ToggleArtifactSort);

            Bus<ArtifactEquipEvent>.Subscribe(HandleArtifactEquip);
            Bus<ArtifactUnequipEvent>.Subscribe(HandleArtifactUnequip);
            Bus<SkillEquipEvent>.Subscribe(HandleSkillEquipped);
            Bus<SkillUnequipEvent>.Subscribe(HandleSkillUnequipped);
        }

        private void OnDestroy()
        {
            if (artifactSortButton != null)
                artifactSortButton.onClick.RemoveListener(ToggleArtifactSort);

            Bus<ArtifactEquipEvent>.Unsubscribe(HandleArtifactEquip);
            Bus<ArtifactUnequipEvent>.Unsubscribe(HandleArtifactUnequip);
            Bus<SkillEquipEvent>.Unsubscribe(HandleSkillEquipped);
            Bus<SkillUnequipEvent>.Unsubscribe(HandleSkillUnequipped);

            ClearArtifactButtons();
            ClearSkillButtons();
        }

        public void Initialize(CharacterMainPanel owner)
        {
            _owner = owner;
            Hide();
        }

        public bool MatchesPanelId(string panelId)
            => string.Equals(id, panelId, System.StringComparison.OrdinalIgnoreCase);

        public void SetUnit(UnitState unitState)
        {
            _unit = unitState?.Data;

            if (_unit != null && SkillSendManager.Instance != null)
                SkillSendManager.Instance.SyncEquippedSkills(_unit);

            if (IsVisible)
                RefreshView();
        }

        public void Show()
        {
            GetContainer().gameObject.SetActive(true);
            RefreshView();
        }

        public void Hide()
        {
            GetContainer().gameObject.SetActive(false);
            Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            Bus<SkillEquipPopupEvent>.Raise(new SkillEquipPopupEvent(null, false, null));
            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
        }

        public void RefreshView()
        {
            if (_unit == null)
                return;

            if (SkillSendManager.Instance != null)
                SkillSendManager.Instance.SyncEquippedSkills(_unit);

            RefreshArtifactUI();
            RefreshSkillList();
            RefreshSkillLoadoutUI(true);
        }

        private void ToggleArtifactSort()
        {
            _isArtifactSortedByRarity = !_isArtifactSortedByRarity;

            if (artifactSortText != null)
                artifactSortText.text = _isArtifactSortedByRarity ? "희귀도순" : "획득순";

            RefreshArtifactUI();
        }

        private List<EquipmentItemSO> GetAvailableArtifacts()
        {
            List<EquipmentItemSO> availableArtifacts = new();

            if (_unit == null)
                return availableArtifacts;

            if (_unit.EquippedArtifacts?.artifacts != null)
                availableArtifacts.AddRange(_unit.EquippedArtifacts.artifacts);

            if (_unit.OwnArtifactStorage?.artifacts != null)
                availableArtifacts.AddRange(_unit.OwnArtifactStorage.artifacts);

            return availableArtifacts;
        }

        private void RefreshArtifactUI()
        {
            if (_unit == null)
                return;

            int currentCount = (_unit.OwnArtifactStorage?.artifacts?.Count ?? 0) +
                               (_unit.EquippedArtifacts?.artifacts?.Count ?? 0);

            if (artifactCountText != null)
                artifactCountText.text = $"{currentCount}/{maxArtifactInventoryCapacity}";

            ClearArtifactButtons();

            if (!CanSpawnArtifactButtons())
                return;

            List<EquipmentItemSO> equippedList = _unit.EquippedArtifacts?.artifacts ?? new List<EquipmentItemSO>();
            List<EquipmentItemSO> displayList = GetAvailableArtifacts();

            if (_isArtifactSortedByRarity)
                displayList = displayList.OrderByDescending(a => a.rarity).ToList();

            foreach (EquipmentItemSO artifact in displayList)
            {
                if (artifact == null)
                    continue;

                ArtifactButton btn = _poolManager.Pop<ArtifactButton>(artifactButtonPoolingSO);
                btn.transform.SetParent(artifactInventoryTrm);
                btn.transform.localScale = Vector3.one;

                bool isEquipped = equippedList.Contains(artifact);
                btn.SetArtifact(artifact, isEquipped);

                if (isEquipped)
                    btn.transform.SetAsFirstSibling();
                else
                    btn.transform.SetAsLastSibling();

                _activeArtifactButtons.Add(btn);
            }
        }

        private void HandleArtifactEquip(ArtifactEquipEvent evt)
        {
            if (_unit == null || evt.EquipmentItem == null || _unit.EquippedArtifacts == null)
                return;

            if (_unit.EquippedArtifacts.artifacts.Contains(evt.EquipmentItem))
                return;

            if (_unit.EquippedArtifacts.artifacts.Count >= maxArtifactEquipCount)
            {
                Bus<WarningUIEvent>.Raise(new WarningUIEvent($"아티팩트는 최대 {maxArtifactEquipCount}개까지만 장착할 수 있습니다."));
                return;
            }

            _unit.OwnArtifactStorage?.artifacts.Remove(evt.EquipmentItem);

            ItemStorage.Instance.SetItem(_unit.UnitType, evt.EquipmentItem);
            _unit.EquippedArtifacts.artifacts.Add(evt.EquipmentItem);

            NotifyEquipmentChanged();
        }

        private void HandleArtifactUnequip(ArtifactUnequipEvent evt)
        {
            if (_unit == null || evt.EquipmentItem == null || _unit.EquippedArtifacts == null)
                return;

            if (!_unit.EquippedArtifacts.artifacts.Remove(evt.EquipmentItem))
                return;

            _unit.OwnArtifactStorage?.artifacts.Add(evt.EquipmentItem);

            ItemStorage.Instance.RemoveItem(_unit.UnitType, evt.EquipmentItem);
            NotifyEquipmentChanged();
        }

        private void RefreshSkillList()
        {
            if (_unit == null || SkillSendManager.Instance == null)
                return;

            ClearSkillButtons();

            if (!CanSpawnSkillButtons())
                return;

            IEnumerable<SkillSO> availableSkills = SkillSendManager.Instance.GetSkillList(_unit.UnitType) ?? Enumerable.Empty<SkillSO>();
            SkillSO[] equippedSkills = SkillSendManager.Instance.GetEquipSkills(_unit.UnitType) ?? System.Array.Empty<SkillSO>();

            foreach (SkillSO skillSO in availableSkills)
            {
                if (skillSO == null)
                    continue;

                CharacterSkillButton btn = _poolManager.Pop<CharacterSkillButton>(skillButtonPoolingSO);
                btn.transform.SetParent(ownSkillContainer);
                btn.transform.localScale = Vector3.one;

                bool isEquipped = equippedSkills.Contains(skillSO);
                btn.SetSkill(skillSO, isEquipped);

                if (isEquipped)
                    btn.transform.SetAsFirstSibling();
                else
                    btn.transform.SetAsLastSibling();

                _activeSkillButtons.Add(btn);
            }
        }

        private void RefreshSkillLoadoutUI(bool instant = false)
        {
            if (_unit == null)
                return;

            int currentCost = GetCurrentSkillLoadoutCost();
            int maxCost = _unit.LoadOutCost;

            if (skillLoadoutText != null)
                skillLoadoutText.text = $"{currentCost} / {maxCost}";

            if (skillLoadoutFillImage == null)
                return;

            skillLoadoutFillImage.type = Image.Type.Filled;
            skillLoadoutFillImage.fillMethod = Image.FillMethod.Vertical;
            skillLoadoutFillImage.fillOrigin = (int)Image.OriginVertical.Bottom;

            float targetFillAmount = maxCost > 0 ? (float)currentCost / maxCost : 0f;

            if (instant || !gameObject.activeInHierarchy)
            {
                skillLoadoutFillImage.fillAmount = targetFillAmount;
                StopFillCoroutine();
                return;
            }

            StopFillCoroutine();
            _fillCoroutine = StartCoroutine(CoSmoothFill(targetFillAmount));
        }

        private IEnumerator CoSmoothFill(float targetAmount)
        {
            if (skillLoadoutFillImage == null)
                yield break;

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

            if (SkillSendManager.Instance == null || _unit == null)
                return totalCost;

            SkillSO[] equippedSkills = SkillSendManager.Instance.GetEquipSkills(_unit.UnitType) ?? System.Array.Empty<SkillSO>();
            foreach (SkillSO skill in equippedSkills)
                if (skill != null)
                    totalCost += skill.SkillValue;

            return totalCost;
        }

        private void HandleSkillEquipped(SkillEquipEvent evt)
        {
            if (_unit == null || evt.Skill == null || evt.Skill.unitType != _unit.UnitType)
                return;

            if (SkillSendManager.Instance == null)
                return;

            SkillSO[] equippedSkills = SkillSendManager.Instance.GetEquipSkills(_unit.UnitType) ?? System.Array.Empty<SkillSO>();

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
            NotifyEquipmentChanged();
        }

        private void HandleSkillUnequipped(SkillUnequipEvent evt)
        {
            if (_unit == null || evt.Skill == null || evt.Skill.unitType != _unit.UnitType)
                return;

            _unit.SkillStorage?.skills.Remove(evt.Skill);

            if (SkillSendManager.Instance != null)
                SkillSendManager.Instance.SyncEquippedSkills(_unit);

            NotifyEquipmentChanged();
        }

        private void NotifyEquipmentChanged()
        {
            if (_owner != null)
            {
                _owner.RefreshViewsAfterEquipmentChanged();
                return;
            }

            if (IsVisible)
                RefreshView();
        }

        private void ClearArtifactButtons()
        {
            foreach (ArtifactButton btn in _activeArtifactButtons)
                if (btn != null)
                    btn.ReturnToPool();

            _activeArtifactButtons.Clear();
        }

        private void ClearSkillButtons()
        {
            foreach (CharacterSkillButton btn in _activeSkillButtons)
                if (btn != null)
                    btn.ReturnToPool();

            _activeSkillButtons.Clear();
        }

        private bool CanSpawnArtifactButtons()
        {
            return _poolManager != null &&
                   artifactButtonPoolingSO != null &&
                   artifactInventoryTrm != null;
        }

        private bool CanSpawnSkillButtons()
        {
            return _poolManager != null &&
                   skillButtonPoolingSO != null &&
                   ownSkillContainer != null;
        }

        private RectTransform GetContainer()
        {
            if (container == null)
                container = transform as RectTransform;

            return container;
        }

        private void StopFillCoroutine()
        {
            if (_fillCoroutine == null)
                return;

            StopCoroutine(_fillCoroutine);
            _fillCoroutine = null;
        }
    }
}

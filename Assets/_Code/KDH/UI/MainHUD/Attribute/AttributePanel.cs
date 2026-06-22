using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.Items;
using Code.SkillSystem;
using Code.UnitSystem;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class AttributePanel : MonoBehaviour
    {
        [Header("Legacy Panel Binding")]
        [SerializeField] private string id = "AttributePanel";
        [SerializeField] private RectTransform container;

        [Header("Pool Settings")]
        [SerializeField] private PoolingItemSO artifactButtonPoolingSO;
        [SerializeField] private PoolingItemSO skillButtonPoolingSO;

        [Header("Equipped Artifacts")]
        [SerializeField] private List<Image> artifactIcons;
        [SerializeField] private List<Image> artifactRarityIcons;
        [SerializeField] private Sprite emptyArtifactSlotSprite;

        [Header("Skill Inventory")]
        [SerializeField] private Transform ownSkillContainer;
        [SerializeField] private Image skillLoadoutFillImage;
        [SerializeField] private TextMeshProUGUI skillLoadoutText;
        [SerializeField] private float fillAnimationDuration = 0.3f;

        [Header("KeyStats")]
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI turnSpeedText;

        [Header("Stats & Info")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI classText;

        [SerializeField] private TextMeshProUGUI maxHealthText;
        [SerializeField] private TextMeshProUGUI atkText;
        [SerializeField] private TextMeshProUGUI defText;
        [SerializeField] private TextMeshProUGUI moveSpeedText;
        [SerializeField] private TextMeshProUGUI criticalProbabilityText;
        [SerializeField] private TextMeshProUGUI criticalDamageIncreaseText;
        [SerializeField] private TextMeshProUGUI maxSkillCostText;
        [SerializeField] private TextMeshProUGUI recoverySkillCostText;

        [Inject] private PoolManagerMono _poolManager;

        private MainPanel _owner;
        private UnitState _currentUnit;
        private readonly List<CharacterSkillButton> _activeSkillButtons = new();
        private bool _triggersInitialized;
        private Coroutine _fillCoroutine;
        private Coroutine _textCoroutine;
        private int _displayedSkillLoadoutCost;
        private bool _hasSkillLoadoutValue;

        public bool IsVisible => GetContainer().gameObject.activeSelf;

        private void Awake()
        {
            ResolvePoolManager();
            SetupArtifactSlotTriggers();

            Bus<SkillEquipEvent>.Subscribe(HandleSkillEquipped);
            Bus<SkillUnequipEvent>.Subscribe(HandleSkillUnequipped);
        }

        private void OnDestroy()
        {
            Bus<SkillEquipEvent>.Unsubscribe(HandleSkillEquipped);
            Bus<SkillUnequipEvent>.Unsubscribe(HandleSkillUnequipped);

            UnsubscribeHpEvent();
            StopSkillLoadoutAnimation();
            ClearSkillButtons();
        }

        public void Initialize(MainPanel owner)
        {
            _owner = owner;
            ResolvePoolManager();
            SetupArtifactSlotTriggers();
            Hide();
        }

        public bool MatchesPanelId(string panelId)
            => string.Equals(id, panelId, System.StringComparison.OrdinalIgnoreCase);

        public void SetUnit(UnitState unit)
        {
            bool isUnitChanged = !ReferenceEquals(_currentUnit, unit);

            UnsubscribeHpEvent();
            _currentUnit = unit;

            if (isUnitChanged)
                ResetSkillLoadoutAnimationState();

            if (_currentUnit?.CurrentHp != null)
                _currentUnit.CurrentHp.OnValueChanged += RefreshHpBar;

            if (_currentUnit?.Data != null && SkillSendManager.Instance != null)
                SkillSendManager.Instance.SyncEquippedSkills(_currentUnit.Data);

            if (IsVisible)
                RefreshView();
        }

        public void Show()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            GetContainer().gameObject.SetActive(true);
            RefreshView();
        }

        public void Hide()
        {
            GetContainer().gameObject.SetActive(false);
            Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            Bus<SkillEquipPopupEvent>.Raise(new SkillEquipPopupEvent(null, false, null));
            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
            StopSkillLoadoutAnimation();
        }

        public void RefreshView()
        {
            if (_currentUnit?.Data == null)
            {
                ClearSkillButtons();
                return;
            }

            if (SkillSendManager.Instance != null)
                SkillSendManager.Instance.SyncEquippedSkills(_currentUnit.Data);

            RefreshInfoTexts();
            RefreshHpBar(0f, _currentUnit.CurrentHp != null ? _currentUnit.CurrentHp.Value : _currentUnit.Data.Maxhealth);
            RefreshArtifactSlots();
            RefreshSkillList();
            RefreshSkillLoadoutUI(!_hasSkillLoadoutValue);
        }

        private void SetupArtifactSlotTriggers()
        {
            if (_triggersInitialized)
                return;

            _triggersInitialized = true;

            Vector2 defaultArtifactOffset = Vector2.zero;
            if (artifactButtonPoolingSO != null && artifactButtonPoolingSO.prefab != null)
            {
                ArtifactButton btn = artifactButtonPoolingSO.prefab.GetComponent<ArtifactButton>();
                if (btn != null)
                    defaultArtifactOffset = btn.EquippedPopupOffset;
            }

            if (artifactIcons == null)
                return;

            for (int i = 0; i < artifactIcons.Count; i++)
            {
                if (artifactIcons[i] == null)
                    continue;

                int index = i;
                SlotHoverClickTrigger trigger = artifactIcons[i].GetComponent<SlotHoverClickTrigger>();
                if (trigger == null)
                    trigger = artifactIcons[i].gameObject.AddComponent<SlotHoverClickTrigger>();

                trigger.useHoverVisuals = false;
                trigger.OnClick = (_, _) => RequestInventoryTab();
                trigger.OnHoverEnter = (pivot, triggerOffset) =>
                {
                    if (_currentUnit?.Data?.EquippedArtifacts?.artifacts == null)
                        return;

                    List<EquipmentItemSO> artifacts = _currentUnit.Data.EquippedArtifacts.artifacts;
                    if (index >= artifacts.Count || artifacts[index] == null)
                        return;

                    Vector2 finalOffset = triggerOffset != Vector2.zero ? triggerOffset : defaultArtifactOffset;
                    Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(artifacts[index], true, pivot, finalOffset, true));
                };
                trigger.OnHoverExit = () => Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
            }
        }

        private void RequestInventoryTab()
        {
            Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            Bus<SkillEquipPopupEvent>.Raise(new SkillEquipPopupEvent(null, false, null));
            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));

            if (_owner != null)
            {
                _owner.ShowTab(UnitPanelTab.Inventory);
                return;
            }

            MainPanel.TryOpenTab("InventoryPanel");
        }

        private void RefreshInfoTexts()
        {
            UnitSO data = _currentUnit.Data;

            if (nameText != null)
                nameText.text = data.UnitName;

            if (classText != null)
                classText.text = data.UnitClass;

            if (maxHealthText != null)
                maxHealthText.text = (data.Maxhealth + GetStatBonus(StatInfo.MaxHealth, data.UnitType)).ToString();

            if (atkText != null)
                atkText.text = (data.AttackDamage + GetStatBonus(StatInfo.AtkDamage, data.UnitType)).ToString();

            if (defText != null)
                defText.text = (data.DefensivePower + GetStatBonus(StatInfo.DefensivePower, data.UnitType)).ToString();

            if (moveSpeedText != null)
                moveSpeedText.text = (data.MoveRange + GetStatBonus(StatInfo.MoveRange, data.UnitType)).ToString();

            if (turnSpeedText != null)
                turnSpeedText.text = data.Speed.ToString();

            if (criticalProbabilityText != null)
                criticalProbabilityText.text = $"{(data.CriticalProbability + GetStatBonus(StatInfo.CriticalProbability, data.UnitType)):F1}%";

            if (criticalDamageIncreaseText != null)
                criticalDamageIncreaseText.text = (data.CriticalDamageIncrease + GetStatBonus(StatInfo.CriticalIncreaseValue, data.UnitType)).ToString("F1");

            if (maxSkillCostText != null)
                maxSkillCostText.text = data.MaxManaCost.ToString();

            if (recoverySkillCostText != null)
                recoverySkillCostText.text = data.RecoveryManaCost.ToString();
        }

        private void RefreshHpBar(float prevValue, float nextValue)
        {
            if (_currentUnit?.Data == null)
                return;

            float maxHp = _currentUnit.Data.Maxhealth;

            if (hpText != null)
                hpText.text = $"{nextValue:F0} / {maxHp:F0}";
        }

        private void RefreshArtifactSlots()
        {
            if (artifactIcons == null || _currentUnit?.Data == null)
                return;

            UnitSO data = _currentUnit.Data;
            ArtifactButton prefabBtn = null;

            if (artifactButtonPoolingSO != null && artifactButtonPoolingSO.prefab != null)
                prefabBtn = artifactButtonPoolingSO.prefab.GetComponent<ArtifactButton>();

            for (int i = 0; i < artifactIcons.Count; i++)
            {
                if (artifactIcons[i] == null)
                    continue;

                SlotHoverClickTrigger trigger = artifactIcons[i].GetComponent<SlotHoverClickTrigger>();
                bool hasArtifact = data.EquippedArtifacts != null &&
                                   data.EquippedArtifacts.artifacts != null &&
                                   i < data.EquippedArtifacts.artifacts.Count &&
                                   data.EquippedArtifacts.artifacts[i] != null;

                if (hasArtifact)
                {
                    artifactIcons[i].sprite = data.EquippedArtifacts.artifacts[i].itemIcon;

                    if (artifactRarityIcons != null && i < artifactRarityIcons.Count && artifactRarityIcons[i] != null)
                    {
                        if (prefabBtn != null)
                            artifactRarityIcons[i].sprite = prefabBtn.GetRaritySprite(data.EquippedArtifacts.artifacts[i].rarity);

                        artifactRarityIcons[i].gameObject.SetActive(true);
                    }
                }
                else
                {
                    artifactIcons[i].sprite = emptyArtifactSlotSprite;

                    if (artifactRarityIcons != null && i < artifactRarityIcons.Count && artifactRarityIcons[i] != null)
                    {
                        artifactRarityIcons[i].sprite = null;
                        artifactRarityIcons[i].gameObject.SetActive(false);
                    }
                }

                if (trigger != null)
                    trigger.SetInteractable(true);
            }
        }

        private void RefreshSkillList()
        {
            ClearSkillButtons();

            if (_currentUnit?.Data == null || SkillSendManager.Instance == null || !CanSpawnSkillButtons())
                return;

            UnitSO data = _currentUnit.Data;
            IEnumerable<SkillSO> availableSkills = SkillSendManager.Instance.GetSkillList(data.UnitType) ?? Enumerable.Empty<SkillSO>();
            SkillSO[] equippedSkills = SkillSendManager.Instance.GetEquipSkills(data.UnitType) ?? System.Array.Empty<SkillSO>();

            foreach (SkillSO skillSO in availableSkills)
            {
                if (skillSO == null)
                    continue;

                CharacterSkillButton btn = _poolManager.Pop<CharacterSkillButton>(skillButtonPoolingSO);
                btn.transform.SetParent(ownSkillContainer, false);
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
            UnitSO data = _currentUnit?.Data;
            if (data == null)
                return;

            int currentCost = GetCurrentSkillLoadoutCost();
            int maxCost = data.LoadOutCost;
            float targetFillAmount = maxCost > 0 ? (float)currentCost / maxCost : 0f;
            bool shouldInstant = instant || !gameObject.activeInHierarchy || !IsVisible;

            if (shouldInstant)
            {
                SetSkillLoadoutImmediate(currentCost, maxCost, targetFillAmount);
                return;
            }

            AnimateSkillLoadoutFill(targetFillAmount);
            AnimateSkillLoadoutText(currentCost, maxCost);
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

        private IEnumerator CoSmoothSkillLoadoutText(int targetCost, int maxCost)
        {
            if (skillLoadoutText == null)
            {
                _textCoroutine = null;
                yield break;
            }

            int startCost = _hasSkillLoadoutValue ? _displayedSkillLoadoutCost : targetCost;
            int stepCount = Mathf.Abs(targetCost - startCost);

            if (stepCount <= 0 || fillAnimationDuration <= 0f)
            {
                SetSkillLoadoutText(targetCost, maxCost);
                _textCoroutine = null;
                yield break;
            }

            int direction = targetCost > startCost ? 1 : -1;
            int currentCost = startCost;
            float stepInterval = Mathf.Max(0.04f, fillAnimationDuration / stepCount);

            SetSkillLoadoutText(currentCost, maxCost);

            while (currentCost != targetCost)
            {
                float elapsedTime = 0f;

                while (elapsedTime < stepInterval)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                currentCost += direction;
                SetSkillLoadoutText(currentCost, maxCost);
            }

            _textCoroutine = null;
        }

        private void SetSkillLoadoutImmediate(int currentCost, int maxCost, float fillAmount)
        {
            StopSkillLoadoutAnimation();

            if (skillLoadoutFillImage != null)
                skillLoadoutFillImage.fillAmount = fillAmount;

            SetSkillLoadoutText(currentCost, maxCost);
        }

        private void AnimateSkillLoadoutFill(float targetFillAmount)
        {
            StopFillCoroutine();

            if (skillLoadoutFillImage == null)
                return;

            _fillCoroutine = StartCoroutine(CoSmoothFill(targetFillAmount));
        }

        private void AnimateSkillLoadoutText(int targetCost, int maxCost)
        {
            StopTextCoroutine();

            if (skillLoadoutText == null)
            {
                _displayedSkillLoadoutCost = targetCost;
                _hasSkillLoadoutValue = true;
                return;
            }

            _textCoroutine = StartCoroutine(CoSmoothSkillLoadoutText(targetCost, maxCost));
        }

        private void SetSkillLoadoutText(int currentCost, int maxCost)
        {
            if (skillLoadoutText != null)
                skillLoadoutText.text = $"{currentCost} / {maxCost}";

            _displayedSkillLoadoutCost = currentCost;
            _hasSkillLoadoutValue = true;
        }

        private int GetCurrentSkillLoadoutCost()
        {
            UnitSO data = _currentUnit?.Data;
            if (SkillSendManager.Instance == null || data == null)
                return 0;

            int totalCost = 0;
            SkillSO[] equippedSkills = SkillSendManager.Instance.GetEquipSkills(data.UnitType) ?? System.Array.Empty<SkillSO>();

            foreach (SkillSO skill in equippedSkills)
                if (skill != null)
                    totalCost += skill.SkillValue;

            return totalCost;
        }

        private void HandleSkillEquipped(SkillEquipEvent evt)
        {
            UnitSO data = _currentUnit?.Data;
            if (data == null || evt.Skill == null || evt.Skill.unitType != data.UnitType)
                return;

            if (SkillSendManager.Instance == null)
                return;

            SkillSO[] equippedSkills = SkillSendManager.Instance.GetEquipSkills(data.UnitType) ?? System.Array.Empty<SkillSO>();

            if (equippedSkills.Contains(evt.Skill))
                return;

            if (equippedSkills.Length >= 4)
            {
                Bus<WarningUIEvent>.Raise(new WarningUIEvent("스킬은 최대 4개까지 장착할 수 있습니다."));
                return;
            }

            int currentCost = GetCurrentSkillLoadoutCost();
            if (currentCost + evt.Skill.SkillValue > data.LoadOutCost)
            {
                Bus<WarningUIEvent>.Raise(new WarningUIEvent("스킬 코스트 총량을 초과하여 장착할 수 없습니다."));
                return;
            }

            if (data.SkillStorage != null && !data.SkillStorage.skills.Contains(evt.Skill))
                data.SkillStorage.skills.Add(evt.Skill);

            SkillSendManager.Instance.SyncEquippedSkills(data);
            NotifyInventoryChanged();
        }

        private void HandleSkillUnequipped(SkillUnequipEvent evt)
        {
            UnitSO data = _currentUnit?.Data;
            if (data == null || evt.Skill == null || evt.Skill.unitType != data.UnitType)
                return;

            data.SkillStorage?.skills.Remove(evt.Skill);

            if (SkillSendManager.Instance != null)
                SkillSendManager.Instance.SyncEquippedSkills(data);

            NotifyInventoryChanged();
        }

        private void NotifyInventoryChanged()
        {
            if (_owner != null)
            {
                _owner.RefreshViewsAfterInventoryChanged();
                return;
            }

            if (IsVisible)
                RefreshView();
        }

        private void ClearSkillButtons()
        {
            foreach (CharacterSkillButton btn in _activeSkillButtons)
                if (btn != null)
                    btn.ReturnToPool();

            _activeSkillButtons.Clear();
        }

        private bool CanSpawnSkillButtons()
        {
            ResolvePoolManager();

            return _poolManager != null &&
                   skillButtonPoolingSO != null &&
                   ownSkillContainer != null;
        }

        private void ResolvePoolManager()
        {
            if (_poolManager == null)
                _poolManager = FindFirstObjectByType<PoolManagerMono>();
        }

        private RectTransform GetContainer()
        {
            if (container == null)
                container = transform as RectTransform;

            return container;
        }

        private void UnsubscribeHpEvent()
        {
            if (_currentUnit?.CurrentHp != null)
                _currentUnit.CurrentHp.OnValueChanged -= RefreshHpBar;
        }

        private void StopFillCoroutine()
        {
            if (_fillCoroutine == null)
                return;

            StopCoroutine(_fillCoroutine);
            _fillCoroutine = null;
        }

        private void StopTextCoroutine()
        {
            if (_textCoroutine == null)
                return;

            StopCoroutine(_textCoroutine);
            _textCoroutine = null;
        }

        private void StopSkillLoadoutAnimation()
        {
            StopFillCoroutine();
            StopTextCoroutine();
        }

        private void ResetSkillLoadoutAnimationState()
        {
            StopSkillLoadoutAnimation();
            _displayedSkillLoadoutCost = 0;
            _hasSkillLoadoutValue = false;
        }

        private static int GetStatBonus(StatInfo statInfo, UnitType unitType)
        {
            return InGameStatCompo.Instance != null
                ? InGameStatCompo.Instance.GetStatToInt(statInfo, unitType)
                : 0;
        }
    }
}

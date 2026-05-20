using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.Items;
using Code.SkillSystem;
using Code.UnitSystem;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class CharacterStatPanel : MonoBehaviour
    {
        [Header("Legacy Panel Binding")]
        [SerializeField] private string id = "StatPanel";
        [SerializeField] private RectTransform container;

        [Header("Pool Settings")]
        [SerializeField] private PoolingItemSO artifactButtonPoolingSO;
        [SerializeField] private PoolingItemSO skillButtonPoolingSO;

        [Header("Equipped Items")]
        [SerializeField] private List<Image> skillIcons;
        [SerializeField] private Sprite emptySkillSlotSprite;
        [SerializeField] private List<Image> artifactIcons;
        [SerializeField] private List<Image> artifactRarityIcons;
        [SerializeField] private Sprite emptyArtifactSlotSprite;

        [Header("HP Bar")]
        [SerializeField] private Image hpBarFill;
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("Stat & Info")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI classText;

        [SerializeField] private TextMeshProUGUI maxHealthText;
        [SerializeField] private TextMeshProUGUI atkText;
        [SerializeField] private TextMeshProUGUI defText;
        [SerializeField] private TextMeshProUGUI moveSpeedText;
        [SerializeField] private TextMeshProUGUI turnSpeedText;
        [SerializeField] private TextMeshProUGUI criticalProbabilityText;
        [SerializeField] private TextMeshProUGUI criticalDamageIncreaseText;
        [SerializeField] private TextMeshProUGUI maxSkillCostText;
        [SerializeField] private TextMeshProUGUI recoverySkillCostText;

        private CharacterMainPanel _owner;
        private UnitState _currentUnit;
        private bool _triggersInitialized;

        public bool IsVisible => GetContainer().gameObject.activeSelf;

        private void Awake()
        {
            SetupSlotTriggers();
        }

        private void OnDestroy()
        {
            UnsubscribeHpEvent();
        }

        public void Initialize(CharacterMainPanel owner)
        {
            _owner = owner;
            SetupSlotTriggers();
            Hide();
        }

        public bool MatchesPanelId(string panelId)
            => string.Equals(id, panelId, System.StringComparison.OrdinalIgnoreCase);

        public void SetUnit(UnitState unit)
        {
            UnsubscribeHpEvent();
            _currentUnit = unit;

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
            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
        }

        public void RefreshView()
        {
            if (_currentUnit?.Data == null)
                return;

            RefreshInfoTexts();
            RefreshHpBar(0f, _currentUnit.CurrentHp != null ? _currentUnit.CurrentHp.Value : _currentUnit.Data.Maxhealth);
            RefreshSkillSlots();
            RefreshArtifactSlots();
        }

        private void SetupSlotTriggers()
        {
            if (_triggersInitialized)
                return;

            _triggersInitialized = true;

            Vector2 defaultSkillOffset = Vector2.zero;
            if (skillButtonPoolingSO != null && skillButtonPoolingSO.prefab != null)
            {
                CharacterSkillButton btn = skillButtonPoolingSO.prefab.GetComponent<CharacterSkillButton>();
                if (btn != null)
                    defaultSkillOffset = btn.EquippedPopupOffset;
            }

            Vector2 defaultArtifactOffset = Vector2.zero;
            if (artifactButtonPoolingSO != null && artifactButtonPoolingSO.prefab != null)
            {
                ArtifactButton btn = artifactButtonPoolingSO.prefab.GetComponent<ArtifactButton>();
                if (btn != null)
                    defaultArtifactOffset = btn.EquippedPopupOffset;
            }

            if (skillIcons != null)
            {
                for (int i = 0; i < skillIcons.Count; i++)
                {
                    if (skillIcons[i] == null)
                        continue;

                    int index = i;
                    SlotHoverClickTrigger trigger = skillIcons[i].GetComponent<SlotHoverClickTrigger>();
                    if (trigger == null)
                        trigger = skillIcons[i].gameObject.AddComponent<SlotHoverClickTrigger>();

                    trigger.useHoverVisuals = false;
                    trigger.OnClick = (_, _) => RequestEquipmentTab();
                    trigger.OnHoverEnter = (pivot, triggerOffset) =>
                    {
                        SkillSO skill = GetEquippedSkill(index);
                        if (skill == null)
                            return;

                        Vector2 finalOffset = triggerOffset != Vector2.zero ? triggerOffset : defaultSkillOffset;
                        Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(skill, pivot, finalOffset));
                    };
                    trigger.OnHoverExit = () => Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
                }
            }

            if (artifactIcons != null)
            {
                for (int i = 0; i < artifactIcons.Count; i++)
                {
                    if (artifactIcons[i] == null)
                        continue;

                    int index = i;
                    SlotHoverClickTrigger trigger = artifactIcons[i].GetComponent<SlotHoverClickTrigger>();
                    if (trigger == null)
                        trigger = artifactIcons[i].gameObject.AddComponent<SlotHoverClickTrigger>();

                    trigger.useHoverVisuals = false;
                    trigger.OnClick = (_, _) => RequestEquipmentTab();
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
        }

        private void RequestEquipmentTab()
        {
            Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));

            if (_owner != null)
            {
                _owner.ShowTab(CharacterUnitPanelTab.Equipment);
                return;
            }

            CharacterMainPanel.TryOpenTab("EquipPanel");
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

            if (hpBarFill != null)
                hpBarFill.fillAmount = maxHp > 0 ? nextValue / maxHp : 0f;
        }

        private void RefreshSkillSlots()
        {
            if (skillIcons == null)
                return;

            SkillSO[] equippedSkills = GetEquippedSkills();

            for (int i = 0; i < skillIcons.Count; i++)
            {
                if (skillIcons[i] == null)
                    continue;

                SlotHoverClickTrigger trigger = skillIcons[i].GetComponent<SlotHoverClickTrigger>();
                SkillSO skill = i < equippedSkills.Length ? equippedSkills[i] : null;

                skillIcons[i].sprite = skill != null ? skill.skillUIImage : emptySkillSlotSprite;

                if (trigger != null)
                    trigger.SetInteractable(true);
            }
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

        private SkillSO GetEquippedSkill(int index)
        {
            SkillSO[] equippedSkills = GetEquippedSkills();
            return index >= 0 && index < equippedSkills.Length ? equippedSkills[index] : null;
        }

        private SkillSO[] GetEquippedSkills()
        {
            if (_currentUnit?.Data == null || SkillSendManager.Instance == null)
                return System.Array.Empty<SkillSO>();

            return SkillSendManager.Instance.GetEquipSkills(_currentUnit.Data.UnitType) ?? System.Array.Empty<SkillSO>();
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

        private static int GetStatBonus(StatInfo statInfo, UnitType unitType)
        {
            return InGameStatCompo.Instance != null
                ? InGameStatCompo.Instance.GetStatToInt(statInfo, unitType)
                : 0;
        }
    }
}

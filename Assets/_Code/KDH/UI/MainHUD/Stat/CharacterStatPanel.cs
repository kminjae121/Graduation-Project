using System.Collections.Generic;
using _Code.UnitSystem;
using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.UnitSystem;
using Code.SkillSystem;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class CharacterStatPanel : Panel
    {
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

        private UnitState _currentUnit;

        public override void Awake()
        {
            base.Awake();
            Bus<CharacterInfoEvent>.Subscribe(HandleCharacterInfo);

            Vector2 defaultSkillOffset = Vector2.zero;
            if (skillButtonPoolingSO != null && skillButtonPoolingSO.prefab != null)
            {
                var btn = skillButtonPoolingSO.prefab.GetComponent<CharacterSkillButton>();
                if (btn != null) defaultSkillOffset = btn.EquippedPopupOffset;
            }

            Vector2 defaultArtifactOffset = Vector2.zero;
            if (artifactButtonPoolingSO != null && artifactButtonPoolingSO.prefab != null)
            {
                var btn = artifactButtonPoolingSO.prefab.GetComponent<ArtifactButton>();
                if (btn != null) defaultArtifactOffset = btn.EquippedPopupOffset;
            }

            for (int i = 0; i < skillIcons.Count; i++)
            {
                int index = i;
                var trigger = skillIcons[i].GetComponent<SlotHoverClickTrigger>();
                if (trigger == null)
                {
                    trigger = skillIcons[i].gameObject.AddComponent<SlotHoverClickTrigger>();
                }

                trigger.useHoverVisuals = false;
                
                trigger.OnClick = (pivot, triggerOffset) => 
                {
                    Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
                    OpenTargetPanel("EquipPanel");
                };
                trigger.OnHoverEnter = (pivot, triggerOffset) =>
                {
                    if (_currentUnit != null && SkillSendManager.Instance != null)
                    {
                        var equippedSkills = SkillSendManager.Instance.GetEquipSkills(_currentUnit.Data.UnitType);
                        if (index < equippedSkills.Length)
                        {
                            var skill = equippedSkills[index];
                            Vector2 finalOffset = triggerOffset != Vector2.zero ? triggerOffset : defaultSkillOffset;
                            Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(skill, pivot, finalOffset));
                        }
                    }
                };
                trigger.OnHoverExit = () => Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            }

            for (int i = 0; i < artifactIcons.Count; i++)
            {
                int index = i;
                var trigger = artifactIcons[i].GetComponent<SlotHoverClickTrigger>();
                if (trigger == null)
                {
                    trigger = artifactIcons[i].gameObject.AddComponent<SlotHoverClickTrigger>();
                }

                trigger.useHoverVisuals = false;
                
                trigger.OnClick = (pivot, triggerOffset) => 
                {
                    Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
                    OpenTargetPanel("EquipPanel");
                };
                trigger.OnHoverEnter = (pivot, triggerOffset) =>
                {
                    if (_currentUnit != null && _currentUnit.Data.EquippedArtifacts != null)
                    {
                        var artifacts = _currentUnit.Data.EquippedArtifacts.artifacts;
                        if (index < artifacts.Count)
                        {
                            var artifact = artifacts[index];
                            Vector2 finalOffset = triggerOffset != Vector2.zero ? triggerOffset : defaultArtifactOffset;
                            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(artifact, true, pivot, finalOffset, true));
                        }
                    }
                };
                trigger.OnHoverExit = () => Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Bus<CharacterInfoEvent>.Unsubscribe(HandleCharacterInfo);
            UnsubscribeHpEvent();
        }

        public override void Close()
        {
            base.Close();
            Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
        }

        private void OpenTargetPanel(string targetPanelId)
        {
            PanelManager.Close("StatPanel");
            PanelManager.Open(targetPanelId);
        }

        private void HandleCharacterInfo(CharacterInfoEvent evt)
        {
            UnsubscribeHpEvent();
            _currentUnit = evt.Unit;
            if (_currentUnit != null)
            {
                if (SkillSendManager.Instance != null)
                    SkillSendManager.Instance.SyncEquippedSkills(_currentUnit.Data);

                _currentUnit.CurrentHp.OnValueChanged += RefreshHpBar;
                if (IsOpen) RefreshAllUI();
            }
        }

        public override void Open()
        {
            base.Open();
            if (_currentUnit != null) RefreshAllUI();
        }

        private void UnsubscribeHpEvent()
        {
            if (_currentUnit != null) _currentUnit.CurrentHp.OnValueChanged -= RefreshHpBar;
        }

        private void RefreshAllUI()
        {
            RefreshInfoTexts();
            RefreshHpBar(0f, _currentUnit.CurrentHp.Value);
            RefreshSkillSlots();
            RefreshArtifactSlots();
        }

        private void RefreshInfoTexts()
        {
            var data = _currentUnit.Data;
            
            if (nameText != null) nameText.text = data.UnitName;
            if (classText != null) classText.text = data.UnitClass;
            
            if (maxHealthText != null) maxHealthText.text = (data.Maxhealth + InGameStatCompo.Instance.GetStatToInt(StatInfo.MaxHealth, data.UnitType)).ToString();
            if (atkText != null) atkText.text = (data.AttackDamage + InGameStatCompo.Instance.GetStatToInt(StatInfo.AtkDamage, data.UnitType)).ToString();
            if (defText != null) defText.text = (data.DefensivePower + InGameStatCompo.Instance.GetStatToInt(StatInfo.DefensivePower, data.UnitType)).ToString();
            if (moveSpeedText != null) moveSpeedText.text = (data.MoveRange + InGameStatCompo.Instance.GetStatToInt(StatInfo.MoveRange, data.UnitType)).ToString();
            if (turnSpeedText != null) turnSpeedText.text = data.Speed.ToString();
            
            if (criticalProbabilityText != null) criticalProbabilityText.text = $"{(data.CriticalProbability + InGameStatCompo.Instance.GetStatToInt(StatInfo.CriticalProbability, data.UnitType)):F1}%";
            if (criticalDamageIncreaseText != null) criticalDamageIncreaseText.text = (data.CriticalDamageIncrease + InGameStatCompo.Instance.GetStatToInt(StatInfo.CriticalIncreaseValue, data.UnitType)).ToString("F1");
            
            if (maxSkillCostText != null) maxSkillCostText.text = data.MaxManaCost.ToString();
            if (recoverySkillCostText != null) recoverySkillCostText.text = data.RecoveryManaCost.ToString();
        }

        private void RefreshHpBar(float prevValue, float nextValue)
        {
            float maxHp = _currentUnit.Data.Maxhealth;
            if (hpText != null) hpText.text = $"{nextValue:F0} / {maxHp:F0}";
            if (hpBarFill != null) hpBarFill.fillAmount = maxHp > 0 ? nextValue / maxHp : 0f;
        }

        private void RefreshSkillSlots()
        {
            var data = _currentUnit.Data;
            SkillSO[] equippedSkills = System.Array.Empty<SkillSO>();

            if (SkillSendManager.Instance != null && data != null)
                equippedSkills = SkillSendManager.Instance.GetEquipSkills(data.UnitType);
            
            for (int i = 0; i < skillIcons.Count; i++)
            {
                var trigger = skillIcons[i].GetComponent<SlotHoverClickTrigger>();
                bool hasSkill = i < equippedSkills.Length;

                if (hasSkill) skillIcons[i].sprite = equippedSkills[i].skillUIImage;
                else skillIcons[i].sprite = emptySkillSlotSprite;
                
                if (trigger != null) trigger.SetInteractable(hasSkill);
            }
        }

        private void RefreshArtifactSlots()
        {
            var data = _currentUnit.Data;
            ArtifactButton prefabBtn = null;

            if (artifactButtonPoolingSO != null && artifactButtonPoolingSO.prefab != null)
                prefabBtn = artifactButtonPoolingSO.prefab.GetComponent<ArtifactButton>();

            for (int i = 0; i < artifactIcons.Count; i++)
            {
                var trigger = artifactIcons[i].GetComponent<SlotHoverClickTrigger>();
                bool hasArtifact = data.EquippedArtifacts != null && i < data.EquippedArtifacts.artifacts.Count;

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
                
                if (trigger != null) trigger.SetInteractable(hasArtifact);
            }
        }
    }
}
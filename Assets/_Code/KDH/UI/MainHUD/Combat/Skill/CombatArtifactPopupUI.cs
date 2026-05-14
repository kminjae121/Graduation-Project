using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.Items;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CombatArtifactPopupUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private RectTransform popupRect;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI tierText;
        
        [Header("Stat Pooling Elements")]
        [SerializeField] private RectTransform statContentArea; 
        [SerializeField] private PoolingItemSO statSlotPoolingSO; 

        private PoolManagerMono _poolManager;
        private List<ArtifactStatSlotUI> _activeStatSlots = new List<ArtifactStatSlotUI>();
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            if (popupRect == null) popupRect = GetComponent<RectTransform>();
            
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            _poolManager = UnityEngine.Object.FindFirstObjectByType<PoolManagerMono>();
            
            HidePopup();
            Bus<CombatArtifactHoverEvent>.Subscribe(HandleArtifactHover);
        }

        private void OnDestroy()
        {
            Bus<CombatArtifactHoverEvent>.Unsubscribe(HandleArtifactHover);
        }

        private void HandleArtifactHover(CombatArtifactHoverEvent evt)
        {
            if (evt.IsShow && evt.Artifact != null) 
            {
                ShowPopup(evt.Artifact);

                if (evt.Pivot != null)
                {
                    popupRect.position = evt.Pivot.position;
                    popupRect.anchoredPosition += new Vector2(evt.Offset.x, evt.Offset.y);
                }
            }
            else 
            {
                HidePopup();
            }
        }

        private void ShowPopup(ItemSO artifact)
        {
            if (nameText != null) nameText.text = artifact.itemName;
            if (descriptionText != null) descriptionText.text = artifact.itemDesc;

            if (artifact is EquipmentItemSO equipSO)
            {
                if (tierText != null)
                {
                    tierText.text = equipSO.rarity.ToString();
                    SetTierTextColor(equipSO.rarity);
                }
                UpdateStatUI(equipSO);
            }
            else
            {
                if (tierText != null) tierText.text = "";
                ClearStatSlots();
            }

            if (popupRect != null) popupRect.gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private void UpdateStatUI(EquipmentItemSO equipSO)
        {
            ClearStatSlots();

            if (statContentArea == null || statSlotPoolingSO == null) return;

            if (equipSO.Stats != null && equipSO.Stats.Count > 0)
            {
                foreach (var stat in equipSO.Stats)
                {
                    var slot = _poolManager.Pop<ArtifactStatSlotUI>(statSlotPoolingSO);
                    if (slot != null)
                    {
                        slot.transform.SetParent(statContentArea);
                        slot.transform.localScale = Vector3.one;
                        slot.SetStat(GetKoreanStatName(stat.StatInfo.ToString()), stat.StatValue);
                        _activeStatSlots.Add(slot);
                    }
                }
            }
        }

        private void ClearStatSlots()
        {
            foreach (var slot in _activeStatSlots)
            {
                if (slot != null) slot.ReturnToPool();
            }
            _activeStatSlots.Clear();
        }

        private string GetKoreanStatName(string statInfoStr)
        {
            switch (statInfoStr)
            {
                case "MoveRange": return "이동 범위";
                case "AtkDamage": return "공격력";
                case "MaxHealth": return "체력";
                case "DefensivePower": return "방어력";
                case "AvoidProbability": return "회피율";
                case "CriticalProbability": return "치명타율";
                case "CriticalIncreaseValue": return "치명타배율";
                default: return statInfoStr; 
            }
        }

        private void SetTierTextColor(ArtifactRarity rarity)
        {
            if (tierText == null) return;
            switch (rarity)
            {
                case ArtifactRarity.Legendary: tierText.color = new Color(1f, 0.84f, 0f); break;
                case ArtifactRarity.Epic: tierText.color = new Color(0.63f, 0.13f, 0.94f); break;
                case ArtifactRarity.Rare: tierText.color = new Color(0f, 0.5f, 1f); break;
                case ArtifactRarity.Uncommon: tierText.color = Color.green; break;
                case ArtifactRarity.Common: default: tierText.color = Color.gray; break;
            }
        }

        private void HidePopup()
        {
            ClearStatSlots();
            if (popupRect != null) popupRect.gameObject.SetActive(false);
        }
    }
}
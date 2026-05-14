using System.Collections;
using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.Items;
using Code.UnitSystem.ArtifactSystem;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ArtifactEquipPopupUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI tierText;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button unequipButton;

        [Header("Stat Pooling Elements")]
        [SerializeField] private RectTransform statContentArea;
        [SerializeField] private PoolingItemSO statSlotPoolingSO;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private EquipmentItemSO _targetEquipmentItem;
        private bool _isCurrentlyEquipped;
        
        private bool _isJustOpened;
        private bool _isMouseOverPopup = false;
        private Coroutine _hideCoroutine;
        
        private PoolManagerMono _poolManager;
        private List<ArtifactStatSlotUI> _activeStatSlots = new List<ArtifactStatSlotUI>();

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _poolManager = UnityEngine.Object.FindFirstObjectByType<PoolManagerMono>();

            Bus<ArtifactPopupEvent>.Subscribe(HandlePopupEvent);
            
            equipButton.onClick.AddListener(HandleEquip);
            unequipButton.onClick.AddListener(HandleUnequip);

            ForceHide();
        }

        private void OnDestroy()
        {
            Bus<ArtifactPopupEvent>.Unsubscribe(HandlePopupEvent);
            equipButton.onClick.RemoveListener(HandleEquip);
            unequipButton.onClick.RemoveListener(HandleUnequip);
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;

            if (_isJustOpened)
            {
                _isJustOpened = false;
                return;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetMouseButtonDown(1))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, UnityEngine.Input.mousePosition, null))
                {
                    ForceHide();
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOverPopup = true;
            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOverPopup = false;
            StartHideRoutine();
        }

        private void HandlePopupEvent(ArtifactPopupEvent evt)
        {
            if (evt.EquipmentItem == null)
            {
                StartHideRoutine();
                return;
            }

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }

            _targetEquipmentItem = evt.EquipmentItem;
            _isCurrentlyEquipped = evt.IsEquipped;

            _canvasGroup.blocksRaycasts = !evt.IsReadOnly;

            nameText.text = _targetEquipmentItem.itemName;
            descriptionText.text = _targetEquipmentItem.itemDesc;

            if (tierText != null)
            {
                tierText.text = _targetEquipmentItem.rarity.ToString();
                SetTierTextColor(_targetEquipmentItem.rarity);
            }

            UpdateStatUI(); 

            equipButton.gameObject.SetActive(!_isCurrentlyEquipped && !evt.IsReadOnly);
            unequipButton.gameObject.SetActive(_isCurrentlyEquipped && !evt.IsReadOnly);

            if (evt.Pivot != null)
            {
                _rectTransform.position = evt.Pivot.position;
                _rectTransform.anchoredPosition += new Vector2(evt.Offset.x, evt.Offset.y);
            }

            gameObject.SetActive(true);
            _isJustOpened = true; 
            transform.SetAsLastSibling();

            if (evt.Pivot != null)
            {
                ClampToWindow();
            }
        }

        private void ClampToWindow()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Canvas.ForceUpdateCanvases();
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);
            
            Vector3[] popupCorners = new Vector3[4];
            _rectTransform.GetWorldCorners(popupCorners);
            
            Vector3 offset = Vector3.zero;

            if (popupCorners[0].x < canvasCorners[0].x)
                offset.x = canvasCorners[0].x - popupCorners[0].x;
            else if (popupCorners[2].x > canvasCorners[2].x)
                offset.x = canvasCorners[2].x - popupCorners[2].x;

            if (popupCorners[0].y < canvasCorners[0].y)
                offset.y = canvasCorners[0].y - popupCorners[0].y;
            else if (popupCorners[2].y > canvasCorners[2].y)
                offset.y = canvasCorners[2].y - popupCorners[2].y;

            _rectTransform.position += offset;
        }

        private void StartHideRoutine()
        {
            if (!gameObject.activeSelf) return;
            
            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
            _hideCoroutine = StartCoroutine(CoWaitAndHide());
        }

        private IEnumerator CoWaitAndHide()
        {
            yield return new WaitForSeconds(0.05f);
            
            if (!_isMouseOverPopup)
            {
                ForceHide();
            }
        }

        private void UpdateStatUI()
        {
            ClearStatSlots();

            if (statContentArea == null || statSlotPoolingSO == null) return;

            if (_targetEquipmentItem.Stats != null && _targetEquipmentItem.Stats.Count > 0)
            {
                foreach (var stat in _targetEquipmentItem.Stats)
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
            switch (rarity)
            {
                case ArtifactRarity.Legendary: tierText.color = new Color(1f, 0.84f, 0f); break;
                case ArtifactRarity.Epic: tierText.color = new Color(0.63f, 0.13f, 0.94f); break;
                case ArtifactRarity.Rare: tierText.color = new Color(0f, 0.5f, 1f); break;
                case ArtifactRarity.Uncommon: tierText.color = Color.green; break;
                case ArtifactRarity.Common: default: tierText.color = Color.gray; break;
            }
        }

        private void HandleEquip()
        {
            if (_targetEquipmentItem != null && !_isCurrentlyEquipped)
                Bus<ArtifactEquipEvent>.Raise(new ArtifactEquipEvent(_targetEquipmentItem));
                
            ForceHide(); 
        }

        private void HandleUnequip()
        {
            if (_targetEquipmentItem != null && _isCurrentlyEquipped)
                Bus<ArtifactUnequipEvent>.Raise(new ArtifactUnequipEvent(_targetEquipmentItem));
                
            ForceHide(); 
        }

        private void ForceHide()
        {
            _isMouseOverPopup = false;
            ClearStatSlots();

            if (!gameObject.activeSelf && _targetEquipmentItem == null) return;
            
            gameObject.SetActive(false);
            _targetEquipmentItem = null;
            
            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
        }
    }
}
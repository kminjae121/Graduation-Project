using System.Collections.Generic;
using System.Linq;
using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.Item;
using Code.Items;
using Code.UnitManaging;
using Code.UnitSystem;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class InventoryPanel : MonoBehaviour
    {
        [Header("Legacy Panel Binding")]
        [SerializeField] private string id = "InventoryPanel";
        [SerializeField] private RectTransform container;
        [SerializeField] private Button closeButton;

        [Header("Pool Settings")]
        [SerializeField] private PoolingItemSO artifactButtonPoolingSO;

        [Header("Unit Data")]
        [SerializeField] private UnitStorageSO unitStorageSO;

        [Header("Artifact Inventory Area")]
        [SerializeField] private Transform artifactInventoryTrm;
        [SerializeField] private TextMeshProUGUI artifactCountText;
        [SerializeField] private Button artifactSortButton;
        [SerializeField] private TextMeshProUGUI artifactSortText;
        [SerializeField] private int maxArtifactInventoryCapacity = 20;
        [SerializeField] private int maxArtifactEquipCount = 2;

        [Inject] private PoolManagerMono _poolManager;

        private MainPanel _owner;
        private UnitSO _unit;
        private readonly List<ArtifactButton> _activeArtifactButtons = new();
        private bool _isArtifactSortedByRarity;

        public bool IsVisible => GetContainer().gameObject.activeSelf;

        private void Awake()
        {
            ResolvePoolManager();

            if (artifactSortButton != null)
                artifactSortButton.onClick.AddListener(ToggleArtifactSort);

            Bus<ArtifactEquipEvent>.Subscribe(HandleArtifactEquip);
            Bus<ArtifactUnequipEvent>.Subscribe(HandleArtifactUnequip);

            WireCloseButton();
        }

        private void OnDestroy()
        {
            UnwireCloseButton();

            if (artifactSortButton != null)
                artifactSortButton.onClick.RemoveListener(ToggleArtifactSort);

            Bus<ArtifactEquipEvent>.Unsubscribe(HandleArtifactEquip);
            Bus<ArtifactUnequipEvent>.Unsubscribe(HandleArtifactUnequip);

            ClearArtifactButtons();
        }

        public void Initialize(MainPanel owner)
        {
            _owner = owner;
            ResolvePoolManager();
            WireCloseButton();
            Hide();
        }

        public bool MatchesPanelId(string panelId)
            => string.Equals(id, panelId, System.StringComparison.OrdinalIgnoreCase);

        public void SetUnit(UnitState unitState)
        {
            _unit = unitState?.Data;

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
            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
        }

        public void RefreshView()
        {
            if (_unit == null)
            {
                ClearArtifactButtons();
                return;
            }

            RefreshArtifactUI();
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
                btn.transform.SetParent(artifactInventoryTrm, false);
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
                Bus<WarningUIEvent>.Raise(new WarningUIEvent($"아티팩트는 최대 {maxArtifactEquipCount}개까지 장착할 수 있습니다."));
                return;
            }

            _unit.OwnArtifactStorage?.artifacts.Remove(evt.EquipmentItem);

            ItemStorage.Instance.SetItem(_unit.UnitType, evt.EquipmentItem);
            _unit.EquippedArtifacts.artifacts.Add(evt.EquipmentItem);

            NotifyInventoryChanged();
        }

        private void HandleArtifactUnequip(ArtifactUnequipEvent evt)
        {
            if (_unit == null || evt.EquipmentItem == null || _unit.EquippedArtifacts == null)
                return;

            if (!_unit.EquippedArtifacts.artifacts.Remove(evt.EquipmentItem))
                return;

            _unit.OwnArtifactStorage?.artifacts.Add(evt.EquipmentItem);

            ItemStorage.Instance.RemoveItem(_unit.UnitType, evt.EquipmentItem);
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

        private void ClearArtifactButtons()
        {
            foreach (ArtifactButton btn in _activeArtifactButtons)
                if (btn != null)
                    btn.ReturnToPool();

            _activeArtifactButtons.Clear();
        }

        private bool CanSpawnArtifactButtons()
        {
            ResolvePoolManager();

            return _poolManager != null &&
                   artifactButtonPoolingSO != null &&
                   artifactInventoryTrm != null;
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

        private void WireCloseButton()
        {
            if (closeButton == null)
                closeButton = FindCloseButton();

            if (closeButton == null)
                return;

            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        private void UnwireCloseButton()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Hide);
        }

        private Button FindCloseButton()
        {
            Transform searchRoot = container != null ? container : transform;
            return FindChildButtonByName(searchRoot, "CloseButton");
        }

        private static Button FindChildButtonByName(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
                return null;

            foreach (Transform child in parent)
            {
                if (child.name == childName && child.TryGetComponent(out Button button))
                    return button;

                Button found = FindChildButtonByName(child, childName);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}

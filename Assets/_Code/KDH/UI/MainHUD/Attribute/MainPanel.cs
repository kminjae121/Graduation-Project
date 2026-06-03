using Code.Core.Events.Bus;
using Code.UnitSystem;
using UnityEngine;

namespace Code.UI
{
    public enum UnitPanelTab
    {
        Attribute,
        Inventory
    }

    public class MainPanel : Panel
    {
        [Header("Default Settings")]
        [SerializeField] private string defaultOpenPanelId = "AttributePanel";

        [Header("Views")]
        [SerializeField] private AttributePanel attributePanel;
        [SerializeField] private InventoryPanel inventoryPanel;

        private UnitState _currentUnit;
        private UnitPanelTab _currentTab;
        private bool _viewsInitialized;

        public override void Awake()
        {
            base.Awake();

            ResolveViews();
            InitializeViews();
            Bus<CharacterInfoEvent>.Subscribe(HandleCharacterInfo);
        }

        protected override void OnDestroy()
        {
            Bus<CharacterInfoEvent>.Unsubscribe(HandleCharacterInfo);
            base.OnDestroy();
        }

        public override void Open()
        {
            OpenPanelShell();
            ShowTab(GetDefaultTab());
        }

        public override void Close()
        {
            if (_viewsInitialized)
            {
                attributePanel?.Hide();
                inventoryPanel?.Hide();
                ClearTransientPopups();
            }

            base.Close();
        }

        public void ShowTab(UnitPanelTab tab)
        {
            ResolveViews();
            InitializeViews();

            _currentTab = tab;

            if (tab == UnitPanelTab.Attribute)
            {
                BringViewToFront(attributePanel);
                attributePanel?.Show();
                return;
            }

            BringViewToFront(inventoryPanel);
            inventoryPanel?.Show();
        }

        public bool TryShowTabByPanelId(string panelId)
        {
            if (string.IsNullOrWhiteSpace(panelId))
                return false;

            ResolveViews();
            InitializeViews();

            if (attributePanel != null && attributePanel.MatchesPanelId(panelId))
            {
                ShowTab(UnitPanelTab.Attribute);
                return true;
            }

            if (inventoryPanel != null && inventoryPanel.MatchesPanelId(panelId))
            {
                ShowTab(UnitPanelTab.Inventory);
                return true;
            }

            return false;
        }

        public void RefreshViewsAfterInventoryChanged()
        {
            attributePanel?.RefreshView();

            if (inventoryPanel != null && inventoryPanel.IsVisible)
                inventoryPanel?.RefreshView();
        }

        public static bool TryOpenTab(string panelId)
        {
            MainPanel mainPanel = FindFirstObjectByType<MainPanel>(FindObjectsInactive.Include);

            if (mainPanel == null)
                return false;

            mainPanel.ResolveViews();
            mainPanel.InitializeViews();

            UnitPanelTab tab;
            if (mainPanel.attributePanel != null && mainPanel.attributePanel.MatchesPanelId(panelId))
                tab = UnitPanelTab.Attribute;
            else if (mainPanel.inventoryPanel != null && mainPanel.inventoryPanel.MatchesPanelId(panelId))
                tab = UnitPanelTab.Inventory;
            else
                return false;

            if (!mainPanel.IsOpen)
                mainPanel.OpenPanelShell();

            mainPanel.ShowTab(tab);
            return true;
        }

        public static bool TryCloseTab(string panelId)
        {
            MainPanel mainPanel = FindFirstObjectByType<MainPanel>(FindObjectsInactive.Include);

            if (mainPanel == null)
                return false;

            mainPanel.ResolveViews();
            mainPanel.InitializeViews();

            if (mainPanel.attributePanel != null && mainPanel.attributePanel.MatchesPanelId(panelId))
            {
                mainPanel.attributePanel.Hide();
                return true;
            }

            if (mainPanel.inventoryPanel != null && mainPanel.inventoryPanel.MatchesPanelId(panelId))
            {
                mainPanel.inventoryPanel.Hide();
                return true;
            }

            return false;
        }

        public static bool IsTabVisible(string panelId)
        {
            if (string.IsNullOrWhiteSpace(panelId))
                return false;

            MainPanel mainPanel = FindFirstObjectByType<MainPanel>(FindObjectsInactive.Include);

            if (mainPanel == null)
                return false;

            mainPanel.ResolveViews();

            if (mainPanel.attributePanel != null && mainPanel.attributePanel.MatchesPanelId(panelId))
                return mainPanel.attributePanel.IsVisible;

            if (mainPanel.inventoryPanel != null && mainPanel.inventoryPanel.MatchesPanelId(panelId))
                return mainPanel.inventoryPanel.IsVisible;

            return false;
        }

        private void OpenPanelShell()
        {
            base.Open();

            ResolveViews();
            InitializeViews();
            ApplyCurrentUnitToViews();
        }

        private void ResolveViews()
        {
            if (attributePanel == null)
                attributePanel = FindViewInCurrentScene<AttributePanel>();

            if (inventoryPanel == null)
                inventoryPanel = FindViewInCurrentScene<InventoryPanel>();
        }

        private T FindViewInCurrentScene<T>() where T : MonoBehaviour
        {
            T[] views = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (T view in views)
            {
                if (view != null && view.gameObject.scene == gameObject.scene)
                    return view;
            }

            return null;
        }

        private void InitializeViews()
        {
            if (_viewsInitialized)
                return;

            attributePanel?.Initialize(this);
            inventoryPanel?.Initialize(this);
            _viewsInitialized = true;
        }

        private void HandleCharacterInfo(CharacterInfoEvent evt)
        {
            _currentUnit = evt.Unit;
            ApplyCurrentUnitToViews();

            if (IsOpen)
                ShowTab(GetDefaultTab());
        }

        private void ApplyCurrentUnitToViews()
        {
            attributePanel?.SetUnit(_currentUnit);
            inventoryPanel?.SetUnit(_currentUnit);
        }

        private UnitPanelTab GetDefaultTab()
        {
            ResolveViews();

            if (inventoryPanel != null && inventoryPanel.MatchesPanelId(defaultOpenPanelId))
                return UnitPanelTab.Inventory;

            return UnitPanelTab.Attribute;
        }

        private static void BringViewToFront(MonoBehaviour view)
        {
            if (view != null)
                view.transform.SetAsLastSibling();
        }

        private static void ClearTransientPopups()
        {
            Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            Bus<SkillEquipPopupEvent>.Raise(new SkillEquipPopupEvent(null, false, null));
            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
        }
    }
}

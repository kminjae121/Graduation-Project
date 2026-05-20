using Code.Core.Events.Bus;
using Code.UnitSystem;
using UnityEngine;

namespace Code.UI
{
    public enum UnitPanelTab
    {
        Stat,
        Inventory
    }

    public class MainPanel : Panel
    {
        [Header("Default Settings")]
        [SerializeField] private string defaultOpenPanelId = "StatPanel";

        [Header("Views")]
        [SerializeField] private StatPanel statPanel;
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
            base.Open();

            ResolveViews();
            InitializeViews();
            ApplyCurrentUnitToViews();
            ShowTab(GetDefaultTab());
        }

        public override void Close()
        {
            if (_viewsInitialized)
            {
                statPanel?.Hide();
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

            if (tab == UnitPanelTab.Stat)
            {
                inventoryPanel?.Hide();
                BringViewToFront(statPanel);
                statPanel?.Show();
                return;
            }

            statPanel?.Hide();
            BringViewToFront(inventoryPanel);
            inventoryPanel?.Show();
        }

        public bool TryShowTabByPanelId(string panelId)
        {
            if (string.IsNullOrWhiteSpace(panelId))
                return false;

            ResolveViews();
            InitializeViews();

            if (statPanel != null && statPanel.MatchesPanelId(panelId))
            {
                ShowTab(UnitPanelTab.Stat);
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
            statPanel?.RefreshView();

            if (_currentTab == UnitPanelTab.Inventory)
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
            if (mainPanel.statPanel != null && mainPanel.statPanel.MatchesPanelId(panelId))
                tab = UnitPanelTab.Stat;
            else if (mainPanel.inventoryPanel != null && mainPanel.inventoryPanel.MatchesPanelId(panelId))
                tab = UnitPanelTab.Inventory;
            else
                return false;

            if (!mainPanel.IsOpen)
                mainPanel.Open();

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

            if (mainPanel.statPanel != null && mainPanel.statPanel.MatchesPanelId(panelId))
            {
                mainPanel.statPanel.Hide();
                return true;
            }

            if (mainPanel.inventoryPanel != null && mainPanel.inventoryPanel.MatchesPanelId(panelId))
            {
                mainPanel.inventoryPanel.Hide();
                return true;
            }

            return false;
        }

        private void ResolveViews()
        {
            if (statPanel == null)
                statPanel = FindViewInCurrentScene<StatPanel>();

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

            statPanel?.Initialize(this);
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
            statPanel?.SetUnit(_currentUnit);
            inventoryPanel?.SetUnit(_currentUnit);
        }

        private UnitPanelTab GetDefaultTab()
        {
            ResolveViews();

            if (inventoryPanel != null && inventoryPanel.MatchesPanelId(defaultOpenPanelId))
                return UnitPanelTab.Inventory;

            return UnitPanelTab.Stat;
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

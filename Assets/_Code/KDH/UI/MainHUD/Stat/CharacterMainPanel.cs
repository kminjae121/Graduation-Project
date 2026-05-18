using Code.Core.Events.Bus;
using Code.UnitSystem;
using UnityEngine;

namespace Code.UI
{
    public enum CharacterUnitPanelTab
    {
        Stat,
        Equipment
    }

    public class CharacterMainPanel : Panel
    {
        [Header("Default Settings")]
        [SerializeField] private string defaultOpenPanelId = "StatPanel";

        [Header("Views")]
        [SerializeField] private CharacterStatPanel statPanel;
        [SerializeField] private CharacterEquipmentPanel equipmentPanel;

        private UnitState _currentUnit;
        private CharacterUnitPanelTab _currentTab;
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
                equipmentPanel?.Hide();
                ClearTransientPopups();
            }

            base.Close();
        }

        public void ShowTab(CharacterUnitPanelTab tab)
        {
            ResolveViews();
            InitializeViews();

            _currentTab = tab;

            if (tab == CharacterUnitPanelTab.Stat)
            {
                equipmentPanel?.Hide();
                statPanel?.Show();
                return;
            }

            statPanel?.Hide();
            equipmentPanel?.Show();
        }

        public bool TryShowTabByPanelId(string panelId)
        {
            if (string.IsNullOrWhiteSpace(panelId))
                return false;

            ResolveViews();
            InitializeViews();

            if (statPanel != null && statPanel.MatchesPanelId(panelId))
            {
                ShowTab(CharacterUnitPanelTab.Stat);
                return true;
            }

            if (equipmentPanel != null && equipmentPanel.MatchesPanelId(panelId))
            {
                ShowTab(CharacterUnitPanelTab.Equipment);
                return true;
            }

            return false;
        }

        public void RefreshViewsAfterEquipmentChanged()
        {
            statPanel?.RefreshView();

            if (_currentTab == CharacterUnitPanelTab.Equipment)
                equipmentPanel?.RefreshView();
        }

        public static bool TryOpenTab(string panelId)
        {
            CharacterMainPanel mainPanel = FindFirstObjectByType<CharacterMainPanel>(FindObjectsInactive.Include);

            if (mainPanel == null)
                return false;

            if (!mainPanel.IsOpen)
                mainPanel.Open();

            return mainPanel.TryShowTabByPanelId(panelId);
        }

        public static bool TryCloseTab(string panelId)
        {
            CharacterMainPanel mainPanel = FindFirstObjectByType<CharacterMainPanel>(FindObjectsInactive.Include);

            if (mainPanel == null)
                return false;

            mainPanel.ResolveViews();
            mainPanel.InitializeViews();

            if (mainPanel.statPanel != null && mainPanel.statPanel.MatchesPanelId(panelId))
            {
                mainPanel.statPanel.Hide();
                return true;
            }

            if (mainPanel.equipmentPanel != null && mainPanel.equipmentPanel.MatchesPanelId(panelId))
            {
                mainPanel.equipmentPanel.Hide();
                return true;
            }

            return false;
        }

        private void ResolveViews()
        {
            if (statPanel == null)
                statPanel = FindFirstObjectByType<CharacterStatPanel>(FindObjectsInactive.Include);

            if (equipmentPanel == null)
                equipmentPanel = FindFirstObjectByType<CharacterEquipmentPanel>(FindObjectsInactive.Include);
        }

        private void InitializeViews()
        {
            if (_viewsInitialized)
                return;

            statPanel?.Initialize(this);
            equipmentPanel?.Initialize(this);
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
            equipmentPanel?.SetUnit(_currentUnit);
        }

        private CharacterUnitPanelTab GetDefaultTab()
        {
            ResolveViews();

            if (equipmentPanel != null && equipmentPanel.MatchesPanelId(defaultOpenPanelId))
                return CharacterUnitPanelTab.Equipment;

            return CharacterUnitPanelTab.Stat;
        }

        private static void ClearTransientPopups()
        {
            Bus<SkillUIHoverEvent>.Raise(new SkillUIHoverEvent(null, null));
            Bus<ArtifactPopupEvent>.Raise(new ArtifactPopupEvent(null, false, null));
        }
    }
}

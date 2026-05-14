using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.UI.SkillTreeUI;
using Code.UnitSystem.Upgrade;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class CharacterUpgradePanel : Panel
    {
        [Header("Tree Settings")]
        [SerializeField] private PoolingItemSO nodeButtonPoolSO;
        [SerializeField] private Transform treeContainer;
        [SerializeField] private List<UpgradeNodeSO> TreeData;

        [Header("Detail Settings")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statInfoText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button upgradeButton;
        private INode thisSkillNode;
        
        [Inject] private PoolManagerMono _poolManager;
        
        private List<UpgradeNodeButton> _activeNodes = new();
        private UpgradeNodeSO _selectedNode;


        public override void Awake()
        {
            base.Awake();
            
            if (_poolManager == null)
            {
                _poolManager = FindFirstObjectByType<PoolManagerMono>();
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(HandleUpgradeClick);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(HandleUpgradeClick);
            }
        }

        public override void Open()
        {
            base.Open();
            RefreshTree();
            ClearDetailView();
        }

        private void RefreshTree()
        {
            if (TreeData == null || TreeData.Count == 0)
            {
                Debug.LogWarning("트리 데이터가 존재하지 않습니다.");
                return;
            }

            if (_poolManager == null)
            {
                _poolManager = FindFirstObjectByType<PoolManagerMono>();
                if (_poolManager == null)
                {
                    Debug.LogError("풀 매니저를 찾을 수 없습니다.");
                    return;
                }
            }

            if (nodeButtonPoolSO == null)
            {
                Debug.LogError("노드 버튼 풀링 데이터가 할당되지 않았습니다.");
                return;
            }

            if (treeContainer == null)
            {
                Debug.LogError("트리 컨테이너가 할당되지 않았습니다.");
                return;
            }

            foreach (var node in _activeNodes) node.ReturnToPool();
            _activeNodes.Clear();


            for (int i = 0; i < TreeData.Count; i++)
            {
                var btn = _poolManager.Pop<UpgradeNodeButton>(nodeButtonPoolSO);
                btn.transform.SetParent(treeContainer);
                btn.transform.SetAsLastSibling();
                btn.transform.localScale = Vector3.one;
                btn.SetData(TreeData[i], OnNodeSelected);
                
                if (i == 0)
                    thisSkillNode = btn.GetComponent<INode>();
                
                _activeNodes.Add(btn);
            }

            //foreach (var data in TreeData)
            //{
            //    var btn = _poolManager.Pop<UpgradeNodeButton>(nodeButtonPoolSO);
            //    btn.transform.SetParent(treeContainer);
            //    btn.transform.SetAsLastSibling();
            //    btn.transform.localScale = Vector3.one;
            //    btn.SetData(data, OnNodeSelected);
            //    
            //    if (data.isUnlocked)
            //        thisSkillNode = btn.GetComponent<INode>();
            //    
            //    _activeNodes.Add(btn);
            //}
        }

        private void OnNodeSelected(UpgradeNodeSO nodeData)
        {
            _selectedNode = nodeData;

            if (iconImage != null)
            {
                iconImage.sprite = nodeData.icon;
                iconImage.color = Color.white;
                iconImage.gameObject.SetActive(true);
            }
            
            if (nameText != null) nameText.text = nodeData.upgradeName;
            if (descriptionText != null) descriptionText.text = nodeData.description;
            if (statInfoText != null) statInfoText.text = $"{nodeData.statOrSkillInfo}";
            if (costText != null) costText.text = $"{nodeData.cost}";
            
            if (upgradeButton != null) upgradeButton.interactable = !nodeData.isUnlocked; 
        }

        private void ClearDetailView()
        {
            _selectedNode = null;
            
            if (iconImage != null)
                iconImage.gameObject.SetActive(false);
            
            if (nameText != null) nameText.text = "업그레이드 선택";
            if (descriptionText != null) descriptionText.text = "위 트리에서 업그레이드 항목을 선택해주세요.";
            if (statInfoText != null) statInfoText.text = "-";
            if (costText != null) costText.text = "-";
            if (upgradeButton != null) upgradeButton.interactable = false;
        }

        private void HandleUpgradeClick()
        {
            if (_selectedNode == null) return;
            
            thisSkillNode.UseNode();
            Bus<ShowMessageUIEvent>.Raise(new ShowMessageUIEvent($"[{_selectedNode.upgradeName}] 업그레이드 완료!"));
            
            _selectedNode.isUnlocked = true;
            
            RefreshTree(); 
            OnNodeSelected(_selectedNode); 
        }
    }
}
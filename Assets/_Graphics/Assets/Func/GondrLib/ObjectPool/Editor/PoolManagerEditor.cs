using System;
using System.Collections.Generic;
using System.IO;
using GondrLib.ObjectPool.Editor;
using GondrLib.ObjectPool.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PoolManagerEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset visualTreeAsset = default;
    [SerializeField] private PoolManagerSO poolManager = default;
    [SerializeField] private VisualTreeAsset itemAsset = default;

    private string _rootFolderPath;
    private Button _createBtn;
    private ScrollView _itemView;

    private List<PoolItemUI> _itemList;
    private PoolItemUI _selectedItem;

    private Editor _cachedEditor;
    private VisualElement _inspectorView;

    [MenuItem("Tools/PoolManager")]
    public static void ShowWindow()
    {
        PoolManagerEditor wnd = GetWindow<PoolManagerEditor>();
        wnd.titleContent = new GUIContent("PoolManagerEditor");
    }

    private void InitializeRootFolder()
    {
        MonoScript script = MonoScript.FromScriptableObject(this);
        string scriptPath = AssetDatabase.GetAssetPath(script);
        string dataPath = Application.dataPath;
        _rootFolderPath = Directory.GetParent( Path.GetDirectoryName(scriptPath)).FullName.Replace("\\", "/");
        
        if (_rootFolderPath.StartsWith(dataPath))
        {
            _rootFolderPath = "Assets" + _rootFolderPath.Substring(dataPath.Length);
        }
        
        if (poolManager == null)
        {
            string filePath = $"{_rootFolderPath}/PoolManager.asset";
            poolManager = AssetDatabase.LoadAssetAtPath<PoolManagerSO>(filePath);
            if (poolManager == null)
            {
                Debug.LogWarning("PoolManager so is not exist, create new one");
                poolManager = ScriptableObject.CreateInstance<PoolManagerSO>();
                AssetDatabase.CreateAsset(poolManager, filePath);
            }
        }
        //visualTreeAsset;
        //itemAsset
         visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{_rootFolderPath}/Editor/PoolManagerEditor.uxml");
         Debug.Assert(visualTreeAsset != null, "Visual tree asset is null");
         itemAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{_rootFolderPath}/Editor/PoolItemUI.uxml");
         Debug.Assert(itemAsset != null, "Item asset is null");
    }

    public void CreateGUI()
    {
        InitializeRootFolder();
        
        VisualElement root = rootVisualElement;
        visualTreeAsset.CloneTree(root);

        InitializeItems(root);

    }

    private void InitializeItems(VisualElement root)
    {
        _createBtn = root.Q<Button>("CreateBtn");
        _createBtn.clicked += HandleCreateItem;
        _itemView = root.Q<ScrollView>("ItemView");

        _itemView.Clear(); //기존에 그려줬던거 지워주고
        _itemList = new List<PoolItemUI>();

        _inspectorView = root.Q<VisualElement>("InspectorView");
        
        GeneratePoolingItems();
    }

    private void HandleCreateItem()
    {
        string itemName = Guid.NewGuid().ToString();
        PoolingItemSO newItemSO = ScriptableObject.CreateInstance<PoolingItemSO>();
        newItemSO.poolingName = itemName;

        if (Directory.Exists($"{_rootFolderPath}/Items") == false)
        {
            Directory.CreateDirectory($"{_rootFolderPath}/Items");
        }
        
        AssetDatabase.CreateAsset(newItemSO, $"{_rootFolderPath}/Items/{itemName}.asset");
        
        poolManager.itemList.Add(newItemSO);
        EditorUtility.SetDirty(poolManager);
        AssetDatabase.SaveAssets();
        
        GeneratePoolingItems();
    }

    private void GeneratePoolingItems()
    {
        _itemView.Clear();
        _itemList.Clear();
        _inspectorView.Clear(); 

        foreach (var item in poolManager.itemList)
        {
            var itemTemplate = itemAsset.Instantiate();
            PoolItemUI itemUI = new PoolItemUI(itemTemplate, item);
            
            _itemView.Add(itemTemplate);
            _itemList.Add(itemUI);

            itemUI.Name = item.poolingName;
            
            if(_selectedItem != null && _selectedItem.poolItem == item)
            {
                itemUI.IsActive = true;
                _selectedItem = itemUI;
                //여기서 만약 선택되어있을경우에는 인스펙터 새로그리는것도 해야한다.
            }

            itemUI.OnSelectEvent += HandleSelectEvent;
            itemUI.OnDeleteEvent += HandleDeleteEvent;
        }
    }
    
    private void HandleSelectEvent(PoolItemUI target)
    {
        if (_selectedItem != null)
            _selectedItem.IsActive = false;
        _selectedItem = target;
        _selectedItem.IsActive = true;

        DrawInspector();
    }

    private void DrawInspector()
    {
        _inspectorView.Clear();
        UnityEditor.Editor.CreateCachedEditor(_selectedItem.poolItem, null, ref _cachedEditor);
        VisualElement inspector = _cachedEditor.CreateInspectorGUI();

        SerializedObject serializedObject = new SerializedObject(_selectedItem.poolItem);
        inspector.Bind(serializedObject);
        inspector.TrackSerializedObjectValue(serializedObject, so =>
        {
            _selectedItem.Name = so.FindProperty("poolingName").stringValue;
        });
        _inspectorView.Add(inspector);
    }

    private void HandleDeleteEvent(PoolItemUI target)
    {
        if (EditorUtility.DisplayDialog("Warning", 
                "Are you sure to delete this item?", "OK", "Cancel") == false)
        {
            return;
        }
        
        poolManager.itemList.Remove(target.poolItem); //눌린녀석을 리스트에서 제거
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(target.poolItem)); //에셋 삭제
        EditorUtility.SetDirty(poolManager);
        AssetDatabase.SaveAssets();

        if (target == _selectedItem)
        {
            _selectedItem = null; //선택된녀석이 삭제되면 null로 초기화
        }
        
        GeneratePoolingItems();
    }
}

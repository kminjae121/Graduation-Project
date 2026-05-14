using System.IO;
using GondrLib.ObjectPool.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GondrLib.ObjectPool.Editor
{
    [CustomEditor(typeof(PoolingItemSO))]
    public class PoolingItemSOEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset = default;
        
        public override VisualElement CreateInspectorGUI()
        {
            CheckVisualTreeAsset();
            VisualElement root = new VisualElement();
            visualTreeAsset.CloneTree(root);
            
            TextField nameField = root.Q<TextField>("PoolingNameField");
            nameField.RegisterValueChangedCallback(HandleAssetNameChange);

            return root;
        }

        private void CheckVisualTreeAsset()
        {
            if (visualTreeAsset == null)
            {
                MonoScript script = MonoScript.FromScriptableObject(this);
                string scriptPath = AssetDatabase.GetAssetPath(script);
                string path = Path.GetDirectoryName(scriptPath).Replace("\\", "/") + "/PoolingItemSOeditor.uxml";
                visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            }
        }

        private void HandleAssetNameChange(ChangeEvent<string> evt)
        {
            if (string.IsNullOrEmpty(evt.newValue))
            {
                EditorUtility.DisplayDialog("Error", "Name cannot be empty", "OK");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(target); //현재 타겟의 경로를 알아낸다.
            string newName = $"{evt.newValue}"; //새로운 이름을 기록
            string message = AssetDatabase.RenameAsset(assetPath, newName); //이름을 바꾼다. 에러가 없다면 공백 반환

            if (string.IsNullOrEmpty(message))
            {
                target.name = newName;
            }
            else
            {
                ((TextField)evt.target).SetValueWithoutNotify(evt.previousValue);
                EditorUtility.DisplayDialog("Error", message, "OK");
            }
        }
    }
}
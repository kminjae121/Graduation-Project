#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Code.Map.Editor
{
    [CustomEditor(typeof(GridMap))]
    public class GridMapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GridMap gridMap = (GridMap)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Map"))
            {
                gridMap.GenerateMap();
                EditorUtility.SetDirty(gridMap);
            }
        }
    }
}
#endif
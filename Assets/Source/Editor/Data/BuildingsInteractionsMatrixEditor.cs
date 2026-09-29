using DB.Data;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DB.Editor.Data
{
    [CustomEditor(typeof(BuildingsInteractionsMatrix))]
    public class BuildingsInteractionsMatrixEditor : UnityEditor.Editor
    {
        private SerializedProperty _buildingsProperty;
        private SerializedProperty _valuesProperty;

        private readonly int _cellWidth = 70;
        private readonly int _labelWidth = 140;

        private GUIStyle _labelStyle;

        private void OnEnable()
        {
            _buildingsProperty = serializedObject.FindProperty("_buildings");
            _valuesProperty = serializedObject.FindProperty("_values");
        }

        public override void OnInspectorGUI()
        {
            _labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
            serializedObject.Update();
            DrawBuildingsSection();
            DrawMatrixSection();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBuildingsSection()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_buildingsProperty);
            bool changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                HandleBuildingsListChanged();
            }
        }

        private void DrawMatrixSection()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("v To build / Neighbour >", GUILayout.Width(_labelWidth));
            for (int i = 0; i < _buildingsProperty.arraySize; i++)
            {
                Object obj = _buildingsProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                GUILayout.Label(
                    obj != null ? obj.name : "<Empty>", 
                    GUILayout.Width(_cellWidth)
                );
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _buildingsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                Object obj = _buildingsProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                GUILayout.Label(
                    obj != null ? obj.name : "<Empty>", 
                    _labelStyle, 
                    GUILayout.Width(_labelWidth)
                );

                for (int j = 0; j < _buildingsProperty.arraySize; j++)
                {
                    DrawCell(i, j);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawCell(int i, int j)
        {
            SerializedProperty cellProperty = _valuesProperty.GetArrayElementAtIndex(i * _buildingsProperty.arraySize + j);
            EditorGUI.BeginChangeCheck();
            int newValue = EditorGUILayout.IntField(cellProperty.intValue, GUILayout.Width(_cellWidth));
            if (EditorGUI.EndChangeCheck())
            {
                cellProperty.intValue = newValue;
            }
        }

        private BuildingDefinition[] ReadBuildings()
        {
            BuildingDefinition[] buildings = new BuildingDefinition[_buildingsProperty.arraySize];
            for (int i = 0; i < _buildingsProperty.arraySize; i++)
            {
                buildings[i] = _buildingsProperty.GetArrayElementAtIndex(i).objectReferenceValue as BuildingDefinition;
            }
            return buildings;
        }

        private int[] ReadValues()
        {
            int[] values = new int[_valuesProperty.arraySize];
            for (int i = 0; i < _valuesProperty.arraySize; i++)
            {
                values[i] = _valuesProperty.GetArrayElementAtIndex(i).intValue;
            }
            return values;
        }

        private void HandleBuildingsListChanged()
        {
            _valuesProperty.ClearArray();
            _valuesProperty.arraySize = _buildingsProperty.arraySize * _buildingsProperty.arraySize;
            for (int i = 0; i < _valuesProperty.arraySize; i++)
            {
                _valuesProperty.GetArrayElementAtIndex(i).intValue = 0;
            }

            // @todo: Instead of clearing and reinitializing, we should check if there is a new row in _buildings (could be still empty),
            // or one of the existing rows has been changed etc. and then modify _values accordingly (change size, rewrite values etc.).
        }
    } 
}
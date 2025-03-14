using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Dialog))]
public class DialogEditor : Editor
{
    private SerializedProperty linesProperty;
    
    private void OnEnable()
    {
        linesProperty = serializedObject.FindProperty("lines");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Dialog Line Management", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Line", GUILayout.Height(30)))
        {
            if (linesProperty != null && linesProperty.isArray)
            {
                linesProperty.arraySize++;
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        GUI.enabled = linesProperty != null && linesProperty.isArray && linesProperty.arraySize > 0;
        if (GUILayout.Button("Remove Last Line", GUILayout.Height(30)))
        {
            if (linesProperty != null && linesProperty.isArray && linesProperty.arraySize > 0)
            {
                linesProperty.arraySize--;
                serializedObject.ApplyModifiedProperties();
            }
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
} 
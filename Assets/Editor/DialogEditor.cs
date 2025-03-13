using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Dialog))]
public class DialogEditor : Editor
{
    private SerializedProperty linesProperty;
    
    private void OnEnable()
    {
        // Get the lines property when the editor is enabled
        linesProperty = serializedObject.FindProperty("lines");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Just draw the default inspector first - this is the safest approach
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Dialog Line Management", EditorStyles.boldLabel);
        
        // Add a button section to add/remove lines
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Line", GUILayout.Height(30)))
        {
            // Only modify if we have a valid lines property
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
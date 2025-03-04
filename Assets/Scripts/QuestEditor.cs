#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Quest))]
public class QuestEditor : Editor
{
    private SerializedProperty questNameProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty isActiveProp;
    private SerializedProperty isCompletedProp;
    private SerializedProperty objectivesProp;

    private void OnEnable()
    {
        questNameProp = serializedObject.FindProperty("questName");
        descriptionProp = serializedObject.FindProperty("description");
        isActiveProp = serializedObject.FindProperty("_isActive");
        isCompletedProp = serializedObject.FindProperty("_isCompleted");
        objectivesProp = serializedObject.FindProperty("_objectives");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(questNameProp);
        EditorGUILayout.PropertyField(descriptionProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quest Status", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isActiveProp);
        EditorGUILayout.PropertyField(isCompletedProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quest Objectives", EditorStyles.boldLabel);

        if (objectivesProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("This quest has no objectives. Add at least one objective.", MessageType.Warning);
        }

        for (int i = 0; i < objectivesProp.arraySize; i++)
        {
            SerializedProperty objectiveProp = objectivesProp.GetArrayElementAtIndex(i);
            SerializedProperty descProp = objectiveProp.FindPropertyRelative("description");
            SerializedProperty completedProp = objectiveProp.FindPropertyRelative("isCompleted");

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"Objective {i + 1}", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(descProp, new GUIContent("Description"));
            EditorGUILayout.PropertyField(completedProp, new GUIContent("Completed"));

            if (GUILayout.Button("Remove Objective"))
            {
                objectivesProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add New Objective"))
        {
            objectivesProp.arraySize++;
            SerializedProperty newObj = objectivesProp.GetArrayElementAtIndex(objectivesProp.arraySize - 1);
            newObj.FindPropertyRelative("description").stringValue = "New Objective";
            newObj.FindPropertyRelative("isCompleted").boolValue = false;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
using UnityEngine;

public class QuestDebugger : MonoBehaviour
{
    public Quest questToDebug; // Assign your quest in the inspector
    
    void Start()
    {
        if (questToDebug != null)
        {
            Debug.Log($"=== QUEST DEBUG: {questToDebug.questName} ===");
            Debug.Log($"Objective count: {questToDebug.objectives.Count}");
            
            foreach (var obj in questToDebug.objectives)
            {
                Debug.Log($"Objective: {obj.description}, Completed: {obj.isCompleted}");
            }
        }
    }
} 
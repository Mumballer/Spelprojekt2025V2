using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questName;
    public string description;
    
    public List<QuestObjective> objectives = new List<QuestObjective>();
    
    [HideInInspector]
    public bool isActive;
    
    [HideInInspector]
    public bool isCompleted;
    
    public bool IsActive
    {
        get { return isActive; }
        set { isActive = value; }
    }
    
    public bool IsCompleted
    {
        get { return isCompleted; }
        set { isCompleted = value; }
    }
    
    public void Initialize()
    {
        isActive = false;
        isCompleted = false;
        
        foreach (QuestObjective objective in objectives)
        {
            objective.Initialize();
        }
    }
    
    public bool CheckCompletion()
    {
        if (isCompleted) return true;
        
        bool allCompleted = true;
        foreach (QuestObjective objective in objectives)
        {
            Debug.Log($"Checking objective: {objective.description}, Completed: {objective.isCompleted}");
            if (!objective.isCompleted)
            {
                allCompleted = false;
                break;
            }
        }
        
        if (allCompleted)
        {
            isCompleted = true;
            Debug.Log($"Quest {questName} is now complete!");
        }
        
        return isCompleted;
    }
    [Header("Quest Chaining")]
    [Tooltip("The next quest to automatically start when this one completes")]
    public Quest nextQuest;
    public void ResetObjectives()
    {
        if (objectives == null)
            return;

        foreach (var objective in objectives)
        {
            objective.isCompleted = false;
        }
    }
}
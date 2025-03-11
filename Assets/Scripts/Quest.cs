using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string title;
    public string description;
    
    public List<QuestObjective> objectives = new List<QuestObjective>();
    
    [HideInInspector]
    public bool isActive;
    
    [HideInInspector]
    public bool isCompleted;
    
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
        
        foreach (QuestObjective objective in objectives)
        {
            if (!objective.isCompleted)
                return false;
        }
        
        isCompleted = true;
        return true;
    }
}
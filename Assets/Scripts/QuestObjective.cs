using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectiveType
{
    Collect,
    Interact,
    GoToLocation,
    KillEnemy
    // Add more types as needed
}

[CreateAssetMenu(fileName = "New Objective", menuName = "Quests/Objective")]
public class QuestObjective : ScriptableObject
{
    public string description;
    public ObjectiveType type;
    
    // For collection quests
    public string itemID; // Identifier for what needs to be collected
    public int requiredAmount = 1;
    
    [HideInInspector]
    public int currentAmount;
    
    [HideInInspector]
    public bool isCompleted;
    
    public void Initialize()
    {
        currentAmount = 0;
        isCompleted = false;
    }
    
    public void UpdateProgress(int amount = 1)
    {
        currentAmount += amount;
        
        if (currentAmount >= requiredAmount && !isCompleted)
        {
            isCompleted = true;
            Debug.Log($"Objective completed: {description}");
        }
    }
}
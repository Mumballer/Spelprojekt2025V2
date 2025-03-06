using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    public string description;
    public bool isCompleted;

    public QuestObjective(string desc, bool completed = false)
    {
        description = desc;
        isCompleted = completed;
    }
}

public class Quest : MonoBehaviour
{
    [Header("Quest Info")]
    public string questName = "New Quest";
    public string description = "Quest description goes here.";

    [Header("Quest Objectives")]
    [SerializeField] private List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Quest Rewards")]
    public int experienceReward = 100;
    public int goldReward = 50;

    // Unique identifier for this quest
    [HideInInspector] public string QuestId;

    private void Awake()
    {
        // Generate a unique ID if not set
        if (string.IsNullOrEmpty(QuestId))
            QuestId = System.Guid.NewGuid().ToString();
    }

    // Public accessor for objectives
    public List<QuestObjective> Objectives
    {
        get { return objectives; }
    }

    // Quest completion status
    public bool IsCompleted
    {
        get
        {
            if (objectives == null || objectives.Count == 0)
                return false;

            foreach (QuestObjective objective in objectives)
            {
                if (!objective.isCompleted)
                    return false;
            }
            return true;
        }
    }

    // Check if the quest is active (has been accepted but not completed)
    public bool IsActive
    {
        get
        {
            return QuestManager.Instance != null && QuestManager.Instance.IsQuestActive(this);
        }
    }

    // Add a new objective to the quest
    public void AddObjective(string description)
    {
        objectives.Add(new QuestObjective(description));
    }

    // Complete a specific objective by index
    public void CompleteObjective(int index)
    {
        if (index >= 0 && index < objectives.Count)
        {
            objectives[index].isCompleted = true;
        }
    }

    // Add this public helper method for debugging
    public bool DebugCheckCompletion()
    {
        bool allDone = true;
        Debug.Log($"Debug checking quest completion for '{questName}':");

        if (objectives == null || objectives.Count == 0)
        {
            Debug.Log("  Quest has no objectives!");
            return false;
        }

        for (int i = 0; i < objectives.Count; i++)
        {
            bool done = objectives[i].isCompleted;
            Debug.Log($"  Objective {i}: {objectives[i].description} - Completed: {done}");
            if (!done) allDone = false;
        }

        Debug.Log($"  All objectives complete? {allDone}");
        Debug.Log($"  Property IsCompleted returns: {IsCompleted}");

        return allDone;
    }
}
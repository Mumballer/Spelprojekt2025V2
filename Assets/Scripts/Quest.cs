using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    [Header("Quest Info")]
    public string questName = "New Quest";
    [TextArea(3, 6)]
    public string description = "Quest description goes here.";

    [Header("Identification")]
    [SerializeField] private string questId = "";

    [Header("Objectives")]
    [SerializeField] private List<QuestObjective> objectives = new List<QuestObjective>();

    // Prevent multiple completion calls
    private bool isAlreadyCompleted = false;

    public List<QuestObjective> Objectives => objectives;
    public string QuestId => questId;

    // Automatically generate a Quest ID if none exists
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(questId))
        {
            questId = System.Guid.NewGuid().ToString();
        }
    }

    // Check if all objectives are completed
    public bool IsCompleted
    {
        get
        {
            if (objectives.Count == 0) return false;

            foreach (var objective in objectives)
            {
                if (!objective.isCompleted)
                    return false;
            }
            return true;
        }
    }

    // Quick way to check objective status by index
    public bool IsObjectiveCompleted(int index)
    {
        if (index < 0 || index >= objectives.Count)
            return false;

        return objectives[index].isCompleted;
    }

    // Reset quest state (useful for testing)
    public void ResetQuest()
    {
        foreach (var objective in objectives)
        {
            objective.isCompleted = false;
        }
        isAlreadyCompleted = false;
    }

    // Complete the quest (called when all objectives are done)
    public void CompleteQuest()
    {
        // Guard against multiple completions
        if (isAlreadyCompleted)
        {
            Debug.LogWarning($"Attempted to complete already completed quest: {questName} (ID: {questId})");
            return;
        }

        isAlreadyCompleted = true;
        Debug.Log($"Quest completed: {questName} (ID: {questId})");

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.NotifyQuestCompleted(this);
        }
    }

    // Complete a specific objective
    public void CompleteObjective(int index)
    {
        if (isAlreadyCompleted)
        {
            Debug.LogWarning($"Attempted to modify already completed quest: {questName} (ID: {questId})");
            return;
        }

        if (index < 0 || index >= objectives.Count)
            return;

        Debug.Log($"Completed objective {index} for quest: {questName} (ID: {questId})");

        objectives[index].isCompleted = true;

        // Check if all objectives are completed
        CheckQuestCompletion();
    }

    // Check if quest is complete and notify if so
    public void CheckQuestCompletion()
    {
        if (!isAlreadyCompleted && IsCompleted)
        {
            CompleteQuest();
        }
    }
}

// Quest objective structure
[Serializable]
public class QuestObjective
{
    [TextArea(1, 3)]
    public string description = "Objective description";
    public bool isCompleted = false;
}

// Extension methods for backward compatibility
public static class QuestExtensions
{
    public static bool IsActive(this Quest quest)
    {
        return QuestManager.Instance != null && QuestManager.Instance.IsQuestActive(quest);
    }

    public static void ActivateQuest(this Quest quest)
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest(quest);
        }
    }
}
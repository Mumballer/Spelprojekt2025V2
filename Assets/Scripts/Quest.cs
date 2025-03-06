using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityEditor.Progress;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    [Header("Quest Info")]
    public string questName;
    [TextArea(3, 10)]
    public string description;

    [Header("Objectives")]
    public List<QuestObjective> Objectives = new List<QuestObjective>();

    [Header("Rewards")]
    public int experienceReward;
    public int goldReward;
    public Item[] itemRewards;

    [Header("Follow-up Quests")]
    public Quest[] followUpQuests;
    public bool requiresManualAcceptance = true;

    [HideInInspector]
    public bool IsActive = false;
    [HideInInspector]
    public bool IsCompleted = false;

    public event Action OnQuestActivated;
    public event Action OnQuestCompleted;
    public event Action<int> OnObjectiveCompleted;

    [Serializable]
    public class QuestObjective
    {
        public string description;
        public bool isCompleted;
    }

    public void ActivateQuest()
    {
        IsActive = true;
        IsCompleted = false;

        // Reset objectives
        foreach (var objective in Objectives)
        {
            objective.isCompleted = false;
        }

        OnQuestActivated?.Invoke();
    }

    public void CompleteQuest()
    {
        IsActive = false;
        IsCompleted = true;

        // Mark all objectives as completed
        foreach (var objective in Objectives)
        {
            objective.isCompleted = true;
        }

        OnQuestCompleted?.Invoke();

        // Notify the quest manager
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.NotifyQuestCompleted(this);
        }
    }

    public bool AreAllObjectivesCompleted()
    {
        if (Objectives == null || Objectives.Count == 0)
            return true;

        foreach (var objective in Objectives)
        {
            if (!objective.isCompleted)
                return false;
        }

        return true;
    }

    public float GetCompletionPercentage()
    {
        if (Objectives == null || Objectives.Count == 0)
            return 0f;

        int completedCount = 0;

        foreach (var objective in Objectives)
        {
            if (objective.isCompleted)
                completedCount++;
        }

        return (float)completedCount / Objectives.Count;
    }
}
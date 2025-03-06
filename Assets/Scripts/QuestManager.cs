using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    // Events for quest state changes
    public event Action<Quest> OnQuestAdded;
    public event Action<Quest> OnQuestAccepted;
    public event Action<Quest> OnQuestAvailable;
    public event Action<Quest> OnQuestCompleted;
    public event Action<Quest> OnQuestRemoved;
    public event Action<Quest, int> OnObjectiveCompleted;

    // Quest lists
    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> availableQuests = new List<Quest>();
    private List<Quest> completedQuests = new List<Quest>();

    [Header("Debug Options")]
    [SerializeField] private bool enableDebugLogs = true;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        DebugLog("QuestManager initialized");
    }

    // Quest Management Methods

    public void AddQuest(Quest quest)
    {
        if (quest == null) return;

        DebugLog($"Adding quest '{quest.questName}' - ID: {quest.QuestId}");

        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            OnQuestAdded?.Invoke(quest);
            OnQuestAccepted?.Invoke(quest);
        }
    }

    public void AcceptQuest(Quest quest)
    {
        if (quest == null) return;

        DebugLog($"Accepting quest '{quest.questName}' - ID: {quest.QuestId}");

        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            availableQuests.Remove(quest); // Remove from available if it was there
            OnQuestAccepted?.Invoke(quest);
        }
    }

    public void MakeQuestAvailable(Quest quest)
    {
        if (quest == null) return;

        DebugLog($"Making quest available '{quest.questName}' - ID: {quest.QuestId}");

        if (!availableQuests.Contains(quest) && !activeQuests.Contains(quest) && !completedQuests.Contains(quest))
        {
            availableQuests.Add(quest);
            OnQuestAvailable?.Invoke(quest);
        }
    }

    public void CompleteQuest(Quest quest)
    {
        if (quest == null) return;

        DebugLog($"Completing ONLY quest '{quest.questName}' - ID: {quest.QuestId}");

        // Only affect THIS specific quest
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            availableQuests.Remove(quest); // Also remove from available if it's there
            completedQuests.Add(quest);
            OnQuestCompleted?.Invoke(quest);
        }
    }

    public void RemoveQuest(Quest quest)
    {
        if (quest == null) return;

        DebugLog($"Removing quest '{quest.questName}' - ID: {quest.QuestId}");

        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            OnQuestRemoved?.Invoke(quest);
        }
    }

    // Query Methods

    public bool IsQuestActive(Quest quest)
    {
        return quest != null && activeQuests.Contains(quest);
    }

    public bool IsQuestAvailable(Quest quest)
    {
        return quest != null && availableQuests.Contains(quest);
    }

    public bool IsQuestCompleted(Quest quest)
    {
        return quest != null && completedQuests.Contains(quest);
    }

    // Objective Methods

    public void CompleteObjective(Quest quest, int objectiveIndex)
    {
        if (quest == null || !activeQuests.Contains(quest)) return;

        DebugLog($"Completing objective {objectiveIndex} for quest '{quest.questName}' - ID: {quest.QuestId}");

        if (objectiveIndex >= 0 && objectiveIndex < quest.Objectives.Count)
        {
            quest.Objectives[objectiveIndex].isCompleted = true;
            OnObjectiveCompleted?.Invoke(quest, objectiveIndex);

            if (quest.IsCompleted)
            {
                DebugLog($"Auto-completing quest '{quest.questName}' as all objectives are done");
                CompleteQuest(quest);
            }
        }
    }

    // Convenience Methods

    public void NotifyQuestCompleted(Quest quest) => CompleteQuest(quest);

    // Getters

    public List<Quest> GetActiveQuests() => new List<Quest>(activeQuests);
    public List<Quest> GetCompletedQuests() => new List<Quest>(completedQuests);
    public List<Quest> GetAvailableQuests() => new List<Quest>(availableQuests);

    // Debug Helper

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] {message}");
        }
    }
}
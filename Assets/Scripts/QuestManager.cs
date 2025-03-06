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
    [SerializeField] private bool verboseQuestLogs = true; // Added for extra detailed logs

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

        // Check for duplicates before adding
        foreach (var existingQuest in activeQuests)
        {
            if (existingQuest.questName == quest.questName && existingQuest != quest)
            {
                Debug.LogWarning($"[QuestManager] Duplicate quest detected! '{quest.questName}' already exists in activeQuests.");
                // Still add it to match existing behavior, but warn about it
            }
        }

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

        // Check for duplicates before adding
        foreach (var existingQuest in activeQuests)
        {
            if (existingQuest.questName == quest.questName && existingQuest != quest)
            {
                Debug.LogWarning($"[QuestManager] Duplicate quest detected! '{quest.questName}' already exists in activeQuests.");
                // Still add it to match existing behavior, but warn about it
            }
        }

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

    private static Quest FindQuestByName(string questName)
    {
        Quest[] allQuests = FindObjectsOfType<Quest>();
        foreach (var quest in allQuests)
        {
            if (quest.questName == questName)
            {
                return quest;
            }
        }
        return null;
    }

    // Convenience Methods

    public void NotifyQuestCompleted(Quest quest) => CompleteQuest(quest);

    // Enhanced Getters with Debugging

    public List<Quest> GetActiveQuests()
    {
        Debug.Log($"[QuestManager] GetActiveQuests called, found {activeQuests.Count} active quests");

        // Create a filtered list without duplicates
        List<Quest> filteredQuests = new List<Quest>();
        HashSet<string> questNames = new HashSet<string>();

        foreach (Quest quest in activeQuests)
        {
            if (quest == null)
            {
                Debug.LogError("[QuestManager] Found NULL quest in activeQuests list!");
                continue;
            }

            if (verboseQuestLogs)
            {
                // Fix: call IsActive() and IsCompleted() as methods if they are methods
                Debug.Log($"[QuestManager] Checking quest: {quest.questName}, ID: {quest.QuestId}");
            }

            // Check for duplicate quest names
            if (questNames.Contains(quest.questName))
            {
                Debug.LogWarning($"[QuestManager] DUPLICATE DETECTED: Quest '{quest.questName}' appears multiple times in active quests!");

                // For debugging - check if it's the same object or a different one
                foreach (var existingQuest in filteredQuests)
                {
                    if (existingQuest.questName == quest.questName)
                    {
                        Debug.LogWarning($"[QuestManager] Comparing duplicate quests: Same object? {ReferenceEquals(existingQuest, quest)}, Existing ID: {existingQuest.QuestId}, This ID: {quest.QuestId}");
                    }
                }

                // Still add it to match existing behavior, but we've warned about it
                filteredQuests.Add(quest);
            }
            else
            {
                questNames.Add(quest.questName);
                filteredQuests.Add(quest);

                if (verboseQuestLogs)
                {
                    Debug.Log($"[QuestManager] Added active quest: {quest.questName}");
                }
            }
        }

        Debug.Log($"[QuestManager] Returning {filteredQuests.Count} active quests");

        // Return a new list to avoid external code modifying our internal list
        return new List<Quest>(activeQuests);
    }

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

    // Debug method to find and remove duplicate quests (call from a debug menu or button)
    public void DebugRemoveDuplicateQuests()
    {
        Debug.Log("[QuestManager] Starting duplicate quest cleanup...");

        Dictionary<string, Quest> uniqueQuestsByName = new Dictionary<string, Quest>();
        List<Quest> duplicatesToRemove = new List<Quest>();

        // Find duplicates
        foreach (var quest in activeQuests)
        {
            if (quest == null) continue;

            if (uniqueQuestsByName.ContainsKey(quest.questName))
            {
                Debug.LogWarning($"[QuestManager] Found duplicate quest: {quest.questName}");
                duplicatesToRemove.Add(quest);
            }
            else
            {
                uniqueQuestsByName[quest.questName] = quest;
            }
        }

        // Remove duplicates
        foreach (var dupe in duplicatesToRemove)
        {
            Debug.Log($"[QuestManager] Removing duplicate quest: {dupe.questName}");
            activeQuests.Remove(dupe);
        }

        Debug.Log($"[QuestManager] Removed {duplicatesToRemove.Count} duplicate quests");
    }
}
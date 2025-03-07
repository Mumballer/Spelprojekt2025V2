using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private Transform questContainer;
    [SerializeField] private GameObject noQuestsMessage;
    [SerializeField] private bool autoRemoveCompletedQuests = true; // Option to auto-remove quests
    [SerializeField] private float completionDisplayTime = 2f; // Time to show completed quests before removing

    private Dictionary<Quest, QuestEntryUI> questEntries = new Dictionary<Quest, QuestEntryUI>();
    private HashSet<Quest> pendingRemoval = new HashSet<Quest>(); // Track quests being removed

    [Header("Debug Options")]
    [SerializeField] private bool enableDebugLogging = true;

    void Start()
    {
        RefreshQuestDisplay();
    }

    void OnEnable()
    {
        // Subscribe to events only if QuestManager exists
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAdded += OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted; // Separate handler for completions
            QuestManager.Instance.OnQuestRemoved += OnQuestRemoved;
            QuestManager.Instance.OnObjectiveCompleted += OnObjectiveChanged;
        }
    }

    void OnDisable()
    {
        // Safely unsubscribe
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAdded -= OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.Instance.OnQuestRemoved -= OnQuestRemoved;
            QuestManager.Instance.OnObjectiveCompleted -= OnObjectiveChanged;
        }
    }

    // Standard handler for quest changes
    void OnQuestChanged(Quest quest)
    {
        DebugLog($"Quest added or changed: {quest.questName} (Type: {quest.GetType().Name})");
        RefreshQuestDisplay();
    }

    // Special handler for completed quests
    void OnQuestCompleted(Quest quest)
    {
        string questType = quest.GetType().Name;
        DebugLog($"Quest completed: {quest.questName} (Type: {questType})");

        // First refresh to show completed state
        RefreshQuestDisplay();

        if (autoRemoveCompletedQuests && !pendingRemoval.Contains(quest))
        {
            // Start the removal process
            pendingRemoval.Add(quest);
            DebugLog($"Added quest to pending removal: {quest.questName}");

            // Choose appropriate delay based on quest type
            float delay = DetermineRemovalDelay(quest);
            StartCoroutine(RemoveCompletedQuestWithDelay(quest, delay));
        }
    }

    // Determine how long to wait before removing a completed quest
    private float DetermineRemovalDelay(Quest quest)
    {
        // Only remove MusicQuest quickly - all other quests get the standard display time
        if (quest is MusicQuest)
        {
            return 0.1f; // Quick removal for MusicQuest
        }
        else
        {
            return completionDisplayTime; // Standard time for all other quests
        }
    }

    // Handle quest removed from manager
    void OnQuestRemoved(Quest quest)
    {
        DebugLog($"Quest removed: {quest.questName} (Type: {quest.GetType().Name})");

        // Remove the quest from UI immediately
        if (questEntries.ContainsKey(quest))
        {
            Destroy(questEntries[quest].gameObject);
            questEntries.Remove(quest);
            DebugLog($"Quest UI entry removed: {quest.questName}");
        }

        // Remove from pending if it was there
        pendingRemoval.Remove(quest);

        // Update UI to show "No Quests" if needed
        if (noQuestsMessage != null)
        {
            noQuestsMessage.SetActive(questEntries.Count == 0);
        }
    }

    // Update display when objective changes
    void OnObjectiveChanged(Quest quest, int objectiveIndex)
    {
        DebugLog($"Objective changed for quest: {quest.questName}, index: {objectiveIndex}");

        if (questEntries.ContainsKey(quest))
        {
            questEntries[quest].RefreshDisplay();
        }

        // Handle special case for completed MusicQuest objectives
        if (quest is MusicQuest musicQuest)
        {
            bool allCompleted = true;
            foreach (var objective in quest.Objectives)
            {
                if (!objective.isCompleted)
                {
                    allCompleted = false;
                    break;
                }
            }

            // If all objectives are completed, force completion check
            if (allCompleted && !quest.IsCompleted)
            {
                DebugLog($"All objectives completed for MusicQuest, forcing completion check");
                quest.CheckQuestCompletion();
            }
        }
    }

    // Coroutine to remove completed quests after a delay
    private IEnumerator RemoveCompletedQuestWithDelay(Quest quest, float delay)
    {
        DebugLog($"Waiting {delay} seconds before removing quest: {quest.questName}");

        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Force immediate removal of MusicQuest instances from QuestManager
        if (quest is MusicQuest)
        {
            DebugLog($"Force removing MusicQuest from QuestManager: {quest.questName}");
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.RemoveQuest(quest);
            }
        }

        // Remove the quest from pending list
        pendingRemoval.Remove(quest);

        // Then remove the quest from UI if still needed
        if (questEntries.ContainsKey(quest))
        {
            DebugLog($"Removing quest from UI: {quest.questName}");

            Destroy(questEntries[quest].gameObject);
            questEntries.Remove(quest);

            // If not a MusicQuest (which was already removed), remove from QuestManager
            if (!(quest is MusicQuest) && QuestManager.Instance != null)
            {
                QuestManager.Instance.RemoveQuest(quest);
            }

            // Update the "No Quests" message
            if (noQuestsMessage != null)
            {
                noQuestsMessage.SetActive(questEntries.Count == 0);
            }
        }
    }

    // Rebuilds the entire quest display
    void RefreshQuestDisplay()
    {
        // Skip if QuestManager doesn't exist
        if (QuestManager.Instance == null) return;

        DebugLog("Refreshing quest display");

        // Get current active quests
        List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();
        DebugLog($"Active quests count: {activeQuests.Count}");

        // Remove entries that aren't active anymore
        List<Quest> questsToRemove = new List<Quest>();
        foreach (var quest in questEntries.Keys)
        {
            if (!activeQuests.Contains(quest) && !pendingRemoval.Contains(quest))
            {
                questsToRemove.Add(quest);
                DebugLog($"Quest no longer active and will be removed: {quest.questName}");
            }
        }

        foreach (var quest in questsToRemove)
        {
            Destroy(questEntries[quest].gameObject);
            questEntries.Remove(quest);
        }

        // Add new entries for quests not already displayed
        foreach (Quest quest in activeQuests)
        {
            if (!questEntries.ContainsKey(quest))
            {
                GameObject entryObj = Instantiate(questEntryPrefab, questContainer);
                QuestEntryUI entryUI = entryObj.GetComponent<QuestEntryUI>();

                if (entryUI != null)
                {
                    entryUI.Setup(quest);
                    questEntries.Add(quest, entryUI);
                    DebugLog($"Added new quest to display: {quest.questName} (Type: {quest.GetType().Name})");
                }
            }
            else
            {
                // Refresh the display for existing quests
                questEntries[quest].RefreshDisplay();

                // Special handling for MusicQuest that's completed but not yet scheduled for removal
                if (quest is MusicQuest && quest.IsCompleted && !pendingRemoval.Contains(quest))
                {
                    DebugLog($"Found completed MusicQuest that hasn't been scheduled for removal: {quest.questName}");
                    pendingRemoval.Add(quest);
                    StartCoroutine(RemoveCompletedQuestWithDelay(quest, 0.1f));
                }
            }
        }

        // Show "No Quests" message if needed
        if (noQuestsMessage != null)
        {
            noQuestsMessage.SetActive(questEntries.Count == 0);
        }
    }

    // Debug logging helper
    private void DebugLog(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[QuestDisplayManager] {message}");
        }
    }

    // Force an immediate check of all quests (can be called from inspector or other scripts)
    public void ForceRefreshAllQuests()
    {
        DebugLog("Force refreshing all quests");

        // Check for any stuck quests, especially MusicQuests
        if (QuestManager.Instance != null)
        {
            List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();
            foreach (Quest quest in activeQuests)
            {
                if (quest is MusicQuest && quest.IsCompleted)
                {
                    DebugLog($"Found stuck completed MusicQuest: {quest.questName}");
                    if (!pendingRemoval.Contains(quest))
                    {
                        pendingRemoval.Add(quest);
                        StartCoroutine(RemoveCompletedQuestWithDelay(quest, 0.1f));
                    }
                }
            }
        }

        RefreshQuestDisplay();
    }
}
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

    [Header("Special Quest Handling")]
    [SerializeField] private bool directlyTrackNametagQuest = true;
    [SerializeField] private float refreshInterval = 0.5f; // How often to refresh for nametag updates
    [SerializeField] private bool isNametagQuestAuthority = true; // Set this true to make this the main script that adds nametag quests

    [Header("Duplicate Handling")]
    [SerializeField] private bool removeDuplicateQuests = true;
    [SerializeField] private bool logDuplicateRemovals = true;
    [SerializeField] private float duplicateCheckDelay = 1.0f; // Delay before checking for duplicates
    [SerializeField] private bool performMultipleChecks = true; // Perform multiple checks to catch all duplicates
    [SerializeField] private int numberOfChecks = 3; // Number of checks to perform
    [SerializeField] private float checkInterval = 0.5f; // Time between checks

    private Dictionary<string, QuestEntryUI> questEntriesByKey = new Dictionary<string, QuestEntryUI>(); // New dictionary using key instead of reference
    private Dictionary<Quest, QuestEntryUI> questEntries = new Dictionary<Quest, QuestEntryUI>(); // Keep for backward compatibility
    private HashSet<Quest> pendingRemoval = new HashSet<Quest>(); // Track quests being removed
    private NametagQuestManager nametagQuestManager; // Reference to nametag manager
    private Coroutine periodicRefreshCoroutine;
    private Coroutine duplicateCheckCoroutine;

    [Header("Debug Options")]
    [SerializeField] private bool enableDebugLogging = true;

    // Helper method to get a unique key for a quest (name + type)
    private string GetQuestKey(Quest quest)
    {
        if (quest == null) return "";
        return quest.questName + "_" + quest.GetType().Name;
    }

    // Helper method to find a quest entry by key
    private QuestEntryUI FindQuestEntryByKey(string key)
    {
        if (questEntriesByKey.ContainsKey(key))
            return questEntriesByKey[key];
        return null;
    }

    // Helper method to find a quest by name and type
    private Quest FindQuestByNameAndType(string questName, System.Type questType)
    {
        if (QuestManager.Instance == null) return null;

        foreach (var quest in QuestManager.Instance.GetActiveQuests())
        {
            if (quest != null && quest.questName == questName && quest.GetType() == questType)
                return quest;
        }
        return null;
    }

    void Start()
    {
        Debug.Log("QuestDisplayManager is attached to this GameObject", this.gameObject);

        // Find the nametag quest manager
        nametagQuestManager = FindObjectOfType<NametagQuestManager>();
        if (nametagQuestManager != null)
        {
            DebugLog("Found NametagQuestManager reference");

            // Only manage nametag quests if this is the authority
            if (directlyTrackNametagQuest && isNametagQuestAuthority)
            {
                // Try to activate tracking
                System.Reflection.MethodInfo method = nametagQuestManager.GetType().GetMethod("ForceActivateTracking");
                if (method != null)
                {
                    method.Invoke(nametagQuestManager, null);
                    DebugLog("Activated nametag tracking from QuestDisplayManager");
                }
                else
                {
                    DebugLog("WARNING: NametagQuestManager doesn't have ForceActivateTracking method!");
                }

                // Get the quest and safely add it if needed
                Quest nametagQuest = nametagQuestManager.GetNametagQuest();
                if (nametagQuest != null && QuestManager.Instance != null)
                {
                    SafelyAddQuest(nametagQuest);
                }
            }
        }
        else
        {
            DebugLog("WARNING: No NametagQuestManager found in scene!");
        }

        // Start delayed duplicate removal process
        if (removeDuplicateQuests)
        {
            if (duplicateCheckCoroutine != null)
                StopCoroutine(duplicateCheckCoroutine);

            duplicateCheckCoroutine = StartCoroutine(DelayedDuplicateRemoval());
        }

        RefreshQuestDisplay();

        // Start periodic refresh to catch nametag updates
        if (directlyTrackNametagQuest && nametagQuestManager != null)
        {
            periodicRefreshCoroutine = StartCoroutine(PeriodicRefresh());
        }
    }

    // NEW COROUTINE: Performs multiple checks for duplicates with delays
    private IEnumerator DelayedDuplicateRemoval()
    {
        DebugLog($"Starting delayed duplicate check in {duplicateCheckDelay} seconds");

        // Wait for initial delay
        yield return new WaitForSeconds(duplicateCheckDelay);

        // Perform initial cleanup
        int removedCount = RemoveDuplicateQuests();
        DebugLog($"Initial duplicate check removed {removedCount} duplicates");

        // If we need to perform multiple checks
        if (performMultipleChecks)
        {
            for (int i = 0; i < numberOfChecks; i++)
            {
                // Wait between checks
                yield return new WaitForSeconds(checkInterval);

                // Perform another cleanup
                removedCount = RemoveDuplicateQuests();
                DebugLog($"Follow-up duplicate check #{i + 1} removed {removedCount} duplicates");

                // If no duplicates found, we can stop early
                if (removedCount == 0)
                    break;

                // Refresh the display after each check
                RefreshQuestDisplay();
            }
        }

        // Final refresh to ensure everything is clean
        RefreshQuestDisplay();
        DebugLog("Completed all duplicate removal checks");
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

        // Stop the refresh coroutine
        if (periodicRefreshCoroutine != null)
        {
            StopCoroutine(periodicRefreshCoroutine);
            periodicRefreshCoroutine = null;
        }

        // Stop the duplicate check coroutine
        if (duplicateCheckCoroutine != null)
        {
            StopCoroutine(duplicateCheckCoroutine);
            duplicateCheckCoroutine = null;
        }
    }

    // Safely add a quest, preventing duplicates
    public void SafelyAddQuest(Quest quest)
    {
        if (quest == null || QuestManager.Instance == null) return;

        string questKey = GetQuestKey(quest);
        bool isDuplicate = false;

        // Check active quests for duplicates
        foreach (var activeQuest in QuestManager.Instance.GetActiveQuests())
        {
            if (activeQuest != null && GetQuestKey(activeQuest) == questKey)
            {
                isDuplicate = true;
                DebugLog($"Quest already active (by name+type): {quest.questName}");
                break;
            }
        }

        // Only add if not a duplicate
        if (!isDuplicate)
        {
            DebugLog($"Safely adding quest: {quest.questName}");
            QuestManager.Instance.AddQuest(quest);
        }
    }

    // Periodically refresh the display to catch nametag updates
    private IEnumerator PeriodicRefresh()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);

            // Only refresh if we have a nametag quest active
            if (nametagQuestManager != null)
            {
                Quest nametagQuest = nametagQuestManager.GetNametagQuest();
                if (nametagQuest != null)
                {
                    string questKey = GetQuestKey(nametagQuest);
                    QuestEntryUI entryUI = FindQuestEntryByKey(questKey);

                    if (entryUI != null)
                    {
                        entryUI.RefreshDisplay();
                    }
                    else
                    {
                        // Try to find the actual active version of this quest
                        Quest activeQuest = FindQuestByNameAndType(nametagQuest.questName, nametagQuest.GetType());
                        if (activeQuest != null && questEntries.ContainsKey(activeQuest))
                        {
                            questEntries[activeQuest].RefreshDisplay();
                        }
                    }
                }
            }
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
        // Quick removal for MusicQuest
        if (quest is MusicQuest)
            return 0.1f;
        // Special case for NametagQuest (keep visible a bit longer)
        else if (quest is NametagQuest)
            return completionDisplayTime * 1.5f; // Show nametag completion longer
        else
            return completionDisplayTime; // Standard time for all other quests
    }

    // Handle quest removed from manager
    void OnQuestRemoved(Quest quest)
    {
        if (quest == null) return;

        DebugLog($"Quest removed: {quest.questName} (Type: {quest.GetType().Name})");

        // Get key for this quest
        string questKey = GetQuestKey(quest);

        // Remove from both dictionaries
        if (questEntries.ContainsKey(quest))
        {
            QuestEntryUI entryUI = questEntries[quest];
            questEntries.Remove(quest);
            questEntriesByKey.Remove(questKey);

            Destroy(entryUI.gameObject);
            DebugLog($"Quest UI entry removed: {quest.questName}");
        }
        else if (questEntriesByKey.ContainsKey(questKey))
        {
            QuestEntryUI entryUI = questEntriesByKey[questKey];
            questEntriesByKey.Remove(questKey);

            // Also remove from the reference dictionary
            foreach (var kvp in new Dictionary<Quest, QuestEntryUI>(questEntries))
            {
                if (kvp.Value == entryUI)
                {
                    questEntries.Remove(kvp.Key);
                    break;
                }
            }

            Destroy(entryUI.gameObject);
            DebugLog($"Quest UI entry removed by key: {quest.questName}");
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
        if (quest == null) return;

        DebugLog($"Objective changed for quest: {quest.questName}, index: {objectiveIndex}");

        // Try direct reference first
        if (questEntries.ContainsKey(quest))
        {
            questEntries[quest].RefreshDisplay();
        }
        else
        {
            // Try by key if direct reference fails
            string questKey = GetQuestKey(quest);
            QuestEntryUI entryUI = FindQuestEntryByKey(questKey);
            if (entryUI != null)
            {
                entryUI.RefreshDisplay();
            }
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

        // Special case for NametagQuest - similar to MusicQuest handling
        if (quest is NametagQuest nametagQuest)
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
                DebugLog($"All objectives completed for NametagQuest, forcing completion check");
                quest.CheckQuestCompletion();
            }
        }
    }

    // Coroutine to remove completed quests after a delay
    private IEnumerator RemoveCompletedQuestWithDelay(Quest quest, float delay)
    {
        if (quest == null) yield break;

        DebugLog($"Waiting {delay} seconds before removing quest: {quest.questName}");

        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Force immediate removal of MusicQuest instances from QuestManager
        if (quest is MusicQuest || quest is NametagQuest)
        {
            DebugLog($"Force removing special quest from QuestManager: {quest.questName}");
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.RemoveQuest(quest);
            }
        }

        // Remove the quest from pending list
        pendingRemoval.Remove(quest);

        // Get the quest key
        string questKey = GetQuestKey(quest);

        // Then remove the quest from UI if still needed - try both references
        if (questEntries.ContainsKey(quest))
        {
            RemoveQuestFromUI(quest);
        }
        else
        {
            QuestEntryUI entryUI = FindQuestEntryByKey(questKey);
            if (entryUI != null)
            {
                RemoveQuestEntryUI(entryUI, questKey);
            }
        }
    }

    // Helper to remove a quest from the UI
    private void RemoveQuestFromUI(Quest quest)
    {
        if (quest == null) return;

        DebugLog($"Removing quest from UI: {quest.questName}");

        string questKey = GetQuestKey(quest);
        if (questEntries.ContainsKey(quest))
        {
            QuestEntryUI entryUI = questEntries[quest];
            Destroy(entryUI.gameObject);
            questEntries.Remove(quest);
            questEntriesByKey.Remove(questKey);
        }

        // If not already removed, remove from QuestManager
        if (!(quest is MusicQuest || quest is NametagQuest) && QuestManager.Instance != null)
        {
            QuestManager.Instance.RemoveQuest(quest);
        }

        // Update the "No Quests" message
        if (noQuestsMessage != null)
        {
            noQuestsMessage.SetActive(questEntries.Count == 0);
        }
    }

    // Helper to remove a quest entry UI
    private void RemoveQuestEntryUI(QuestEntryUI entryUI, string questKey)
    {
        if (entryUI == null) return;

        DebugLog($"Removing quest entry UI for key: {questKey}");

        Destroy(entryUI.gameObject);
        questEntriesByKey.Remove(questKey);

        // Also remove from the reference dictionary
        Quest questToRemove = null;
        foreach (var kvp in questEntries)
        {
            if (kvp.Value == entryUI)
            {
                questToRemove = kvp.Key;
                break;
            }
        }

        if (questToRemove != null)
            questEntries.Remove(questToRemove);

        // Update the "No Quests" message
        if (noQuestsMessage != null)
        {
            noQuestsMessage.SetActive(questEntries.Count == 0);
        }
    }

    // Method to handle duplicate quests
    private int RemoveDuplicateQuests()
    {
        if (QuestManager.Instance == null) return 0;

        Dictionary<string, Quest> uniqueQuests = new Dictionary<string, Quest>();
        List<Quest> duplicatesToRemove = new List<Quest>();

        // Get a fresh list of active quests directly from QuestManager
        List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();
        DebugLog($"Checking for duplicates among {activeQuests.Count} active quests");

        foreach (Quest quest in activeQuests)
        {
            if (quest == null) continue;

            string questKey = GetQuestKey(quest);

            if (uniqueQuests.ContainsKey(questKey))
            {
                duplicatesToRemove.Add(quest);

                if (logDuplicateRemovals)
                {
                    DebugLog($"Marked duplicate quest for removal: {quest.questName} ({quest.GetType().Name})");
                }
            }
            else
            {
                uniqueQuests[questKey] = quest;
            }
        }

        // Now remove all duplicates
        foreach (Quest duplicate in duplicatesToRemove)
        {
            QuestManager.Instance.RemoveQuest(duplicate);
        }

        if (duplicatesToRemove.Count > 0 && logDuplicateRemovals)
        {
            DebugLog($"Removed {duplicatesToRemove.Count} duplicate quests");
        }

        return duplicatesToRemove.Count;
    }

    // Public method to manually force duplicate removal
    public void ForceDuplicateRemoval()
    {
        int removed = RemoveDuplicateQuests();
        DebugLog($"Force duplicate removal: removed {removed} quests");
        RefreshQuestDisplay();
    }

    // Rebuilds the entire quest display
    void RefreshQuestDisplay()
    {
        // Skip if QuestManager doesn't exist
        if (QuestManager.Instance == null) return;

        DebugLog("Refreshing quest display");

        // Remove duplicates first if enabled
        if (removeDuplicateQuests)
        {
            RemoveDuplicateQuests();
        }

        // Get current active quests
        List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();
        DebugLog($"Active quests count: {activeQuests.Count}");

        // Track which quest entries to keep
        HashSet<string> activeQuestKeys = new HashSet<string>();
        foreach (Quest quest in activeQuests)
        {
            if (quest != null)
                activeQuestKeys.Add(GetQuestKey(quest));
        }

        // Remove entries that aren't active anymore
        List<Quest> questsToRemove = new List<Quest>();
        foreach (var kvp in questEntries)
        {
            Quest quest = kvp.Key;
            if (quest == null) continue;

            string questKey = GetQuestKey(quest);

            if (!activeQuestKeys.Contains(questKey) && !pendingRemoval.Contains(quest))
            {
                questsToRemove.Add(quest);
                DebugLog($"Quest no longer active and will be removed: {quest.questName}");
            }
        }

        foreach (var quest in questsToRemove)
        {
            RemoveQuestFromUI(quest);
        }

        // Create a hash set of keys we already have UI for
        HashSet<string> existingKeys = new HashSet<string>(questEntriesByKey.Keys);

        // Add new entries for quests not already displayed
        foreach (Quest quest in activeQuests)
        {
            if (quest == null) continue;

            string questKey = GetQuestKey(quest);

            // Check if we already have this quest type in the UI
            if (!existingKeys.Contains(questKey))
            {
                GameObject entryObj = Instantiate(questEntryPrefab, questContainer);
                QuestEntryUI entryUI = entryObj.GetComponent<QuestEntryUI>();

                if (entryUI != null)
                {
                    entryUI.Setup(quest);
                    questEntries[quest] = entryUI;
                    questEntriesByKey[questKey] = entryUI;
                    DebugLog($"Added new quest to display: {quest.questName} (Type: {quest.GetType().Name})");
                }
            }
            else
            {
                // Refresh the display for existing quests
                if (questEntries.ContainsKey(quest))
                {
                    questEntries[quest].RefreshDisplay();
                }
                else
                {
                    QuestEntryUI entryUI = questEntriesByKey[questKey];
                    if (entryUI != null)
                    {
                        // Update the entryUI's quest reference
                        entryUI.Setup(quest);
                        questEntries[quest] = entryUI;
                        DebugLog($"Updated existing UI with new quest instance: {quest.questName}");
                    }
                }

                // Special handling for quests that are completed but not yet scheduled for removal
                if ((quest is MusicQuest || quest is NametagQuest) &&
                    quest.IsCompleted && !pendingRemoval.Contains(quest))
                {
                    DebugLog($"Found completed special quest that hasn't been scheduled for removal: {quest.questName}");
                    pendingRemoval.Add(quest);
                    StartCoroutine(RemoveCompletedQuestWithDelay(quest,
                        quest is MusicQuest ? 0.1f : completionDisplayTime * 1.5f));
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

        // Remove duplicates first to clean up the display
        if (removeDuplicateQuests)
        {
            RemoveDuplicateQuests();
        }

        // If we have nametag manager, get its quest directly
        if (nametagQuestManager != null && directlyTrackNametagQuest && isNametagQuestAuthority)
        {
            Quest nametagQuest = nametagQuestManager.GetNametagQuest();
            if (nametagQuest != null)
            {
                // Only add if not already present
                SafelyAddQuest(nametagQuest);
            }

            // Ensure tracking is active
            System.Reflection.MethodInfo method = nametagQuestManager.GetType().GetMethod("ForceActivateTracking");
            if (method != null)
            {
                method.Invoke(nametagQuestManager, null);
                DebugLog("Re-activated nametag tracking from ForceRefresh");
            }
        }

        // Check for any stuck quests
        if (QuestManager.Instance != null)
        {
            List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();
            foreach (Quest quest in activeQuests)
            {
                if ((quest is MusicQuest || quest is NametagQuest) && quest.IsCompleted)
                {
                    DebugLog($"Found stuck completed special quest: {quest.questName}");
                    if (!pendingRemoval.Contains(quest))
                    {
                        pendingRemoval.Add(quest);
                        StartCoroutine(RemoveCompletedQuestWithDelay(quest,
                            quest is MusicQuest ? 0.1f : completionDisplayTime));
                    }
                }
            }
        }

        RefreshQuestDisplay();
    }
}
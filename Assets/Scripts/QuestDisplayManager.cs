using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private Transform questContainer;
    [SerializeField] private GameObject noQuestsMessage;
    [SerializeField] private bool autoRemoveCompletedQuests = true;
    [SerializeField] private bool enableDebugLogs = true;

    private Dictionary<Quest, QuestEntryUI> questEntries = new Dictionary<Quest, QuestEntryUI>();
    private bool isRefreshing = false;

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[QuestDisplay] {message}");
    }

    void Start()
    {
        if (questEntryPrefab == null || questContainer == null)
        {
            Debug.LogError("[QuestDisplay] Required references not set!");
            return;
        }

        // Clear any design-time elements
        ClearQuestContainer();
        RefreshQuestDisplay();
    }

    void OnEnable()
    {
        // Subscribe to all the relevant events
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAdded += OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestManager.Instance.OnQuestRemoved += OnQuestChanged;
            QuestManager.Instance.OnObjectiveCompleted += OnObjectiveCompleted;

            // Force refresh on enable
            RefreshQuestDisplay();
        }
    }

    void OnDisable()
    {
        // Unsubscribe from all events
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAdded -= OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.Instance.OnQuestRemoved -= OnQuestChanged;
            QuestManager.Instance.OnObjectiveCompleted -= OnObjectiveCompleted;
        }
    }

    // Called when a quest is added or removed
    private void OnQuestChanged(Quest quest)
    {
        DebugLog($"Quest changed: {quest.questName}");
        RefreshQuestDisplay();
    }

    // Called when a quest is completed
    private void OnQuestCompleted(Quest quest)
    {
        DebugLog($"Quest completed: {quest.questName} - Will remove from display");

        // Handle completed quests - IMPORTANT FIX
        if (autoRemoveCompletedQuests)
        {
            // Force removal of this quest from UI
            RemoveQuestEntry(quest);

            // Force a refresh to ensure the quest is gone
            StartCoroutine(DelayedRefresh(0.1f));
        }
        else
        {
            RefreshQuestDisplay();
        }
    }

    // Delay refresh for smoother transitions
    private IEnumerator DelayedRefresh(float delay)
    {
        yield return new WaitForSeconds(delay);
        RefreshQuestDisplay();
    }

    // Called when a quest objective is completed
    private void OnObjectiveCompleted(Quest quest, int objectiveIndex)
    {
        DebugLog($"Objective completed: {quest.questName} - Objective #{objectiveIndex}");

        // Check if the whole quest is now complete
        if (quest.DebugCheckCompletion())
        {
            DebugLog($"All objectives completed for: {quest.questName} - Should auto-complete soon");

            // IMPORTANT: Force the quest to complete if all objectives are done
            // This handles cases where auto-completion might not trigger properly
            QuestManager.Instance.CompleteQuest(quest);

            // Remove from display right away
            RemoveQuestEntry(quest);
        }
        else
        {
            // Only update the specific quest entry
            if (questEntries.TryGetValue(quest, out QuestEntryUI entryUI) && entryUI != null)
            {
                entryUI.RefreshDisplay();
            }
            else
            {
                // Fall back to full refresh if entry not found
                RefreshQuestDisplay();
            }
        }
    }

    // Explicitly remove a quest entry from display
    private void RemoveQuestEntry(Quest quest)
    {
        DebugLog($"Removing quest entry: {quest.questName}");

        if (questEntries.TryGetValue(quest, out QuestEntryUI entryUI) && entryUI != null)
        {
            Destroy(entryUI.gameObject);
            questEntries.Remove(quest);
            UpdateNoQuestsMessage();
        }
    }

    // Clear all quest entries from container
    private void ClearQuestContainer()
    {
        foreach (Transform child in questContainer)
        {
            Destroy(child.gameObject);
        }
        questEntries.Clear();
    }

    // Update the "No Quests" message
    private void UpdateNoQuestsMessage()
    {
        if (noQuestsMessage != null)
        {
            noQuestsMessage.SetActive(questEntries.Count == 0);
        }
    }

    // Rebuild the entire quest display - publicly accessible
    public void RefreshQuestDisplay()
    {
        if (isRefreshing)
            return;

        isRefreshing = true;
        StartCoroutine(RefreshCoroutine());
    }

    private IEnumerator RefreshCoroutine()
    {
        DebugLog("Refreshing quest display");

        if (QuestManager.Instance == null)
        {
            DebugLog("QuestManager instance is null!");
            isRefreshing = false;
            yield break;
        }

        // Get active quests
        List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();

        // Get completed quests to ensure we don't display them
        List<Quest> completedQuests = QuestManager.Instance.GetCompletedQuests();

        // Enhanced debugging - check all quests for completion status
        DebugLog("Checking active quests for completion status:");
        foreach (var quest in activeQuests)
        {
            if (quest == null) continue;

            // Use our debug helper to check objectives
            bool allObjectivesDone = true;
            foreach (var objective in quest.Objectives)
            {
                allObjectivesDone = allObjectivesDone && objective.isCompleted;
            }

            DebugLog($"  Quest: {quest.questName} - All objectives done: {allObjectivesDone}, IsCompleted property: {quest.IsCompleted}");

            // If all objectives are done but the quest is not being marked as completed,
            // we have a problem in the Quest class's IsCompleted property
            if (allObjectivesDone && !quest.IsCompleted)
            {
                Debug.LogWarning($"[QuestDisplay] Quest {quest.questName} has all objectives completed but IsCompleted is false!");

                // IMPORTANT: Force quest completion in this case
                Debug.Log($"[QuestDisplay] FORCING completion of quest {quest.questName}");
                QuestManager.Instance.CompleteQuest(quest);
            }
        }

        DebugLog($"Found {activeQuests.Count} active quests and {completedQuests.Count} completed quests");

        // Clear existing entries
        ClearQuestContainer();

        // Add entries for active quests - track names to avoid duplicates
        HashSet<string> questNames = new HashSet<string>();

        foreach (Quest quest in activeQuests)
        {
            if (quest == null) continue;

            // Skip completed quests
            if (completedQuests.Contains(quest) || quest.IsCompleted)
            {
                DebugLog($"Skipping completed quest: {quest.questName}");
                continue;
            }

            // Skip duplicate quest names
            if (questNames.Contains(quest.questName))
            {
                DebugLog($"Skipping duplicate quest: {quest.questName}");
                continue;
            }

            // Add to display
            questNames.Add(quest.questName);
            GameObject entryObj = Instantiate(questEntryPrefab, questContainer);
            QuestEntryUI entryUI = entryObj.GetComponent<QuestEntryUI>();

            if (entryUI != null)
            {
                entryUI.Setup(quest);
                questEntries.Add(quest, entryUI);
                DebugLog($"Added quest: {quest.questName}");
            }
        }

        // Update "No Quests" message
        UpdateNoQuestsMessage();

        // Small delay before allowing next refresh
        yield return new WaitForSeconds(0.1f);
        isRefreshing = false;
    }
}
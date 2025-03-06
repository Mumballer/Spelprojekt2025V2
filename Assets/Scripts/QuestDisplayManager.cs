using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private Transform questContainer;
    [SerializeField] private GameObject noQuestsMessage;
    [SerializeField] private bool autoRemoveCompletedQuests = true; // New option to auto-remove quests

    private Dictionary<Quest, QuestEntryUI> questEntries = new Dictionary<Quest, QuestEntryUI>();

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
            QuestManager.Instance.OnQuestRemoved += OnQuestChanged;
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
            QuestManager.Instance.OnQuestRemoved -= OnQuestChanged;
            QuestManager.Instance.OnObjectiveCompleted -= OnObjectiveChanged;
        }
    }

    // Standard handler for quest changes
    void OnQuestChanged(Quest quest)
    {
        RefreshQuestDisplay();
    }

    // Special handler for completed quests
    void OnQuestCompleted(Quest quest)
    {
        if (autoRemoveCompletedQuests)
        {
            // Remove the completed quest from display with a short delay
            StartCoroutine(RemoveCompletedQuestWithDelay(quest, 2f));
        }
        else
        {
            // Just update the display
            RefreshQuestDisplay();
        }
    }

    // Update display when objective changes
    void OnObjectiveChanged(Quest quest, int objectiveIndex)
    {
        if (questEntries.ContainsKey(quest))
        {
            questEntries[quest].RefreshDisplay();
        }
    }

    // Coroutine to remove completed quests after a delay
    private IEnumerator RemoveCompletedQuestWithDelay(Quest quest, float delay)
    {
        // First refresh the display to show the completed state
        RefreshQuestDisplay();

        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Then remove the quest from UI
        if (questEntries.ContainsKey(quest))
        {
            Destroy(questEntries[quest].gameObject);
            questEntries.Remove(quest);

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

        // Clear existing entries
        foreach (var entry in questEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        questEntries.Clear();

        // Create new entries for active quests
        List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();

        foreach (Quest quest in activeQuests)
        {
            GameObject entryObj = Instantiate(questEntryPrefab, questContainer);
            QuestEntryUI entryUI = entryObj.GetComponent<QuestEntryUI>();

            if (entryUI != null)
            {
                entryUI.Setup(quest);
                questEntries.Add(quest, entryUI);
            }
        }

        // Show "No Quests" message if needed
        if (noQuestsMessage != null)
        {
            noQuestsMessage.SetActive(questEntries.Count == 0);
        }
    }
}
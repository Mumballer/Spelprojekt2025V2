using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private Transform questContainer;
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private GameObject noQuestsMessage;

    [Header("Tab UI")]
    [SerializeField] private Button activeQuestsTabButton;
    [SerializeField] private Button completedQuestsTabButton;
    [SerializeField] private Button availableQuestsTabButton;

    [Header("Quest Details")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI questObjectivesText;
    [SerializeField] private Button acceptQuestButton;

    [Header("Color Settings")]
    [SerializeField] private Color inProgressColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color titleColor = Color.white;
    [SerializeField] private Color completedTitleColor = Color.green;

    private enum QuestTab { Active, Completed, Available }
    private QuestTab currentTab = QuestTab.Active;
    private Quest selectedQuest;
    private Dictionary<Quest, QuestEntryUI> activeQuestEntries = new Dictionary<Quest, QuestEntryUI>();
    private Dictionary<Quest, QuestEntryUI> completedQuestEntries = new Dictionary<Quest, QuestEntryUI>();
    private Dictionary<Quest, QuestEntryUI> availableQuestEntries = new Dictionary<Quest, QuestEntryUI>();

    private void Awake()
    {
        if (questPanel != null)
            questPanel.SetActive(false);

        if (detailsPanel != null)
            detailsPanel.SetActive(false);

        // Set up tab button listeners
        if (activeQuestsTabButton != null)
            activeQuestsTabButton.onClick.AddListener(() => SwitchTab(QuestTab.Active));

        if (completedQuestsTabButton != null)
            completedQuestsTabButton.onClick.AddListener(() => SwitchTab(QuestTab.Completed));

        if (availableQuestsTabButton != null)
            availableQuestsTabButton.onClick.AddListener(() => SwitchTab(QuestTab.Available));

        if (acceptQuestButton != null)
            acceptQuestButton.onClick.AddListener(AcceptSelectedQuest);
    }

    private void OnEnable()
    {
        // Subscribe to quest events
        QuestManager.Instance.OnQuestAdded += OnQuestAdded;
        QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
        QuestManager.Instance.OnQuestAvailable += OnQuestAvailable;
        QuestManager.Instance.OnObjectiveCompleted += OnObjectiveCompleted;

        RefreshAllQuests();
        SwitchTab(currentTab);
    }

    private void OnDisable()
    {
        // Unsubscribe from quest events
        QuestManager.Instance.OnQuestAdded -= OnQuestAdded;
        QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        QuestManager.Instance.OnQuestAvailable -= OnQuestAvailable;
        QuestManager.Instance.OnObjectiveCompleted -= OnObjectiveCompleted;
    }

    public void ToggleQuestPanel()
    {
        if (questPanel != null)
        {
            bool isActive = !questPanel.activeSelf;
            questPanel.SetActive(isActive);

            if (isActive)
            {
                RefreshAllQuests();
                SwitchTab(currentTab);
            }
            else
            {
                CloseDetailsPanel();
            }
        }
    }

    private void OnQuestAdded(Quest quest)
    {
        AddQuestToList(quest, QuestTab.Active);
        RefreshQuestsList(currentTab);
    }

    private void OnQuestCompleted(Quest quest)
    {
        if (activeQuestEntries.ContainsKey(quest))
        {
            Destroy(activeQuestEntries[quest].gameObject);
            activeQuestEntries.Remove(quest);
        }

        AddQuestToList(quest, QuestTab.Completed);
        RefreshQuestsList(currentTab);
    }

    private void OnQuestAvailable(Quest quest)
    {
        AddQuestToList(quest, QuestTab.Available);
        RefreshQuestsList(currentTab);
    }

    private void OnObjectiveCompleted(Quest quest, int objectiveIndex)
    {
        if (activeQuestEntries.ContainsKey(quest))
        {
            activeQuestEntries[quest].RefreshDisplay();
        }

        if (selectedQuest == quest)
        {
            UpdateDetailsPanel(quest);
        }
    }

    private void RefreshAllQuests()
    {
        ClearAllQuestLists();

        // Active quests
        List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();
        foreach (Quest quest in activeQuests)
        {
            AddQuestToList(quest, QuestTab.Active);
        }

        // Completed quests
        List<Quest> completedQuests = QuestManager.Instance.GetCompletedQuests();
        foreach (Quest quest in completedQuests)
        {
            AddQuestToList(quest, QuestTab.Completed);
        }

        // Available quests
        List<Quest> availableQuests = QuestManager.Instance.GetAvailableQuests();
        foreach (Quest quest in availableQuests)
        {
            AddQuestToList(quest, QuestTab.Available);
        }
    }

    private void ClearAllQuestLists()
    {
        foreach (var entry in activeQuestEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        activeQuestEntries.Clear();

        foreach (var entry in completedQuestEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        completedQuestEntries.Clear();

        foreach (var entry in availableQuestEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        availableQuestEntries.Clear();
    }

    private void AddQuestToList(Quest quest, QuestTab tab)
    {
        if (quest == null || questEntryPrefab == null || questContainer == null)
            return;

        Dictionary<Quest, QuestEntryUI> targetDictionary = GetDictionaryForTab(tab);

        if (targetDictionary.ContainsKey(quest))
            return;

        GameObject entryObj = Instantiate(questEntryPrefab, questContainer);
        entryObj.SetActive(false); // Hide initially

        QuestEntryUI entryUI = entryObj.GetComponent<QuestEntryUI>();
        if (entryUI != null)
        {
            // This is line 286 that had the error - changed from SetupQuest to Setup
            entryUI.Setup(quest);

            // Add click handler
            Button button = entryObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => SelectQuest(quest));
            }

            targetDictionary.Add(quest, entryUI);
        }
    }

    private void AcceptSelectedQuest()
    {
        if (selectedQuest != null)
        {
            QuestManager.Instance.AddQuest(selectedQuest);

            // Remove from available list
            if (availableQuestEntries.ContainsKey(selectedQuest))
            {
                Destroy(availableQuestEntries[selectedQuest].gameObject);
                availableQuestEntries.Remove(selectedQuest);
            }

            // If viewing available quests, refresh the list
            if (currentTab == QuestTab.Available)
            {
                RefreshQuestsList(QuestTab.Available);
            }

            CloseDetailsPanel();
        }
    }

    private void SwitchTab(QuestTab tab)
    {
        currentTab = tab;

        // Update UI elements to reflect selected tab
        if (activeQuestsTabButton != null)
            activeQuestsTabButton.interactable = tab != QuestTab.Active;

        if (completedQuestsTabButton != null)
            completedQuestsTabButton.interactable = tab != QuestTab.Completed;

        if (availableQuestsTabButton != null)
            availableQuestsTabButton.interactable = tab != QuestTab.Available;

        RefreshQuestsList(tab);
        CloseDetailsPanel();
    }

    private void RefreshQuestsList(QuestTab tab)
    {
        // Hide all quest entries first
        foreach (var entry in activeQuestEntries.Values)
        {
            entry.gameObject.SetActive(false);
        }

        foreach (var entry in completedQuestEntries.Values)
        {
            entry.gameObject.SetActive(false);
        }

        foreach (var entry in availableQuestEntries.Values)
        {
            entry.gameObject.SetActive(false);
        }

        // Show entries based on selected tab
        Dictionary<Quest, QuestEntryUI> entriesToShow = GetDictionaryForTab(tab);

        foreach (var entry in entriesToShow.Values)
        {
            entry.gameObject.SetActive(true);
        }

        // Show "No Quests" message if needed
        if (noQuestsMessage != null)
        {
            noQuestsMessage.SetActive(entriesToShow.Count == 0);
        }
    }

    private Dictionary<Quest, QuestEntryUI> GetDictionaryForTab(QuestTab tab)
    {
        switch (tab)
        {
            case QuestTab.Active:
                return activeQuestEntries;
            case QuestTab.Completed:
                return completedQuestEntries;
            case QuestTab.Available:
                return availableQuestEntries;
            default:
                return activeQuestEntries;
        }
    }

    private void SelectQuest(Quest quest)
    {
        selectedQuest = quest;
        UpdateDetailsPanel(quest);
    }

    private void UpdateDetailsPanel(Quest quest)
    {
        if (detailsPanel == null || quest == null)
            return;

        detailsPanel.SetActive(true);

        if (questTitleText != null)
            questTitleText.text = quest.questName;

        if (questDescriptionText != null)
            questDescriptionText.text = quest.description;

        // Update objectives text
        if (questObjectivesText != null && quest.Objectives != null)
        {
            string objectivesContent = "";
            foreach (var objective in quest.Objectives)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(objective.isCompleted ? completedColor : inProgressColor);
                string checkmark = objective.isCompleted ? "✓ " : "• ";
                objectivesContent += $"<color=#{colorHex}>{checkmark}{objective.description}</color>\n";
            }
            questObjectivesText.text = objectivesContent.TrimEnd('\n');
        }

        // Show/hide the accept button based on tab
        if (acceptQuestButton != null)
        {
            acceptQuestButton.gameObject.SetActive(currentTab == QuestTab.Available);
        }
    }

    private void CloseDetailsPanel()
    {
        if (detailsPanel != null)
        {
            detailsPanel.SetActive(false);
        }
        selectedQuest = null;
    }
}
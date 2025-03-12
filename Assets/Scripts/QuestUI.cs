using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI questStatusText;
    [SerializeField] private Button closeButton;
    
    [Header("Text Settings")]
    [SerializeField] private Color inProgressColor = new Color(1f, 0.92f, 0.016f); // Yellow
    [SerializeField] private Color completedColor = new Color(0f, 0.75f, 0.22f);   // Green
    
    private Quest currentQuest;
    
    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideQuestPanel);
        }
        
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += HandleQuestStarted;
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated += HandleObjectiveUpdated;
        }
        
        if (questPanel != null)
        {
            questPanel.SetActive(false); // Hide initially
        }
    }
    
    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated -= HandleObjectiveUpdated;
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HideQuestPanel);
        }
    }
    
    private void HandleQuestStarted(Quest quest)
    {
        currentQuest = quest;
        UpdateQuestDisplay();
        ShowQuestPanel();
    }
    
    private void HandleQuestCompleted(Quest quest)
    {
        // If the completed quest is our current quest, update display
        if (currentQuest == quest)
        {
            UpdateQuestDisplay();
        }
    }
    
    private void HandleObjectiveUpdated(QuestObjective objective)
    {
        // Check if the objective is part of the current quest
        if (currentQuest != null && currentQuest.objectives.Contains(objective))
        {
            UpdateQuestDisplay();
        }
    }
    
    private void UpdateQuestDisplay()
    {
        if (currentQuest == null) return;
        
        // Set the quest title - ALWAYS use quest name
        if (questTitleText != null)
        {
            questTitleText.text = currentQuest.questName;
            Debug.Log($"Setting title text to: {currentQuest.questName}");
        }
        
        // Set the quest description - ALWAYS use quest description
        if (questDescriptionText != null)
        {
            questDescriptionText.text = currentQuest.description;
            Debug.Log($"Setting description text to: {currentQuest.description}");
        }
        
        // Update the status
        if (questStatusText != null)
        {
            if (currentQuest.IsCompleted)
            {
                questStatusText.text = "[COMPLETED]";
                questStatusText.color = completedColor;
            }
            else
            {
                questStatusText.text = "[IN PROGRESS]";
                questStatusText.color = inProgressColor;
                
                // Optional: Add progress info
                int completedObjectives = currentQuest.objectives.Count(o => o.isCompleted);
                questStatusText.text += $" ({completedObjectives}/{currentQuest.objectives.Count})";
            }
            Debug.Log($"Setting status text to: {questStatusText.text}");
        }
    }
    
    public void ShowQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(true);
        }
    }
    
    public void HideQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
    }
    
    public void ForceShowTestQuest()
    {
        // Create a test quest
        Quest testQuest = ScriptableObject.CreateInstance<Quest>();
        testQuest.questName = "Set the table";
        testQuest.description = "Arrange the table orderly";
        
        // Create an objective
        QuestObjective objective = ScriptableObject.CreateInstance<QuestObjective>();
        objective.description = "Arrange the table orderly";
        objective.type = ObjectiveType.Collect;
        objective.itemID = "table_item";
        objective.requiredAmount = 1;
        
        // Add objective to quest
        testQuest.objectives.Add(objective);
        
        // Set as current quest and update display
        currentQuest = testQuest;
        UpdateQuestDisplay();
        ShowQuestPanel();
        
        Debug.Log("Force showing test quest");
    }

    public void ShowActiveQuests()
    {
        if (QuestManager.Instance == null)
            return;

        List<Quest> quests = QuestManager.Instance.activeQuests;
        
        if (quests.Count > 0)
        {
            // For now, just show the first active quest
            currentQuest = quests[0];
            UpdateQuestDisplay();
            ShowQuestPanel();
        }
        else
        {
            Debug.Log("No active quests to display");
        }
    }
}
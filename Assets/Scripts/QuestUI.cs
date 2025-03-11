using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public GameObject questPanel;
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;
    public Transform objectivesContainer;
    public GameObject objectivePrefab;
    
    private Dictionary<QuestObjective, GameObject> objectiveUIElements = new Dictionary<QuestObjective, GameObject>();
    
    private void Start()
    {
        QuestManager.Instance.OnQuestStarted += HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        QuestManager.Instance.OnObjectiveUpdated += HandleObjectiveUpdated;
    }
    
    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated -= HandleObjectiveUpdated;
        }
    }
    
    private void HandleQuestStarted(Quest quest)
    {
        // Show quest panel
        questPanel.SetActive(true);
        
        // Update quest info
        questTitle.text = quest.title;
        questDescription.text = quest.description;
        
        // Clear previous objectives
        foreach (Transform child in objectivesContainer)
        {
            Destroy(child.gameObject);
        }
        objectiveUIElements.Clear();
        
        // Add new objectives
        foreach (QuestObjective objective in quest.objectives)
        {
            GameObject objectiveUI = Instantiate(objectivePrefab, objectivesContainer);
            TextMeshProUGUI objectiveText = objectiveUI.GetComponentInChildren<TextMeshProUGUI>();
            
            string progressText = objective.type == ObjectiveType.Collect ? 
                $" ({objective.currentAmount}/{objective.requiredAmount})" : "";
                
            objectiveText.text = objective.description + progressText;
            
            objectiveUIElements.Add(objective, objectiveUI);
        }
    }
    
    private void HandleQuestCompleted(Quest quest)
    {
        // Could show a completion message or reward
        Debug.Log($"Quest completed: {quest.title}");
    }
    
    private void HandleObjectiveUpdated(QuestObjective objective)
    {
        // Update the UI for this objective
        if (objectiveUIElements.TryGetValue(objective, out GameObject objectiveUI))
        {
            TextMeshProUGUI objectiveText = objectiveUI.GetComponentInChildren<TextMeshProUGUI>();
            
            string progressText = objective.type == ObjectiveType.Collect ? 
                $" ({objective.currentAmount}/{objective.requiredAmount})" : "";
                
            objectiveText.text = objective.description + progressText;
            
            // If completed, mark it visually (e.g., strikethrough or check mark)
            if (objective.isCompleted)
            {
                objectiveText.text = "✓ " + objectiveText.text;
                objectiveText.color = Color.green;
            }
        }
    }
}
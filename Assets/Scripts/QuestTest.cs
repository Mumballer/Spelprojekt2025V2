using UnityEngine;

public class QuestTest : MonoBehaviour
{
    // Set this to false to prevent auto-starting
    [SerializeField] private bool autoStartOnAwake = false;
    
    private void Start()
    {
        Debug.Log("QuestTest Start called");
        
        // We'll only auto-start if specifically enabled
        if (autoStartOnAwake)
        {
            StartTestQuest();
        }
    }
    
    public void StartTestQuest()
    {
        if (QuestManager.Instance != null)
        {
            Debug.Log("Creating test quest");
            // Create a simple quest for testing
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
            
            // Initialize the quest
            testQuest.Initialize();
            
            // Start the quest
            QuestManager.Instance.StartQuest(testQuest);
            
            // After starting the quest:
            Debug.Log("Quest started: " + testQuest.questName);
        }
        else
        {
            Debug.LogError("QuestManager instance is null in QuestTest!");
        }
    }
    
    // Button to test completing the quest
    public void CompleteQuest()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateObjective("table_item");
        }
    }
} 
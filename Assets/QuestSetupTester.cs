using UnityEngine;

public class QuestSetupTester : MonoBehaviour
{
    public string questName = "Find All Treasures";
    public string questDescription = "Collect all 3 hidden treasures";
    
    // Store reference to created quest for testing
    private Quest createdQuest;
    
    // Set this to false to prevent auto-creation
    [SerializeField] private bool autoCreateOnStart = false;
    
    void Start()
    {
        if (autoCreateOnStart)
        {
            CreateMultiObjectiveQuest();
        }
    }
    
    public void CreateMultiObjectiveQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager not found!");
            return;
        }
        
        // Create a new quest
        createdQuest = ScriptableObject.CreateInstance<Quest>();
        createdQuest.questName = questName;
        createdQuest.description = questDescription;
        
        // Create multiple objectives
        string[] itemNames = new string[] { "Golden Key", "Ancient Scroll", "Magic Amulet" };
        string[] itemIDs = new string[] { "treasure_key", "treasure_scroll", "treasure_amulet" };
        
        for (int i = 0; i < itemNames.Length; i++)
        {
            QuestObjective objective = ScriptableObject.CreateInstance<QuestObjective>();
            objective.description = $"Find the {itemNames[i]}";
            objective.type = ObjectiveType.Collect;
            objective.itemID = itemIDs[i];
            objective.requiredAmount = 1;
            
            createdQuest.objectives.Add(objective);
            Debug.Log($"Added objective: {objective.description}");
        }
        
        // Initialize and start the quest
        createdQuest.Initialize();
        QuestManager.Instance.AddQuest(createdQuest);
        
        Debug.Log($"Created multi-objective quest: {questName} with {createdQuest.objectives.Count} objectives");
    }
    
    // For debugging - call this to check the current quest status
    public void CheckQuestStatus()
    {
        if (createdQuest == null)
        {
            Debug.Log("No quest has been created yet");
            return;
        }
        
        string status = $"Quest: {createdQuest.questName}\nActive: {createdQuest.IsActive}\nCompleted: {createdQuest.IsCompleted}\n\nObjectives:";
        
        foreach (var objective in createdQuest.objectives)
        {
            status += $"\n- {objective.description}: {objective.currentAmount}/{objective.requiredAmount} ({(objective.isCompleted ? "Completed" : "Incomplete")})";
        }
        
        Debug.Log(status);
    }
} 
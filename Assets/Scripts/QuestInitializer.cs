using UnityEngine;

public class QuestInitializer : MonoBehaviour
{
    public QuestManager questManager;
    
    // Names/paths to your objective scriptable objects
    public string[] objectivePaths = new string[6]; 
    
    // Set this to false to prevent auto-creation
    [SerializeField] private bool autoCreateOnStart = false;
    
    private void Start()
    {
        if (questManager == null && QuestManager.Instance != null)
        {
            questManager = QuestManager.Instance;
        }
        
        if (autoCreateOnStart)
        {
            // This will only run if you explicitly set autoCreateOnStart to true
            Quest newQuest = ScriptableObject.CreateInstance<Quest>();
            newQuest.questName = "Treasure Hunt";
            newQuest.description = "Find all six hidden treasures scattered around the area.";
            
            // Load objectives
            for (int i = 0; i < objectivePaths.Length; i++)
            {
                if (!string.IsNullOrEmpty(objectivePaths[i]))
                {
                    QuestObjective objective = Resources.Load<QuestObjective>(objectivePaths[i]);
                    if (objective != null)
                    {
                        newQuest.objectives.Add(objective);
                    }
                }
            }
            
            // Initialize and add to manager
            newQuest.Initialize();
            questManager.availableQuests.Add(newQuest);
            
            // NOTE: We're only adding it to available quests, NOT starting it
            Debug.Log("Quest created and added to available quests (not started)");
        }
    }
} 
using UnityEngine;

public class QuestInitializer : MonoBehaviour
{
    // Set this to false to prevent auto-creation
    [SerializeField] private bool autoCreateOnStart = false;
    
    private void Start()
    {
        if (autoCreateOnStart)
        {
            // This will only run if you explicitly set autoCreateOnStart to true
            Quest newQuest = ScriptableObject.CreateInstance<Quest>();
            newQuest.questName = "Treasure Hunt";
            newQuest.description = "Find all six hidden treasures scattered around the area.";
            
            // Load objectives...
            
            // Initialize and add to manager
            newQuest.Initialize();
            QuestManager.Instance.availableQuests.Add(newQuest);
            
            // NOTE: We're only adding it to available quests, NOT starting it
            Debug.Log("Quest created and added to available quests (not started)");
        }
    }
} 
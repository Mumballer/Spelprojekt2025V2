using UnityEngine;

public class QuestInitializer : MonoBehaviour
{

    [SerializeField] private bool autoCreateOnStart = false;
    

    //gjort för att manuellt lägga till quests för bug fixing
    private void Start()
    {
        if (autoCreateOnStart)
        {

            Quest newQuest = ScriptableObject.CreateInstance<Quest>();
            newQuest.questName = "Treasure Hunt";
            newQuest.description = "Find all six hidden treasures scattered around the area.";
            

            newQuest.Initialize();
            QuestManager.Instance.availableQuests.Add(newQuest);
            

            Debug.Log("Quest created and added to available quests (not started)");
        }
    }
} 
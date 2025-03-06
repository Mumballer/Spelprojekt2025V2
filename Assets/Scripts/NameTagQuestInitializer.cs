using UnityEngine;

public class NameTagQuestInitializer : MonoBehaviour
{
    [Header("Quest References")]
    [SerializeField] private Quest nametagQuest;

    private void Start()
    {
        if (nametagQuest == null)
        {
            Debug.LogError("Nametag Quest not assigned to NameTagQuestInitializer!");
            return;
        }

        // Add the quest to the QuestManager if it's not already there
        if (QuestManager.Instance != null && !QuestManager.Instance.IsQuestActive(nametagQuest))
        {
            QuestManager.Instance.AddQuest(nametagQuest);
        }
    }
}
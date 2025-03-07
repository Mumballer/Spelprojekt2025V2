using UnityEngine;

public class PlayerQuestInitializer : MonoBehaviour
{
    [Header("Initial Quests")]
    [SerializeField] private Quest gramophoneQuest;
    [SerializeField] private bool addQuestOnStart = true;
    [SerializeField] private float startDelay = 1f; // Optional delay before adding the quest

    private void Start()
    {
        if (addQuestOnStart && gramophoneQuest != null)
        {
            // Add quest immediately or with delay
            if (startDelay <= 0)
            {
                AddQuestToPlayer();
            }
            else
            {
                Invoke(nameof(AddQuestToPlayer), startDelay);
            }
        }
    }

    private void AddQuestToPlayer()
    {
        if (QuestManager.Instance != null)
        {
            Debug.Log($"Adding gramophone quest to player: {gramophoneQuest.questName}");
            QuestManager.Instance.AddQuest(gramophoneQuest);
        }
        else
        {
            Debug.LogWarning("QuestManager instance not found. Cannot add gramophone quest.");
        }
    }
}
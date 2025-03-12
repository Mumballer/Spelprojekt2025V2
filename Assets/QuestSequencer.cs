using UnityEngine;
using System.Collections.Generic;
using static QuestManager;

public class QuestSequencer : MonoBehaviour
{
    [Header("Collection Quest Settings")]
    [SerializeField] private string collectionQuestName = "Find All Items";
    [SerializeField] private string collectionQuestDescription = "Find all the required items";
    [SerializeField] private string[] itemNames;
    [SerializeField] private string[] itemIDs;

    [Header("Placement Quest Settings")]
    [SerializeField] private string placementQuestName = "Place All Items";
    [SerializeField] private string placementQuestDescription = "Place all items in their correct spots";

    [Header("Objects")]
    [SerializeField] private QuestGiver questGiver;

    private Quest collectionQuest;
    private Quest placementQuest;

    private void Start()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager not found!");
            return;
        }

        // Create both quests
        CreateQuestSequence();

        // Assign the collection quest to the quest giver
        if (questGiver != null)
        {
            // Use reflection to set the private field
            var field = typeof(QuestGiver).GetField("questToGive",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(questGiver, collectionQuest);
            }
        }
    }

    private void CreateQuestSequence()
    {
        // Create collection quest
        collectionQuest = ScriptableObject.CreateInstance<Quest>();
        collectionQuest.questName = collectionQuestName;
        collectionQuest.description = collectionQuestDescription;

        // Add collection objectives
        for (int i = 0; i < Mathf.Min(itemNames.Length, itemIDs.Length); i++)
        {
            QuestObjective objective = ScriptableObject.CreateInstance<QuestObjective>();
            objective.description = $"Find the {itemNames[i]}";
            objective.type = ObjectiveType.Collect;
            objective.itemID = itemIDs[i];
            objective.requiredAmount = 1;

            collectionQuest.objectives.Add(objective);
        }

        // Create placement quest
        placementQuest = ScriptableObject.CreateInstance<Quest>();
        placementQuest.questName = placementQuestName;
        placementQuest.description = placementQuestDescription;

        // Add placement objectives
        for (int i = 0; i < Mathf.Min(itemNames.Length, itemIDs.Length); i++)
        {
            QuestObjective objective = ScriptableObject.CreateInstance<QuestObjective>();
            objective.description = $"Place the {itemNames[i]} in its spot";
            objective.type = ObjectiveType.PlaceItem;
            objective.itemID = "place_" + itemIDs[i];
            objective.requiredAmount = 1;

            placementQuest.objectives.Add(objective);
        }

        // Initialize both quests
        collectionQuest.Initialize();
        placementQuest.Initialize();

        // Add to available quests
        QuestManager.Instance.availableQuests.Add(collectionQuest);
        QuestManager.Instance.availableQuests.Add(placementQuest);

        // Create the follow-up relationship
        QuestPair questPair = new QuestPair
        {
            initialQuest = collectionQuest,
            followUpQuest = placementQuest
        };

        QuestManager.Instance.followUpQuests.Add(questPair);
    }
}
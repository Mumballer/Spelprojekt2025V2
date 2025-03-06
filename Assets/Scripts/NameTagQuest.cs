using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Nametag Quest", menuName = "Quests/Nametag Quest")]
public class NameTagQuest : Quest
{
    [Header("Nametag Quest Settings")]
    public string eventName = "Dinner Party";
    public string placeNametagsObjective = "Place all the nametags in the correct spots";
    public int totalNameTags = 6;
    public bool createIndividualObjectives = false;
    public List<string> guestNames = new List<string>();

    [Header("Completion Settings")]
    public bool completeWhenAllPlaced = true;
    public string nextSceneToLoad = "";
    public float sceneLoadDelay = 2.0f;

    public void SetupNameTagQuest()
    {
        // Clear existing objectives
        Objectives.Clear();

        if (createIndividualObjectives && guestNames.Count > 0)
        {
            // Add individual nametag objectives
            foreach (string guest in guestNames)
            {
                QuestObjective nametagObjective = new QuestObjective
                {
                    description = $"Place {guest}'s nametag on the table",
                    isCompleted = false
                };
                Objectives.Add(nametagObjective);
            }
        }
        else
        {
            // Add a single objective for all nametags
            QuestObjective nametagObjective = new QuestObjective
            {
                description = placeNametagsObjective,
                isCompleted = false
            };
            Objectives.Add(nametagObjective);
        }

        // Set quest name and description if not already set
        if (string.IsNullOrEmpty(questName))
        {
            questName = "Set The Table";
        }

        if (string.IsNullOrEmpty(description))
        {
            description = $"Place all the nametags correctly for the {eventName}.";
        }
    }

    // Override OnEnable to set up the quest when created
    private void OnEnable()
    {
        SetupNameTagQuest();
    }

    // Mark a specific guest's nametag as placed - for individual objectives mode
    public void MarkGuestNameTagPlaced(string guestName)
    {
        if (!createIndividualObjectives || guestNames.Count == 0)
        {
            return;
        }

        int index = -1;
        for (int i = 0; i < guestNames.Count; i++)
        {
            if (guestNames[i] == guestName)
            {
                index = i;
                break;
            }
        }

        if (index >= 0 && index < Objectives.Count)
        {
            CompleteObjective(index);
        }
    }

    // Use this when all nametags are placed (single objective mode)
    public void MarkAllNameTagsPlaced()
    {
        if (createIndividualObjectives)
        {
            // Complete all remaining objectives
            for (int i = 0; i < Objectives.Count; i++)
            {
                if (!Objectives[i].isCompleted)
                {
                    CompleteObjective(i);
                }
            }
        }
        else if (Objectives.Count > 0)
        {
            // Complete the main objective
            CompleteObjective(0);
        }

        if (completeWhenAllPlaced)
        {
            // Call the non-overridden CompleteQuest in the base class
            CompleteQuest();

            // Then handle scene transition if needed
            HandleQuestCompletion();
        }
    }

    // Handle post-completion actions (not an override)
    public void HandleQuestCompletion()
    {
        if (IsCompleted && !string.IsNullOrEmpty(nextSceneToLoad))
        {
            Debug.Log($"NameTagQuest '{questName}' completed - Scene to load: {nextSceneToLoad}");

            // Use FindFirstObjectByType instead of FindObjectOfType (fixes warning)
            var sceneLoader = Object.FindFirstObjectByType<QuestSceneLoader>();
            if (sceneLoader != null)
            {
                // Trigger scene loading check
                sceneLoader.CheckQuestAndLoadScene();
            }
        }
    }
}
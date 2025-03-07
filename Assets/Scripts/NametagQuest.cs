using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Nametag Quest", menuName = "Quests/Nametag Quest")]
public class NametagQuest : Quest
{
    [Header("Nametag Quest Settings")]
    public string[] nametagNames = new string[6] {
        "Nametag 1",
        "Nametag 2",
        "Nametag 3",
        "Nametag 4",
        "Nametag 5",
        "Nametag 6"
    };

    public void SetupNametagQuest()
    {
        // Clear existing objectives
        Objectives.Clear();

        // Set quest name and description if not already set
        if (string.IsNullOrEmpty(questName))
        {
            questName = "Prepare for Dinner";
        }

        if (string.IsNullOrEmpty(description))
        {
            description = "Place all nametags at the correct seats at the dinner table.";
        }

        // Add single counter objective
        QuestObjective objective = new QuestObjective
        {
            description = $"Place nametags at the table (0/{nametagNames.Length})",
            isCompleted = false
        };
        Objectives.Add(objective);
    }

    // Override OnEnable to set up the quest when created
    private void OnEnable()
    {
        SetupNametagQuest();
    }
}
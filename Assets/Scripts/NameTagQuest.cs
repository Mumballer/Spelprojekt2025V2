using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameTagQuest : MonoBehaviour
{
    [SerializeField] private string questName = "Set up the nametags.";
    [SerializeField] private string questDescription = "Help set up the nametags for the meeting.";
    [SerializeField] private Quest questPrefab;

    private Quest nameTagQuest;

    // References to name tags
    [SerializeField] private NameTag[] nameTags;

    // Track found name tags
    private int nameTagsFound = 0;
    private bool questCompleted = false;

    void Start()
    {
        InitializeQuest();
    }

    private void InitializeQuest()
    {
        if (questPrefab == null)
        {
            Debug.LogError("Quest prefab not assigned to NameTagQuest!");
            return;
        }

        nameTagQuest = Instantiate(questPrefab);
        nameTagQuest.questName = questName;
        nameTagQuest.description = questDescription;

        // Add objectives - fixed constructor calls with required parameters
        nameTagQuest.Objectives.Add(new QuestObjective("Find all the name tags", false));

        // Make quest available
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest(nameTagQuest);
        }
    }

    public void NameTagFound()
    {
        if (questCompleted) return;

        nameTagsFound++;
        Debug.Log($"Name tag found! Total found: {nameTagsFound}/{nameTags.Length}");

        // Check if all name tags are found
        if (nameTagsFound >= nameTags.Length)
        {
            Debug.Log("All name tags found! Quest objective complete.");
            CompleteNameTagObjective();
        }
    }

    private void CompleteNameTagObjective()
    {
        if (nameTagQuest != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteObjective(nameTagQuest, 0);
            Debug.Log("Name tag quest objective completed!");

            // Check if this completes the entire quest
            if (nameTagQuest.IsCompleted)
            {
                CompleteNameTagQuest();
            }
        }
    }

    // Fixed method to use QuestManager instead of direct call
    private void CompleteNameTagQuest()
    {
        if (nameTagQuest != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(nameTagQuest);
            questCompleted = true;
            Debug.Log("Name tag quest completed!");
        }
    }

    public Quest GetQuest()
    {
        return nameTagQuest;
    }

    public bool IsQuestCompleted()
    {
        return questCompleted || (nameTagQuest != null && nameTagQuest.IsCompleted);
    }
}
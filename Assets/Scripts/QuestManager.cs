using UnityEngine;
using System.Collections.Generic;
using System;
using static Quest;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private List<Quest> activeQuests = new List<Quest>();
    [SerializeField] private List<Quest> completedQuests = new List<Quest>();

    public event Action<Quest> OnQuestAdded;
    public event Action<Quest> OnQuestCompleted;
    public event Action<Quest, int> OnObjectiveCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        ResetAllQuests();
    }
    public void NotifyQuestCompleted(Quest quest)
    {
        // Handle quest completion logic
        Debug.Log($"QuestManager notified of quest completion: {quest.questName}");

        // If you have an event for quest completion, invoke it here
        if (OnQuestCompleted != null)
        {
            OnQuestCompleted.Invoke(quest);
        }
    }

    public void ResetAllQuests()
    {
        activeQuests.Clear();
        completedQuests.Clear();
        Quest[] allQuests = Resources.FindObjectsOfTypeAll<Quest>();
        foreach (var quest in allQuests)
        {
            quest.IsActive = false;
            quest.IsCompleted = false;

            if (quest.Objectives != null)
            {
                foreach (var objective in quest.Objectives)
                {
                    objective.isCompleted = false;
                }
            }
        }
    }

    public void AddQuest(Quest quest)
    {
        if (quest == null) return;

        if (activeQuests.Contains(quest) || completedQuests.Contains(quest))
        {
            Debug.Log($"Quest '{quest.questName}' is already in progress or completed");
            return;
        }

        activeQuests.Add(quest);
        quest.ActivateQuest();
        Debug.Log($"Added quest: {quest.questName}");

        OnQuestAdded?.Invoke(quest);
    }

    public void CompleteQuest(Quest quest)
    {
        if (quest == null) return;

        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            completedQuests.Add(quest);
            quest.CompleteQuest();

            Debug.Log($"Completed quest: {quest.questName}");

            OnQuestCompleted?.Invoke(quest);
        }
    }

    public bool IsQuestActive(Quest quest)
    {
        return quest != null && activeQuests.Contains(quest);
    }

    public bool IsQuestCompleted(Quest quest)
    {
        return quest != null && completedQuests.Contains(quest);
    }

    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(activeQuests);
    }

    public List<Quest> GetCompletedQuests()
    {
        return new List<Quest>(completedQuests);
    }

    public void CompleteObjective(Quest quest, int objectiveIndex)
    {
        if (quest == null || objectiveIndex < 0 || objectiveIndex >= quest.Objectives.Count)
        {
            Debug.LogWarning("Invalid quest or objective index");
            return;
        }

        if (!activeQuests.Contains(quest))
        {
            Debug.LogWarning($"Trying to complete objective for inactive quest: {quest.questName}");
            return;
        }

        QuestObjective objective = quest.Objectives[objectiveIndex];
        if (objective.isCompleted)
        {
            return; 
        }

        objective.isCompleted = true;

        OnObjectiveCompleted?.Invoke(quest, objectiveIndex);

        bool allCompleted = true;
        foreach (var obj in quest.Objectives)
        {
            if (!obj.isCompleted)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            CompleteQuest(quest);
        }
    }
}
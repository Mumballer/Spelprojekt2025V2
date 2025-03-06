using UnityEngine;
using System.Collections.Generic;
using System;
using static Quest;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private List<Quest> activeQuests = new List<Quest>();
    [SerializeField] private List<Quest> completedQuests = new List<Quest>();
    [SerializeField] private List<Quest> availableQuests = new List<Quest>();

    public event Action<Quest> OnQuestAdded;
    public event Action<Quest> OnQuestCompleted;
    public event Action<Quest> OnQuestRemoved;
    public event Action<Quest> OnQuestAvailable;
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

        // If the quest is in our active list, move it to completed
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            completedQuests.Add(quest);

            // Notify UI to remove this quest
            OnQuestRemoved?.Invoke(quest);
            OnQuestCompleted?.Invoke(quest);

            // Check for follow-up quests
            CheckForFollowUpQuests(quest);
        }
    }

    private void CheckForFollowUpQuests(Quest quest)
    {
        if (quest.followUpQuests == null || quest.followUpQuests.Length == 0)
            return;

        foreach (Quest followUpQuest in quest.followUpQuests)
        {
            if (followUpQuest == null) continue;

            if (followUpQuest.requiresManualAcceptance)
            {
                // Make the quest available but don't add it yet
                if (!availableQuests.Contains(followUpQuest) &&
                    !activeQuests.Contains(followUpQuest) &&
                    !completedQuests.Contains(followUpQuest))
                {
                    availableQuests.Add(followUpQuest);
                    OnQuestAvailable?.Invoke(followUpQuest);
                    Debug.Log($"Follow-up quest available: {followUpQuest.questName}");
                }
            }
            else
            {
                // Automatically add the follow-up quest
                AddQuest(followUpQuest);
                Debug.Log($"Follow-up quest automatically added: {followUpQuest.questName}");
            }
        }
    }

    public void ResetAllQuests()
    {
        activeQuests.Clear();
        completedQuests.Clear();
        availableQuests.Clear();

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

        // Remove from available quests if it was there
        if (availableQuests.Contains(quest))
        {
            availableQuests.Remove(quest);
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
            OnQuestRemoved?.Invoke(quest);

            // Check for follow-up quests
            CheckForFollowUpQuests(quest);
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

    public bool IsQuestAvailable(Quest quest)
    {
        return quest != null && availableQuests.Contains(quest);
    }

    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(activeQuests);
    }

    public List<Quest> GetCompletedQuests()
    {
        return new List<Quest>(completedQuests);
    }

    public List<Quest> GetAvailableQuests()
    {
        return new List<Quest>(availableQuests);
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

        if (quest.AreAllObjectivesCompleted())
        {
            CompleteQuest(quest);
        }
    }
}
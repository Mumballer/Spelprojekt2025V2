using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    public List<Quest> availableQuests = new List<Quest>();
    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> completedQuests = new List<Quest>();
    
    public event Action<Quest> OnQuestStarted;
    public event Action<Quest> OnQuestCompleted;
    public event Action<QuestObjective> OnObjectiveUpdated;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Initialize all quests
        foreach (Quest quest in availableQuests)
        {
            quest.Initialize();
        }
    }
    
    public void StartQuest(Quest quest)
    {
        if (activeQuests.Contains(quest) || completedQuests.Contains(quest))
            return;
            
        quest.isActive = true;
        activeQuests.Add(quest);
        OnQuestStarted?.Invoke(quest);
    }
    
    public void CompleteQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest) || completedQuests.Contains(quest))
            return;
            
        quest.isCompleted = true;
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        OnQuestCompleted?.Invoke(quest);
    }
    
    public void UpdateObjective(string itemID)
    {
        foreach (Quest quest in activeQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.Collect && objective.itemID == itemID)
                {
                    objective.UpdateProgress();
                    OnObjectiveUpdated?.Invoke(objective);
                    
                    if (quest.CheckCompletion())
                    {
                        CompleteQuest(quest);
                    }
                }
            }
        }
    }
}
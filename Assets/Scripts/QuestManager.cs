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
    
    public void AddQuest(Quest quest)
    {
        // This should only be called by QuestGiver, never automatically
        if (activeQuests.Contains(quest) || completedQuests.Contains(quest))
            return;
            
        quest.isActive = true;
        activeQuests.Add(quest);
        
        // Play sound or show effects when quest is added
        // AudioManager.Instance?.PlaySound("quest_accept");
        
        // Invoke the event for listeners (like QuestUI)
        OnQuestStarted?.Invoke(quest);
        
        Debug.Log($"Quest started: {quest.questName}");
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
    
    public void CompleteObjective(Quest quest, int objectiveIndex)
    {
        if (!activeQuests.Contains(quest))
        {
            Debug.LogWarning($"Quest {quest.questName} is not in active quests list!");
            return;
        }
        
        if (objectiveIndex < 0 || objectiveIndex >= quest.objectives.Count)
        {
            Debug.LogError($"Invalid objective index {objectiveIndex} for quest {quest.questName}!");
            return;
        }
        
        QuestObjective objective = quest.objectives[objectiveIndex];
        Debug.Log($"Completing objective: {objective.description} for quest: {quest.questName}");
        
        // Mark as complete by setting it to the required amount
        objective.UpdateProgress(objective.requiredAmount);
        OnObjectiveUpdated?.Invoke(objective);
        
        Debug.Log($"Checking if quest {quest.questName} is complete...");
        bool isComplete = quest.CheckCompletion();
        Debug.Log($"Quest completion check result: {isComplete}");
        
        if (isComplete)
        {
            Debug.Log($"Quest {quest.questName} is complete, calling CompleteQuest()");
            CompleteQuest(quest);
        }
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
    
    public bool IsQuestActive(Quest quest)
    {
        return quest != null && activeQuests.Contains(quest);
    }
    
    public bool IsQuestCompleted(Quest quest)
    {
        return quest != null && completedQuests.Contains(quest);
    }
    
    public Quest CreateAndStartQuest(string questName, string description, string itemID)
    {
        // Create a new quest
        Quest newQuest = ScriptableObject.CreateInstance<Quest>();
        newQuest.questName = questName;
        newQuest.description = description;
        
        // Create an objective
        QuestObjective objective = ScriptableObject.CreateInstance<QuestObjective>();
        objective.description = description;
        objective.type = ObjectiveType.Collect;
        objective.itemID = itemID;
        objective.requiredAmount = 1;
        
        // Add objective to quest
        newQuest.objectives.Add(objective);
        
        // Initialize the quest
        newQuest.Initialize();
        
        // Start the quest
        AddQuest(newQuest);
        
        return newQuest;
    }
}
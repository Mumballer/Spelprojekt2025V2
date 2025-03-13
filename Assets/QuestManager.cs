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

    [Header("Follow-up Quests")]
    [SerializeField] public List<QuestPair> followUpQuests = new List<QuestPair>();

    [System.Serializable]
    public class QuestPair
    {
        public Quest initialQuest;
        public Quest followUpQuest;
    }

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
        // Add detailed logging
        Debug.Log($"<color=green>QUEST SYSTEM: Adding quest '{quest.questName}'</color>");

        // Log objectives status
        if (quest.objectives != null && quest.objectives.Count > 0)
        {
            Debug.Log($"<color=green>QUEST '{quest.questName}' has {quest.objectives.Count} objectives:</color>");
            for (int i = 0; i < quest.objectives.Count; i++)
            {
                var obj = quest.objectives[i];
                Debug.Log($"<color=green>  - Objective {i}: '{obj.description}' | Completed: {obj.isCompleted}</color>");
            }
        }
        else
        {
            Debug.Log($"<color=green>QUEST '{quest.questName}' has no objectives!</color>");
        }

        // Existing code...
        activeQuests.Add(quest);
        OnQuestStarted?.Invoke(quest);

        Debug.Log($"<color=green>QUEST SYSTEM: Active quests now: {activeQuests.Count}</color>");
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

        // Check if this quest has a follow-up quest
        foreach (QuestPair pair in followUpQuests)
        {
            if (pair.initialQuest == quest && !IsQuestCompleted(pair.followUpQuest))
            {
                Debug.Log($"Starting follow-up quest: {pair.followUpQuest.questName}");
                // Start the follow-up quest after a short delay
                StartCoroutine(StartFollowUpQuestAfterDelay(pair.followUpQuest, 2.0f));
                break;
            }
        }
    }

    private IEnumerator StartFollowUpQuestAfterDelay(Quest followUpQuest, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Make sure the follow-up quest is properly initialized
        followUpQuest.isActive = false;
        followUpQuest.isCompleted = false;
        
        // IMPORTANT: Explicitly re-initialize all objectives
        foreach (QuestObjective objective in followUpQuest.objectives)
        {
            objective.currentAmount = 0;
            objective.isCompleted = false;
            Debug.Log($"Initializing follow-up objective: {objective.description}, completed: {objective.isCompleted}");
        }
        
        // Start the follow-up quest
        AddQuest(followUpQuest);
        
        // Debug log for verification
        Debug.Log($"Follow-up quest {followUpQuest.questName} started with {followUpQuest.objectives.Count} objectives:");
        foreach (QuestObjective objective in followUpQuest.objectives)
        {
            Debug.Log($"- {objective.description}: completed = {objective.isCompleted}, current = {objective.currentAmount}");
        }
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
                if ((objective.type == ObjectiveType.Collect || objective.type == ObjectiveType.PlaceItem)
                    && objective.itemID == itemID)
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

    public Quest CreatePlacementQuest(string questName, string description, int itemCount)
    {
        // Create a new quest
        Quest placementQuest = ScriptableObject.CreateInstance<Quest>();
        placementQuest.questName = questName;
        placementQuest.description = description;

        // Create placement objectives
        for (int i = 0; i < itemCount; i++)
        {
            QuestObjective objective = ScriptableObject.CreateInstance<QuestObjective>();
            objective.description = $"Place item {i + 1} in its correct spot";
            objective.type = ObjectiveType.PlaceItem;
            objective.itemID = $"place_item_{i}";
            objective.requiredAmount = 1;

            placementQuest.objectives.Add(objective);
        }

        // Initialize the quest but don't start it yet
        placementQuest.Initialize();

        return placementQuest;
    }
}
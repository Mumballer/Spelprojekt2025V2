using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private List<Quest> availableQuests = new List<Quest>();
    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> completedQuests = new List<Quest>();

    // Quest sequences for different gameplay variations
    [System.Serializable]
    public class QuestSequence
    {
        public string sequenceName;
        public List<QuestID> questOrder = new List<QuestID>();
    }

    [SerializeField] private List<QuestSequence> questSequences = new List<QuestSequence>();
    private int currentQuestIndex = 0;
    private QuestSequence activeSequence;

    [System.Serializable]
    public enum QuestID
    {
        GoToBed,
        TalkToWife,
        TalkToDaughter,
        PlayMusic,
        SetNametags
    }

    // Add these methods to your QuestManager class

    public void AddQuest(Quest quest)
    {
        // This is a compatibility method for older scripts
        if (!availableQuests.Contains(quest))
        {
            availableQuests.Add(quest);
        }
        ActivateQuest(quest.id);
    }

    
    // Add this method to your QuestManager class
    public void CompleteObjective(Quest quest, int objectiveIndex)
    {
        // This is a compatibility method for older code
        if (quest != null && IsQuestActive(quest.id))
        {
            // Log for debugging
            Debug.Log($"Completing objective {objectiveIndex} for quest {quest.questTitle}");

            // In your new system, you don't track individual objectives within a quest
            // So just complete the entire quest
            CompleteQuest(quest.id);
        }
    }

    /*public void CompleteObjective(Quest quest)
    {
        // Compatibility method for older scripts
        CompleteQuest(quest.id);
    }*/

    /*public void CompleteObjective(QuestID questID)
    {
        // Compatibility method for older scripts
        CompleteQuest(questID);
    }*/

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
    }

    // Automatically spawn if missing

    public void ActivateQuest(QuestID id)
    {
        Quest quest = availableQuests.Find(q => q.id == id);
        if (quest != null && !activeQuests.Contains(quest) && !completedQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            quest.OnActivate();
            UIManager.Instance.UpdateQuestText(quest.questDescription);

            Debug.Log($"Activated Quest: {quest.questTitle}"); // ADD THIS LINE
        }
    }


    public void CompleteQuest(QuestID id)
    {
        Quest quest = activeQuests.Find(q => q.id == id);
        if (quest != null)
        {
            activeQuests.Remove(quest);
            completedQuests.Add(quest);
            quest.OnComplete();

            Debug.Log($"Completed Quest: {quest.questTitle} with ID: {id}");

            // Move to the next quest in the sequence
            if (activeSequence != null)
            {
                Debug.Log($"Current index: {currentQuestIndex}, advancing to next quest");
                currentQuestIndex++;
                Debug.Log($"Now activating next quest in sequence at new index: {currentQuestIndex}");
                ActivateNextQuestInSequence();
            }
        }
        else
        {
            Debug.LogWarning($"Tried to complete quest with ID {id} but it's not active!");
        }
    }



    public bool IsQuestActive(QuestID id)
    {
        return activeQuests.Find(q => q.id == id) != null;
    }

    public bool IsQuestCompleted(QuestID id)
    {
        return completedQuests.Find(q => q.id == id) != null;
    }

    // Start a specific quest sequence by its index
    public void StartQuestSequence(int sequenceIndex)
    {
        if (sequenceIndex >= 0 && sequenceIndex < questSequences.Count)
        {
            // Reset any current sequence
            activeQuests.Clear();
            currentQuestIndex = 0;

            // Set the new active sequence
            activeSequence = questSequences[sequenceIndex];

            // Log the entire sequence for debugging
            Debug.Log($"=== STARTING QUEST SEQUENCE: {activeSequence.sequenceName} ===");
            for (int i = 0; i < activeSequence.questOrder.Count; i++)
            {
                Debug.Log($"  Quest {i}: {activeSequence.questOrder[i]}");
            }

            // Start the first quest in the sequence
            ActivateNextQuestInSequence();
        }
    }

    // Advance to the next quest in the active sequence
    private void ActivateNextQuestInSequence()
    {
        if (activeSequence != null && currentQuestIndex < activeSequence.questOrder.Count)
        {
            QuestID nextQuestID = activeSequence.questOrder[currentQuestIndex];
            Debug.Log($"Activating quest at index {currentQuestIndex}: {nextQuestID}");
            ActivateQuest(nextQuestID);
        }
        else if (activeSequence != null)
        {
            // All quests in the sequence are completed
            Debug.Log($"Completed all quests in sequence: {activeSequence.sequenceName}");
            UIManager.Instance.UpdateQuestText("All tasks completed!");
        }
    }
}
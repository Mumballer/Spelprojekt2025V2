using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    [Header("Quest Information")]
    public string questName;
    [TextArea(3, 5)]
    public string description;

    [Header("Quest Status")]
    [SerializeField] private bool _isActive;
    [SerializeField] private bool _isCompleted;

    [System.Serializable]
    public class QuestObjective
    {
        [TextArea(1, 3)]
        public string description;
        public bool isCompleted;
    }

    [Header("Quest Objectives")]
    [SerializeField] private List<QuestObjective> _objectives = new List<QuestObjective>();

    [Header("Follow-up Quests")]
    [Tooltip("Quests that will become available after this quest is completed")]
    public Quest[] followUpQuests;
    [Tooltip("If true, follow-up quests must be manually accepted. If false, they will be automatically added.")]
    public bool requiresManualAcceptance = true;

    public bool IsActive
    {
        get { return _isActive; }
        set { _isActive = value; }
    }

    public Quest CreateInstance()
    {
        Quest instance = Instantiate(this);
        instance.ActivateQuest();
        return instance;
    }

    public bool IsCompleted
    {
        get { return _isCompleted; }
        set { _isCompleted = value; }
    }

    public List<QuestObjective> Objectives => _objectives;

    public void ActivateQuest()
    {
        _isActive = true;
        _isCompleted = false;

        foreach (var objective in _objectives)
        {
            objective.isCompleted = false;
        }

        Debug.Log($"Quest activated: {questName}");
    }

    public void CompleteQuest()
    {
        _isActive = false;
        _isCompleted = true;

        foreach (var objective in _objectives)
        {
            objective.isCompleted = true;
        }

        Debug.Log($"Quest completed: {questName}");

        // Notify the QuestManager
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.NotifyQuestCompleted(this);
        }
    }

    public void CompleteObjective(int index)
    {
        if (index >= 0 && index < _objectives.Count)
        {
            _objectives[index].isCompleted = true;
            Debug.Log($"Completed objective {index + 1} for quest: {questName}");
            CheckQuestCompletion();
        }
    }

    public void CheckQuestCompletion()
    {
        bool allCompleted = true;
        foreach (var objective in _objectives)
        {
            if (!objective.isCompleted)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted && !_isCompleted)
        {
            CompleteQuest();
        }
    }

    public bool AreAllObjectivesCompleted()
    {
        foreach (var objective in _objectives)
        {
            if (!objective.isCompleted)
                return false;
        }
        return true;
    }

    public float GetCompletionPercentage()
    {
        if (_objectives == null || _objectives.Count == 0)
            return 0f;

        int completedCount = 0;
        foreach (var objective in _objectives)
        {
            if (objective.isCompleted)
                completedCount++;
        }

        return (float)completedCount / _objectives.Count;
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(questName))
        {
            questName = "New Quest";
        }
    }
}
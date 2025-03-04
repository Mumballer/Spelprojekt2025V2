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

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(questName))
        {
            questName = "New Quest";
        }
    }
}
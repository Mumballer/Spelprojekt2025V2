using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    [Header("Quest Info")]
    public string questName;
    public string description;

    [HideInInspector] public string QuestId => name;

    [SerializeField] private bool _isActive;
    [SerializeField] private bool _isCompleted;
    [SerializeField] private List<QuestObjective> _objectives = new List<QuestObjective>();

    // Public accessors
    public bool IsActive => _isActive;
    public bool IsCompleted => _isCompleted;
    public List<QuestObjective> Objectives => _objectives;

    // Add this new method to update objective text with progress counts
    public void UpdateObjectiveText(int objectiveIndex, string newText)
    {
        if (objectiveIndex >= 0 && objectiveIndex < _objectives.Count)
        {
            // Update the text
            _objectives[objectiveIndex].description = newText;

            // Notify QuestManager about this change to update UI
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.NotifyObjectiveUpdated(this, objectiveIndex);
            }
        }
    }

    // Complete a specific objective
    public void CompleteObjective(int objectiveIndex)
    {
        if (objectiveIndex >= 0 && objectiveIndex < _objectives.Count)
        {
            _objectives[objectiveIndex].isCompleted = true;

            // Notify QuestManager
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.CompleteObjective(this, objectiveIndex);
            }

            CheckQuestCompletion();
        }
    }

    // Check if all objectives are completed
    public void CheckQuestCompletion()
    {
        // If already completed, don't do anything
        if (_isCompleted) return;

        // Check if all objectives are completed
        bool allCompleted = true;
        foreach (var objective in _objectives)
        {
            if (!objective.isCompleted)
            {
                allCompleted = false;
                break;
            }
        }

        // If all completed, mark quest as completed
        if (allCompleted)
        {
            _isCompleted = true;

            // Notify QuestManager
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.NotifyQuestCompleted(this);
            }
        }
    }

    // Set active status
    public void SetActive(bool active)
    {
        _isActive = active;
    }

    // Reset quest
    public void ResetQuest()
    {
        _isCompleted = false;
        foreach (var objective in _objectives)
        {
            objective.isCompleted = false;
        }
    }

    // Check if quest is active in QuestManager (renamed to avoid conflict)
    public bool IsActiveInManager()
    {
        return QuestManager.Instance != null && QuestManager.Instance.IsQuestActive(this);
    }
}

[Serializable]
public class QuestObjective
{
    public string description;
    public bool isCompleted;
}
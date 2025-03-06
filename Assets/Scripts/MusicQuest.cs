using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Music Quest", menuName = "Quests/Music Quest")]
public class MusicQuest : Quest
{
    [Header("Music Quest Settings")]
    public string musicName = "Gramophone Music";
    public string startMusicObjective = "Start playing music on the gramophone";
    public string stopMusicObjective = "Stop the music on the gramophone";
    public bool requireBothStartAndStop = true;

    [Header("Completion Settings")]
    public bool completeOnMusicStart = false;
    public bool completeOnMusicStop = false;

    public void SetupMusicQuest()
    {
        // Clear existing objectives
        Objectives.Clear();

        // Add start music objective
        QuestObjective startObjective = new QuestObjective
        {
            description = startMusicObjective,
            isCompleted = false
        };
        Objectives.Add(startObjective);

        // Add stop music objective if required
        if (requireBothStartAndStop)
        {
            QuestObjective stopObjective = new QuestObjective
            {
                description = stopMusicObjective,
                isCompleted = false
            };
            Objectives.Add(stopObjective);
        }

        // Set quest name and description if not already set
        if (string.IsNullOrEmpty(questName))
        {
            questName = "Play the " + musicName;
        }

        if (string.IsNullOrEmpty(description))
        {
            description = "Find and play the " + musicName + " on the gramophone.";
        }
    }

    // Override OnEnable to set up the quest when created
    private void OnEnable()
    {
        SetupMusicQuest();
    }
}
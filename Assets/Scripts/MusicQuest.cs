using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicQuest : MonoBehaviour
{
    [SerializeField] private string questName = "Play the Gramophone Music";
    [SerializeField] private string questDescription = "Turn on the gramophone and then turn it off.";
    [SerializeField] private Quest questPrefab;

    private Quest musicQuest;

    void Start()
    {
        InitializeQuest();
    }

    private void InitializeQuest()
    {
        if (questPrefab == null)
        {
            Debug.LogError("Quest prefab not assigned to MusicQuest!");
            return;
        }

        musicQuest = Instantiate(questPrefab);
        musicQuest.questName = questName;
        musicQuest.description = questDescription;

        // Add objectives - fixed constructor calls with required parameters
        musicQuest.Objectives.Add(new QuestObjective("Turn off the gramophone", false));
        musicQuest.Objectives.Add(new QuestObjective("Turn on the gramophone", false));

        // Make quest available
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest(musicQuest);
        }
    }

    public Quest GetQuest()
    {
        return musicQuest;
    }

    public void CompleteObjective(int index)
    {
        if (musicQuest != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteObjective(musicQuest, index);
        }
    }
}
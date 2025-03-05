using UnityEngine;
using System.Collections;

public class NameTagQuestGiver : MonoBehaviour
{
    [SerializeField] private Dialog instructionsDialog;
    [SerializeField] private Dialog completionDialog;
    [SerializeField] private Quest nameTagQuest;

    private bool questCompleted = false;

    private void Start()
    {
        // Check if the quest is already completed
        if (nameTagQuest != null && nameTagQuest.IsCompleted)
        {
            questCompleted = true;
        }
    }

    public void StartNameTagQuest()
    {
        if (questCompleted)
        {
            // If quest is already completed, show completion dialog
            if (DialogManager.Instance != null && completionDialog != null)
            {
                StartCoroutine(DialogManager.Instance.ShowDialog(completionDialog));
            }
            return;
        }

        if (DialogManager.Instance != null && instructionsDialog != null)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(instructionsDialog));

            // Subscribe to dialog completion
            DialogManager.Instance.OnDialogComplete += OnDialogComplete;
        }
    }

    private void OnDialogComplete(Dialog completedDialog)
    {
        if (completedDialog == instructionsDialog)
        {
            // Unsubscribe
            DialogManager.Instance.OnDialogComplete -= OnDialogComplete;

            // Start the quest
            if (nameTagQuest != null)
            {
                // Activate the quest using the Quest ScriptableObject
                nameTagQuest.ActivateQuest();

                // If you have a QuestManager, you can add it there
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.AddQuest(nameTagQuest);
                }

                Debug.Log($"Started nametag quest: {nameTagQuest.questName}");
            }
        }
    }

    // This can be called by an event system when the quest is completed
    public void OnQuestCompleted(Quest quest)
    {
        if (quest == nameTagQuest)
        {
            questCompleted = true;
            Debug.Log("Nametag quest completed!");

            // Optionally show completion dialog
            if (DialogManager.Instance != null && completionDialog != null)
            {
                StartCoroutine(DialogManager.Instance.ShowDialog(completionDialog));
            }
        }
    }
}